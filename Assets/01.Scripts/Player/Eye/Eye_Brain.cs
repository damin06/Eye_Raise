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
    [SerializeField] private GameObject eyeAgentObj;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private InputReader inputReader;

    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;

    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkObject mainAgent;

    [SerializeField] private CinemachineVirtualCamera cam;

    private void Awake()
    {
 
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
            //cam = transform.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            cam.Priority = 15;
            inputReader.MovementEvent += HandleMovement;
            eyeAgents.OnListChanged += HandleRankListChanged;


            //mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[eyeAgents[0].networkObjectId];
            //cam.Follow = mainAgent.transform;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MovementEvent -= HandleMovement;
            eyeAgents.OnListChanged -= HandleRankListChanged;
        }

        if (IsServer)
        {
            OnPlayerDeSpawned?.Invoke(this);

            totalScore.OnValueChanged -= HandleTotalScoreChanged;
        }
    }

    private void HandleTotalScoreChanged(int previousValue, int newValue)
    {
        RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
    }

    private void CreateAgent(int socre = 100)
    {
        GameObject _newAgent = Instantiate(eyeAgentObj, transform.Find("Agents"));
        _newAgent.transform.position = Vector3.zero;

        if (_newAgent.TryGetComponent(out NetworkObject _network))
        {
            _network.SpawnAsPlayerObject(OwnerClientId);
            _network.TrySetParent(transform);
      
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

    public void ModifySocre(ulong networkObjectId, int newScore)
    {
        for(int i = 0; i < eyeAgents.Count; i++)
        {
            if (eyeAgents[i].networkObjectId != NetworkObjectId)
                continue;

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

    public FixedString32Bytes GetUserName()
    {
        return username.Value;
    }


    private void HandleNameChanged(FixedString32Bytes prev, FixedString32Bytes newValue)
    {

    }

    private void HandleEyeColorChanged(Color prev , Color newValue)
    {

    }

    private void HandleRankListChanged(NetworkListEvent<EyeEntityState> evt)
    {
        UpdateTotalScore(evt.Value);

        //switch (evt.Type)
        //{
        //    case NetworkListEvent<EyeEntityState>.EventType.Add:
        //    case NetworkListEvent<EyeEntityState>.EventType.Clear:
        //    case NetworkListEvent<EyeEntityState>.EventType.RemoveAt:
        //    case NetworkListEvent<EyeEntityState>.EventType.Remove:
        //    case NetworkListEvent<EyeEntityState>.EventType.Value:
        //        //UpdateTotalScore(evt.Value);
        //        break;
        //}
    }

    public void UpdateTotalScore(EyeEntityState value)
    {
        int newScore = 0;
        Debug.Log("Score Updated!");

        foreach (var _agents in eyeAgents)
        {
            newScore += _agents.score;
        }

        totalScore.Value = newScore;

        if(mainAgent == null)
        {
            mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
            cam.Follow = mainAgent.transform;
            
            if(mainAgent.TryGetComponent(out Eye_Agent _agent))
            {
                _agent.score.OnValueChanged += HandleMainAgentScoreChanged;
            }
        }
        else
        {
            if(mainAgent.gameObject.TryGetComponent(out Eye_Agent _agent) && mainAgent.NetworkObjectId != value.networkObjectId)
            {
                if(_agent.score.Value < value.score)
                {
                    if (mainAgent.TryGetComponent(out Eye_Agent _prvAgent))
                    {
                        _prvAgent.score.OnValueChanged -= HandleMainAgentScoreChanged;
                    }

                    mainAgent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[value.networkObjectId];
                    cam.Follow = mainAgent.transform;

                    _agent.score.OnValueChanged += HandleMainAgentScoreChanged;
                }
            }
        }

    }

    private void HandleMainAgentScoreChanged(int previousValue, int newValue)
    {
        cam.m_Lens.OrthographicSize = (float)((double)newValue / (double)100) * 10;
    }

    private void HandleMovement(Vector2 movementInput)
    {
        Eye_Agent[] _agents = transform.GetComponentsInChildren<Eye_Agent>();
        Debug.Log("input Move");
        foreach (var _agent in _agents)
        {
            _agent.MoveInput(movementInput * moveSpeed);
        }
    }
}
