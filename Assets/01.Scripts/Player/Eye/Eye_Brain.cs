using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Eye_Brain : NetworkBehaviour
{
    #region Variables
    [Header("Agent")]
    [SerializeField] private GameObject eyeAgentObj;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float baseMoveSpeed = 3f;
    [SerializeField] private int minSplitPoint = 100;
    [SerializeField] private float mergeableTime = 30f;
    [SerializeField] private float splitForce = 2f;

    public float MergeableTime => mergeableTime;
    private Vector2 aimPos;
    public ulong LastHitDealerID;
    private Eye_Camera eyeCamera;
    public NetworkObject mainAgent { private set; get; }
    #endregion

    #region Network Variables
    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> TotalScore => totalScore;
    public NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion

    #region Events
    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        eyeCamera = GetComponent<Eye_Camera>();
    }

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

            foreach (var entity in eyeAgents)
            {
                HandleAgentsListChanged(new NetworkListEvent<EyeEntityState>
                {
                    Value = entity,
                    Type = NetworkListEvent<EyeEntityState>.EventType.Add,
                });
            }
            UpdateMainAgent(eyeAgents[0]);
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
        }

        if (IsServer)
        {
            OnPlayerDeSpawned?.Invoke(this);
        }
    }
    #endregion

    #region Agent Management
    public void CreateAgent(int score = 100, Vector3 pos = default)
    {
        CreateAgent(out ulong networkObjectId, score, pos);
    }

    private void CreateAgent(out ulong networkObjectId, int score = 100, Vector3 pos = default)
    {
        GameObject newAgent = Instantiate(eyeAgentObj, transform);
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

        HandleNameChanged(username.Value, username.Value);
    }
    #endregion

    #region Split Management
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

                ulong _newAgentObjId;
                CreateAgent(out _newAgentObjId, _eyeAgent.score.Value, _agentObj.transform.position);

                var _newAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_newAgentObjId];
                AddForceAgentClientRpc(_newAgentObjId, _pos - (Vector2)_newAgent.transform.position, splitForce);
            }
        }
    }

    [ClientRpc]
    private void AddForceAgentClientRpc(ulong agentId, Vector2 dir, float force)
    {
        var _agentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[agentId];
        if (_agentObj.TryGetComponent(out Rigidbody2D _rb))
        {
            _rb.AddForce(dir * force, ForceMode2D.Impulse);
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
        foreach (var agent in eyeAgents)
        {
            // Color change logic
        }
    }

    private void HandleAgentsListChanged(NetworkListEvent<EyeEntityState> evt)
    {
        if (evt.Type == NetworkListEvent<EyeEntityState>.EventType.Remove && eyeAgents.Count <= 0)
        {
            HandleDie();
            return;
        }

        UpdateTotalScore(evt.Value);
        UpdateMainAgent(evt.Value);
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

    private void HandleDie()
    {
        totalScore.Value = 0;
        OnPlayerDeSpawned?.Invoke(this);
        NetworkObject.Despawn(true);
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

    private void UpdateMainAgent(EyeEntityState value)
    {
        if (mainAgent == null)
        {
            var newMainAgent = eyeAgents[0];

            foreach (var agent in eyeAgents)
            {
                if (agent.score < newMainAgent.score)
                    continue;

                newMainAgent = agent;
            }

            mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[newMainAgent.networkObjectId];
        }
        else
        {
            if (mainAgent.gameObject.TryGetComponent(out Eye_Agent _agent) && mainAgent.NetworkObjectId == value.networkObjectId)
                return;

            if (_agent.score.Value > value.score)
                return;

            mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
        }
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
