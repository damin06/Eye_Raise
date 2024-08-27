using Cinemachine;
using System;
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
    private NetworkList<EyeEntityState> eyeAgents;
         //= new NetworkList<EyeEntityState>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private void Awake()
    {
        eyeAgents = new NetworkList<EyeEntityState>();
    }

    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        username.OnValueChanged += HandleNameChanged;
        eyeColor.OnValueChanged += HandleEyeColorChanged;

        if (IsOwner)
        {
            inputReader.MovementEvent += HandleMovement;

            if(transform.Find("Virtual Camera").TryGetComponent(out CinemachineVirtualCamera _camera))
            {
                _camera.Priority = 15;
            }
        }

        if (IsClient)
        {
            Debug.Log("Client!");
            eyeAgents.OnListChanged += HandleRankListChanged;
        }

        if (IsServer)
        {
            CreateAgent();
            OnPlayerSpawned?.Invoke(this);

            totalScore.OnValueChanged += (int previousValue, int newValue) =>
            {
                RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
            };
        }

    }

    private void CreateAgent(int socre = 100)
    {
        GameObject _newAgent = Instantiate(eyeAgentObj, transform.Find("Agents"));
        _newAgent.transform.position = Vector3.zero;


        if (_newAgent.TryGetComponent(out NetworkObject _network))
        {
            _network.SpawnAsPlayerObject(OwnerClientId);
            _network.TrySetParent(transform);

            if(_newAgent.TryGetComponent(out Eye_Agent _agent))
            {
                _agent.score.Value = socre;
            }

            //_network.ChangeOwnership(OwnerClientId);


            eyeAgents.Add(new EyeEntityState
            {
                networkObjectId = _network.NetworkObjectId,
                score = socre
            });
        }

    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MovementEvent -= HandleMovement;
        }

        if (IsClient)
        {
            eyeAgents.OnListChanged -= HandleRankListChanged;
        }

        if (IsServer)
        {
            OnPlayerDeSpawned?.Invoke(this);

            totalScore.OnValueChanged -= (int previousValue, int newValue) =>
            {
                RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
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
        switch (evt.Type)
        {
            case NetworkListEvent<EyeEntityState>.EventType.Add:
            case NetworkListEvent<EyeEntityState>.EventType.Clear:
            case NetworkListEvent<EyeEntityState>.EventType.RemoveAt:
            case NetworkListEvent<EyeEntityState>.EventType.Remove:
            case NetworkListEvent<EyeEntityState>.EventType.Value:
                UpdateTotalScore();
                break;
        }
    }

    public void UpdateTotalScore()
    {
        int newScore = 0;

        foreach (var _agents in eyeAgents)
        {
            newScore += _agents.score;
        }

        totalScore.Value = newScore;
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
