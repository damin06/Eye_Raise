using DG.Tweening;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
using Util;
using Util.Math;
using UnityEngine.SocialPlatforms.Impl;

public class Eye_Brain : NetworkBehaviour
{
    #region Variables
    [Header("Agent")]
    [SerializeField] private GameObject eyeAgentObj;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float baseMoveSpeed = 3f;
    [SerializeField] private int minSplitPoint = 100;
    [SerializeField] private int minEmissionPoint = 100;
    [SerializeField] private float mergeableTime = 30f;
    [SerializeField] private float splitForce = 2f;
    [SerializeField] private float BlinkSpeed = 4.5f;
    [SerializeField] private float minBlinkDelay = 2f;
    [SerializeField] private float maxBlinkDelay = 5f;

    private bool IsEyeClosed = false;
    public float MergeableTime => mergeableTime;
    private Vector2 aimPos;
    public ulong LastHitDealerID;

    #endregion

    #region Network Variables
    private NetworkVariable<float> eyelidValue = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> TotalScore => totalScore;
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion

    #region Events
    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        username.OnValueChanged += HandleNameChanged;
        eyeColor.OnValueChanged += HandleEyeColorChanged;

        if (IsServer)
        {
            CreateAgent();
            OnPlayerSpawned?.Invoke(this);
            totalScore.OnValueChanged += HandleTotalScoreChanged;
        }

        if (IsOwner)
        {
            eyeAgents.OnListChanged += HandleAgentsListChanged;
            inputReader.MovementEvent += HandleMovement;
            inputReader.SplitEvent += HandleSplit;
            inputReader.AimPositionEvent += HandleAimPosition;
            inputReader.MouseScrollEvent += HandleMousScroll;
            inputReader.EmissionEvent += HandleEmission;

            foreach (var entity in eyeAgents)
            {
                HandleAgentsListChanged(new NetworkListEvent<EyeEntityState>
                {
                    Value = entity,
                    Type = NetworkListEvent<EyeEntityState>.EventType.Add,
                });
            }
        }

