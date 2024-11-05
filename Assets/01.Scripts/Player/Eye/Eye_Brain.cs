using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Eye_Brain : NetworkBehaviour
{
    [Header("Agent")]
    [SerializeField] private GameObject eyeAgentObj;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int minSplitPoint = 100;
    [SerializeField] private float mergeableTime = 30f;
    public float MergeableTime => mergeableTime;
    private Vector2 aimPos;
    public ulong LastHitDealerID;
    private Eye_Camera eyeCamera;
    public NetworkObject mainAgent { private set; get; }

    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;

    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> TotalScore => totalScore;
    public NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
            eyeAgents.OnListChanged      += HandleAgentsListChanged;

            inputReader.MovementEvent    += HandleMovement;
            inputReader.SplitEvent       += HandleSplit;
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
        if (IsOwner)
        {
            inputReader.MovementEvent    -= HandleMovement;
            inputReader.SplitEvent       -= HandleSplit;
            inputReader.AimPositionEvent -= HandleAimPosition;
            eyeAgents.OnListChanged      -= HandleAgentsListChanged;
            inputReader.MouseScrollEvent -= HandleMousScroll;
        }

        if (IsServer)
        {
            OnPlayerDeSpawned?.Invoke(this);
            totalScore.OnValueChanged -= HandleTotalScoreChanged;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socre"></param>
    /// <param name="pos">pos must be localPosition</param>
    private void CreateAgent(int socre = 100, Vector3 pos = default)
    {
        GameObject _newAgent = Instantiate(eyeAgentObj);

        if (_newAgent.TryGetComponent(out NetworkObject _network))
        {
            _network.SpawnAsPlayerObject(OwnerClientId);
            _network.TrySetParent(transform);
            _newAgent.transform.localPosition = pos;

            eyeAgents.Add(new EyeEntityState
            {
                networkObjectId = _network.NetworkObjectId,
                score = socre
            });

            if(_newAgent.TryGetComponent(out Eye_Agent _agent))
            {
                _agent.score.Value = socre;
                _agent.SetEyeColor(eyeColor.Value);
                _agent.Init();
            }

 
            eyeColor.Value = eyeColor.Value;
            username.Value = username.Value;
        }
    }

    private void CreateAgent(out ulong networkObjectId, int socre = 100, Vector3 pos = default)
    {
        GameObject _newAgent = Instantiate(eyeAgentObj);
        //_newAgent.transform.position = Vector3.zero;
        networkObjectId = default;

        if (_newAgent.TryGetComponent(out NetworkObject _network))
        {
            _network.SpawnAsPlayerObject(OwnerClientId);
            _newAgent.transform.position = pos;
            _network.TrySetParent(transform);
            networkObjectId = _network.NetworkObjectId;

            eyeAgents.Add(new EyeEntityState
            {
                networkObjectId = _network.NetworkObjectId,
                score = socre
            });

            if (_newAgent.TryGetComponent(out Eye_Agent _agent))
            {
                _agent.score.Value = socre;
                _agent.SetEyeColor(eyeColor.Value);
                _agent.Init();
            }
        }

        HandleNameChanged(username.Value, username.Value);
    }

    [ServerRpc]
    private void SplitAgentServerRpc(Vector2 _pos)
    {
        foreach(var _agent in eyeAgents)
        {
            if (_agent.score < minSplitPoint)
                continue;

            var _agentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_agent.networkObjectId];
            if(_agentObj.TryGetComponent(out Eye_Agent _eyeAgent))
            {
                _eyeAgent.score.Value /= 2;
                Vector2 _spawnPos = _eyeAgent.transform.InverseTransformPoint((Vector2)_eyeAgent.transform.position + _pos * 2.5f);
                ulong _newAgentObjId;
                //CreateAgent(out _newAgentObjId, _eyeAgent.score.Value, _spawnPos);
                CreateAgent(out _newAgentObjId, _eyeAgent.score.Value, _agentObj.transform.position);

                var _newAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_newAgentObjId];
                if(_newAgent.TryGetComponent(out Rigidbody2D _rb))
                {
                    _rb.AddForce(_pos, ForceMode2D.Impulse);
                    Debug.Log("AddForce" + _pos);
                    Debug.Log($"Original AgentPos : {_agentObj.transform.position} New AgentPos : {_newAgent.transform.position} Distance : {Vector2.Distance(_agentObj.transform.position, _newAgent.transform.position)}");
                }

            }
        }
    }

    public void ModifySocre(ulong networkObjectId, int newScore)
    {
        Debug.Log($"ModifyScore : {eyeAgents.Count}");

        for (int i = 0; i < eyeAgents.Count; i++)
        {
            if (eyeAgents[i].networkObjectId != networkObjectId)
                continue;

            Debug.Log("ModifyScore");
            eyeAgents[i] = new EyeEntityState
            {
                networkObjectId = networkObjectId,
                score = newScore
            };
        }
    }

    public void SetUserName(string newUsername)
    {
        username.Value = newUsername;
    }

    public void SetEyeColor(Color newColor)
    {
        eyeColor.Value = newColor;
    }

    public Color GetEyeColor()
    {
        return eyeColor.Value;
    }

    public FixedString32Bytes GetUserName()
    {
        return username.Value;
    }

    private void HandleTotalScoreChanged(int previousValue, int newValue)
    {
        RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
    }

    private void HandleMousScroll(float obj)
    {
        //Debug.Log($"Mouse Scroll : {obj}");

    }

    private void HandleAimPosition(Vector2 vector)
    {
        //Vector3 mouseWorldPosition = Camera.main.WorldToScreenPoint(vector);
        //aimPos = (mouseWorldPosition - Camera.main.ViewportToWorldPoint(Vector3.one / 2)).normalized;

        Vector3 mousePos = Camera.main.ViewportToWorldPoint(vector);
        mousePos.z = 0;
        aimPos = (mousePos - mainAgent.transform.position).normalized;
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

    private void HandleEyeColorChanged(Color prev , Color newValue)
    {
        foreach(var agent in eyeAgents)
        {

        }
    }

    private void HandleAgentsListChanged(NetworkListEvent<EyeEntityState> evt)
    {
        if(evt.Type == NetworkListEvent<EyeEntityState>.EventType.Remove && eyeAgents.Count <= 0)
        {
            HandleDie();
        }

        UpdateTotalScore(evt.Value);
        UpdateMainAgent(evt.Value);
    }

    private void HandleDie()
    {
        OnPlayerDeSpawned?.Invoke(this);
        NetworkObject.Despawn(true);
    }

    public void RemoveAgent(ulong networkObjectId)
    {
        for(int i = 0; i < eyeAgents.Count; i++)
        {
            if (eyeAgents[i].networkObjectId != networkObjectId)
                continue;

            eyeAgents.RemoveAt(i);
        }
    }

    public Vector2 GetCenterOfAgents()
    {
        Vector2 centerPoint = Vector2.zero;

        foreach (var agent in eyeAgents)
        {
            centerPoint += (Vector2)NetworkManager.SpawnManager.SpawnedObjects[agent.networkObjectId].transform.position;
        }

        centerPoint /= eyeAgents.Count;

        return centerPoint;
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
            //cam.Follow = mainAgent.transform;
        }
        else
        {
            if (mainAgent.gameObject.TryGetComponent(out Eye_Agent _agent) && mainAgent.NetworkObjectId == value.networkObjectId)
                return;

            if (_agent.score.Value > value.score)
                return;

            mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
            //cam.Follow = mainAgent.transform;
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
        //AdjustZoom(totalScore.Value);
    }

    private void HandleMovement(Vector2 movementInput)
    {
        Eye_Agent[] _agents = transform.GetComponentsInChildren<Eye_Agent>();
        foreach (var _agent in _agents)
        {
            _agent.MoveInput(movementInput * moveSpeed);
        }
    }
}
