using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private Vector2 aimPos;

    [Space]

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera cam;
    public float minFov = 1f;
    public float maxFov = 5f;
    public float Fov 
    {
        get { return Fov; }
        set { Fov = Math.Clamp(value, minFov, maxFov); }
    }

    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;

    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkObject mainAgent { private set; get; }



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
            Debug.Log("Owner Spawned!");
            //cam = transform.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            cam.Priority = 15;
            eyeAgents.OnListChanged      += HandleAgentsListChanged;

            inputReader.MovementEvent    += HandleMovement;
            inputReader.SplitEvent       += HandleSplit;
            inputReader.AimPositionEvent += HandleAimPosition;
            inputReader.MouseScrollEvent += HandleMousScroll;

            foreach(var entity in eyeAgents)
            {
                HandleAgentsListChanged(new NetworkListEvent<EyeEntityState>
                {
                    Value = entity,
                    Type = NetworkListEvent<EyeEntityState>.EventType.Add,
                });
            }

            //mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[eyeAgents[0].networkObjectId];
            //cam.Follow = mainAgent.transform;
        }
    }

    private void HandleMousScroll(float obj)
    {
        Debug.Log($"Mouse Scroll : {obj}");
        Fov += obj / 100;
        cam.m_Lens.OrthographicSize += Fov;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MovementEvent    -= HandleMovement;
            inputReader.SplitEvent       -= HandleSplit;
            inputReader.AimPositionEvent -= HandleAimPosition;
            eyeAgents.OnListChanged      -= HandleAgentsListChanged;
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
        //_newAgent.transform.position = Vector3.zero;

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
                _agent.Init();
            }
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
                _agent.Init();
            }
        }
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

            //HandleAgentsListChanged(new NetworkListEvent<EyeEntityState>
            //{
            //    Value = eyeAgents[i],
            //    Type = NetworkListEvent<EyeEntityState>.EventType.Value,
            //});
            //HandleValueChangedClientRpc(i);
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

    public FixedString32Bytes GetUserName()
    {
        return username.Value;
    }

    private void HandleTotalScoreChanged(int previousValue, int newValue)
    {
        RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
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

    }

    private void HandleEyeColorChanged(Color prev , Color newValue)
    {

    }

    private void HandleAgentsListChanged(NetworkListEvent<EyeEntityState> evt)
    {
        Debug.Log("Handle List Changed!");
        UpdateTotalScore(evt.Value);
        UpdateMainAgent(evt.Value);

        switch (evt.Type)
        {
            case NetworkListEvent<EyeEntityState>.EventType.Add:
            case NetworkListEvent<EyeEntityState>.EventType.Clear:
            case NetworkListEvent<EyeEntityState>.EventType.RemoveAt:
            case NetworkListEvent<EyeEntityState>.EventType.Remove:

                break;
            case NetworkListEvent<EyeEntityState>.EventType.Value:
                //UpdateTotalScore(evt.Value);
                break;
        }
    }

    private void RemoveAgent(NetworkObject agent)
    {
        var item = eyeAgents.
    }

    private void UpdateMainAgent(EyeEntityState value)
    {
        if (mainAgent == null)
        {
            mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
            cam.Follow = mainAgent.transform;

            if (mainAgent.TryGetComponent(out Eye_Agent _agent))
            {
                //_agent.score.OnValueChanged += HandleMainAgentScoreChanged;
            }
        }
        else
        {
            if (mainAgent.gameObject.TryGetComponent(out Eye_Agent _agent) && mainAgent.NetworkObjectId != value.networkObjectId)
            {
                if (_agent.score.Value < value.score)
                {
                    if (mainAgent.TryGetComponent(out Eye_Agent _prvAgent))
                    {
                        //_prvAgent.score.OnValueChanged -= HandleMainAgentScoreChanged;
                        //_prvAgent.score.OnValueChanged = null;
                    }

                    mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
                    cam.Follow = mainAgent.transform;

                    //_agent.score.OnValueChanged += HandleMainAgentScoreChanged;
                }
            }
        }
    }
    private void UpdateTotalScore(EyeEntityState value)
    {
        int newScore = 0;
        Debug.Log("Score Updated!");

        foreach (var _agents in eyeAgents)
        {
            newScore += _agents.score;
        }

        totalScore.Value = newScore;
        AdjustZoom(totalScore.Value);
    }

    private void AdjustZoom(int newValue)
    {
        cam.m_Lens.OrthographicSize = (float)((double)newValue / (double)100) * 14;
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