        if (IsClient)
        {
            eyeAgents.OnListChanged += (evt) =>
            {
                if (evt.Type == NetworkListEvent<EyeEntityState>.EventType.Add || evt.Type == NetworkListEvent<EyeEntityState>.EventType.Insert)
                {
                    HandleNameChanged(username.Value, username.Value);
                    HandleEyeColorChanged(eyeColor.Value, eyeColor.Value);
                }
            };

            HandleNameChanged(username.Value, username.Value);
            HandleEyeColorChanged(eyeColor.Value, eyeColor.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        username.OnValueChanged -= HandleNameChanged;
        eyeColor.OnValueChanged -= HandleEyeColorChanged;

        if (IsServer)
        {
            totalScore.OnValueChanged -= HandleTotalScoreChanged;
        }

        if (IsOwner)
        {
            eyeAgents.OnListChanged -= HandleAgentsListChanged;
            inputReader.MovementEvent -= HandleMovement;
            inputReader.SplitEvent -= HandleSplit;
            inputReader.AimPositionEvent -= HandleAimPosition;
            inputReader.MouseScrollEvent -= HandleMousScroll;
            inputReader.EmissionEvent -= HandleEmission;

        }
    }

    private void Update()
    {
        UpdateEyelidValue();
    }
    #endregion

    #region Agent Management
    public void CreateAgent(int score = 100, Vector2 pos = default)
    {
        CreateAgent(out ulong networkObjectId, score, pos);
    }

    private void CreateAgent(out ulong networkObjectId, int score = 100, Vector2 pos = default)
    {
        GameObject newAgent = Instantiate(eyeAgentObj, pos, Quaternion.identity);
        networkObjectId = default;

        if (newAgent.TryGetComponent(out NetworkObject network))
        {
            network.SpawnAsPlayerObject(OwnerClientId);
            network.TrySetParent(transform);
            newAgent.transform.position = pos;
            networkObjectId = network.NetworkObjectId;

            eyeAgents.Add(new EyeEntityState
            {
                networkObjectId = network.NetworkObjectId,
                score = score
            });

            if (newAgent.TryGetComponent(out Eye_Agent agent))
            {
                agent.score.Value = score;
            }
        }

        Log.Message($"{username} is Spawned at {newAgent.transform.position} origin pos : {pos}");
    }

    public void MergeAgent(Eye_Agent _myAgent, Eye_Agent _deadAgent)
    {
        if (!_deadAgent.NetworkObject.IsSpawned || _deadAgent == null)
            return;

        if (_deadAgent.OwnerClientId != OwnerClientId)
        {
            LastHitDealerID = _deadAgent.OwnerClientId;
        }

        _myAgent.score.Value += _deadAgent.score.Value;
        _deadAgent.eyeBrain.RemoveAgent(_deadAgent.NetworkObjectId);

        Log.Message($"Agent is Alive : {_deadAgent.NetworkObject.IsSpawned}");
        _deadAgent.NetworkObject.Despawn(true);
    }
    #endregion

    #region Skill Management

    private Coroutine ICloseEye;
    public void CloseEye(float _duration)
    {
        ICloseEye = StartCoroutine(CloseEyeRoutine(_duration));
    }

    private IEnumerator CloseEyeRoutine(float _duration)
    {
        DOTween.To(() => eyelidValue.Value, x => eyelidValue.Value = x, 1, 0.5f).OnUpdate(() =>
        {
            VolumeManager.Instance.SetEyeClosure(eyelidValue.Value);
        });
        IsEyeClosed = true;

        yield return new WaitForSeconds(_duration);

        DOTween.To(() => eyelidValue.Value, x => eyelidValue.Value = x, -1, 0.5f).OnUpdate(() =>
        {
            VolumeManager.Instance.SetEyeClosure(eyelidValue.Value);
        });
        blinkDelay = Random.Range(minBlinkDelay, maxBlinkDelay);
        IsEyeClosed = false;
    }

    [ServerRpc]
    private void SplitAgentServerRpc(Vector2 _pos)
    {
        foreach (var _agent in eyeAgents)
        {
            if (_agent.score < minSplitPoint)
                continue;

            var _agentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_agent.networkObjectId];
            if (_agentObj.TryGetComponent(out Eye_Agent _eyeAgent))
            {
                _eyeAgent.score.Value /= 2;

                CreateAgent(_eyeAgent.score.Value, _agentObj.transform.InverseTransformPoint((_pos - (Vector2)_agentObj.transform.position).normalized * 0.5f));
                //AddForceAgentClientRpc(_newAgentObjId, (_pos - (Vector2)_newAgent.transform.position) * splitForce);
            }
        }
    }

    [ClientRpc]
    private void AddForceAgentClientRpc(ulong agentId, Vector2 dir)
    {
        var _agentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[agentId];
        if (_agentObj.TryGetComponent(out Rigidbody2D _rb))
        {
            _rb.AddForce(dir, ForceMode2D.Impulse);
        }
    }

