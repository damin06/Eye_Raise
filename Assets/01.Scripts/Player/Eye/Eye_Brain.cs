using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Eye_Brain : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private InputReader inputReader;

    public static event Action<Eye_Brain> OnPlayerSpawned;
    public static event Action<Eye_Brain> OnPlayerDeSpawned;

    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<Color> eyeColor = new NetworkVariable<Color>();
    private NetworkList<EyeEntityState> eyeAgents = new NetworkList<EyeEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        inputReader.MovementEvent += HandleMovement;

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
            eyeAgents.OnListChanged += HandleRankListChanged;
            inputReader.MovementEvent += HandleMovement;
        }

        if (IsServer)
        {
            OnPlayerSpawned?.Invoke(this);

            totalScore.OnValueChanged += (int previousValue, int newValue) =>
            {
                RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
            };
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            eyeAgents.OnListChanged -= HandleRankListChanged;
            inputReader.MovementEvent -= HandleMovement;
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
            case NetworkListEvent<EyeEntityState>.EventType.Clear:
            case NetworkListEvent<EyeEntityState>.EventType.RemoveAt:
            case NetworkListEvent<EyeEntityState>.EventType.Remove:
            case NetworkListEvent<EyeEntityState>.EventType.Add:
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
        Eye_Agent[] _agents = transform.Find("Agents").GetComponentsInChildren<Eye_Agent>();
        Debug.Log("input Move");
        foreach (var _agent in _agents)
        {
            _agent.MoveInput(movementInput * moveSpeed);
        }
    }
}
