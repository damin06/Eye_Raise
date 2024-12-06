using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 네트워크 환경에서 서버와 클라이언트 간의 이벤트 통신을 관리하는 클래스입니다.
/// 서버-클라이언트 간 Action 호출을 안전하게 처리합니다.
/// </summary>
public class NetAction : NetworkBehaviour
{
    #region Fields
    /// <summary>
    /// 클라이언트 ID를 키로 하여 해당 클라이언트의 Action 목록을 관리하는 딕셔너리입니다.
    /// </summary>
    private Dictionary<ulong, List<Action>> subscribers = new Dictionary<ulong, List<Action>>();
    #endregion

    #region Subscription Methods
    /// <summary>
    /// 현재 클라이언트에 Action을 구독합니다.
    /// </summary>
    /// <param name="action">구독할 Action</param>
    public void Subscribe(Action action)
    {
        ulong clientId = NetworkManager.LocalClientId;
        if (!subscribers.ContainsKey(clientId))
            subscribers[clientId] = new List<Action>();

        subscribers[clientId].Add(action);
    }

    /// <summary>
    /// 현재 클라이언트에서 Action 구독을 해제합니다.
    /// </summary>
    /// <param name="action">구독 해제할 Action</param>
    public void Unsubscribe(Action action)
    {
        ulong clientId = NetworkManager.LocalClientId;
        if (subscribers.ContainsKey(clientId))
            subscribers[clientId].Remove(action);
    }
    #endregion

    #region Server-to-Client Communication
    /// <summary>
    /// 서버에서 특정 클라이언트의 구독자들을 실행합니다.
    /// </summary>
    /// <param name="targetClientId">대상 클라이언트 ID</param>
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

    /// <summary>
    /// 서버에서 모든 클라이언트의 구독자들을 실행합니다.
    /// </summary>
    [ServerRpc]
    public void InvokeAllClientsRpc()
    {
        InvokeSubscribersClientRpc();
    }
    #endregion

    #region Client-to-Server Communication
    /// <summary>
    /// 클라이언트에서 서버의 구독자들을 실행합니다.
    /// </summary>
    [ClientRpc]
    public void InvokeServerRpc()
    {
        if (IsServer && subscribers.ContainsKey(NetworkManager.ServerClientId))
        {
            foreach (var action in subscribers[NetworkManager.ServerClientId])
                action?.Invoke();
        }
    }
    #endregion

    #region Internal Methods
    /// <summary>
    /// 클라이언트의 구독자들을 실행하는 내부 메서드입니다.
    /// </summary>
    /// <param name="clientRpcParams">클라이언트 RPC 매개변수</param>
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
    #endregion
}