    [ServerRpc]
    private void EmissionPointServerRpc(Vector2 _pos, ulong ownerClientId)
    {
        foreach (var _agent in eyeAgents)
        {
            if (_agent.score < minEmissionPoint)
                continue;

            var _agentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_agent.networkObjectId];
            if (_agentObj.TryGetComponent(out Eye_Agent _eyeAgent))
            {
                _eyeAgent.score.Value -= 17;

                ScoreManager.Instance.SpawnPlayerPoint(ownerClientId, 13, _eyeAgent.transform.position, _pos - (Vector2)_agentObj.transform.position, eyeColor.Value);
            }
        }
    }

    #endregion

    #region Event Handlers
    private void HandleTotalScoreChanged(int previousValue, int newValue)
    {
        RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
    }

    private void HandleMousScroll(float obj) { }

    private void HandleAimPosition(Vector2 vector)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(vector);
        mousePos.z = 0;
        aimPos = mousePos;
    }

    private void HandleSplit()
    {
        SplitAgentServerRpc(aimPos);
    }

    private void HandleEmission()
    {
        EmissionPointServerRpc(aimPos, OwnerClientId);
    }

    private void HandleNameChanged(FixedString32Bytes prev, FixedString32Bytes newValue)
    {
        Eye_Agent[] _agents = transform.GetComponentsInChildren<Eye_Agent>();
        foreach (var _agent in _agents)
        {
            _agent.SetNameLabel(newValue.ToString());
        }
    }

    private void HandleEyeColorChanged(Color prev, Color newValue)
    {
        Eye_Agent[] _agents = transform.GetComponentsInChildren<Eye_Agent>();
        foreach (var _agent in _agents)
        {
            _agent.SetEyeColor(newValue);
        }
    }

    private void HandleAgentsListChanged(NetworkListEvent<EyeEntityState> evt)
    {
        if (evt.Type == NetworkListEvent<EyeEntityState>.EventType.Remove || evt.Type == NetworkListEvent<EyeEntityState>.EventType.RemoveAt)
        {
            if(eyeAgents.Count == 0)
            {
                Log.Message($"{username} is Die!");
                HandleDie();
                return;
            }
        }

        UpdateTotalScore(evt.Value);
    }
    #endregion

    #region Movement & Input
    private void HandleMovement(Vector2 movementInput)
    {
        Eye_Agent[] _agents = transform.GetComponentsInChildren<Eye_Agent>();
        foreach (var _agent in _agents)
        {
            _agent.MoveInput(movementInput * baseMoveSpeed);
        }
    }
    #endregion

    #region Score & State Management
    public void ModifySocre(ulong networkObjectId, int newScore)
    {
        for (int i = 0; i < eyeAgents.Count; i++)
        {
            if (eyeAgents[i].networkObjectId != networkObjectId)
                continue;

            eyeAgents[i] = new EyeEntityState
            {
                networkObjectId = networkObjectId,
                score = newScore
            };
        }
    }

    private float blinkDelay = 0;
    private float temp;
    private void UpdateEyelidValue()
    {
        if (IsClient && eyeAgents.Count > 0)
        {
            foreach (var agent in eyeAgents)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects[agent.networkObjectId].TryGetComponent(out Eye_Animation _animation))
                {
                    _animation.SetEyelid(eyelidValue.Value + 1);
                }
            }
        }

        if (IsOwner)
        {
            if (IsEyeClosed)
                return;

            if (blinkDelay > 0)
            {
                blinkDelay -= Time.deltaTime;
                return;
            }

            temp += Time.deltaTime * Random.Range(BlinkSpeed - 0.5f, BlinkSpeed + 0.5f);
            eyelidValue.Value = Mathf.Cos(temp);
            VolumeManager.Instance.SetEyeClosure(Util.Math.MathUtils.Remap(eyelidValue.Value, 0, 1, 1, 0.15f));

            if (temp >= Mathf.PI * 2)
            {
                blinkDelay = Random.Range(minBlinkDelay, maxBlinkDelay);
                temp = 0;
            }
        }
    }

    private void HandleDie()
    {
        totalScore.Value = 0;
        HandleDieServerRpc();
    }

    [ServerRpc]
    private void HandleDieServerRpc()
    {
        OnPlayerDeSpawned?.Invoke(this);
    }

    public void RemoveAgent(ulong networkObjectId)
    {
        for (int i = 0; i < eyeAgents.Count; i++)
        {
            if (eyeAgents[i].networkObjectId != networkObjectId)
                continue;

            eyeAgents.RemoveAt(i);
        }
    }
    #endregion

    #region Utility Methods
    public Vector2 GetCenterOfAgents()
    {
        if (eyeAgents.Count == 0) return Vector2.zero;

        Vector2 sum = Vector2.zero;
        foreach (var agent in eyeAgents)
        {
            sum += (Vector2)NetworkManager.SpawnManager.SpawnedObjects[agent.networkObjectId].transform.position;
        }
        return sum / eyeAgents.Count;
    }

    private void UpdateTotalScore(EyeEntityState value)
    {
        int newScore = 0;
        foreach (var _agents in eyeAgents)
        {
            newScore += _agents.score;
        }
        totalScore.Value = newScore;
    }

    public void SetUserName(string newUsername) => username.Value = newUsername;
    public void SetEyeColor(Color newColor) => eyeColor.Value = newColor;
    public Color GetEyeColor() => eyeColor.Value;
    public FixedString32Bytes GetUserName() => username.Value;
    #endregion
}
