using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetAction : NetworkBehaviour
{
    private Dictionary<ulong, List<Action>> subscribers = new Dictionary<ulong, List<Action>>();

    public void Subscribe(Action action)
    {
        ulong clientId = NetworkManager.LocalClientId;
        if (!subscribers.ContainsKey(clientId))
            subscribers[clientId] = new List<Action>();

        subscribers[clientId].Add(action);
    }

    public void Unsubscribe(Action action)
    {
        ulong clientId = NetworkManager.LocalClientId;
        if (subscribers.ContainsKey(clientId))
            subscribers[clientId].Remove(action);
    }

    // 서버에서 특정 클라이언트의 구독자들 실행
    [ServerRpc]
    public void InvokeClientRpc(ulong targetClientId)
    {
        InvokeSubscribersClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { targetClientId }
            }
        });
    }

    // 서버에서 모든 클라이언트의 구독자들 실행
    [ServerRpc]
    public void InvokeAllClientsRpc()
    {
        InvokeSubscribersClientRpc();
    }

    // 클라이언트에서 서버의 구독자들 실행
    [ClientRpc]
    public void InvokeServerRpc()
    {
        if (IsServer && subscribers.ContainsKey(NetworkManager.ServerClientId))
        {
            foreach (var action in subscribers[NetworkManager.ServerClientId])
                action?.Invoke();
        }
    }

    [ClientRpc]
    private void InvokeSubscribersClientRpc(ClientRpcParams clientRpcParams = default)
    {
        ulong clientId = NetworkManager.LocalClientId;
        if (subscribers.ContainsKey(clientId))
        {
            foreach (var action in subscribers[clientId])
                action?.Invoke();
        }
    }
}
