using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 네트워크 환경에서 서버와 클라이언트 간의 이벤트 통신을 관리하는 클래스입니다.
/// 서버-클라이언트 간 Action 호출을 안전하게 처리합니다.
/// </summary>
[Serializable]
public class NetAction
{
    #region Fields
    [FormerlySerializedAs("onClick")]
    [SerializeField]
    private UnityEvent m_subscribers;

    #endregion

    #region Subscription Methods
    /// <summary>
    /// 현재 클라이언트에 Action을 구독합니다.
    /// </summary>
    /// <param name="action">구독할 Action</param>
    public void Subscribe(UnityAction action)
    {
        m_subscribers.AddListener(action);
    }

    /// <summary>
    /// 현재 클라이언트에서 Action 구독을 해제합니다.
    /// </summary>
    /// <param name="action">구독 해제할 Action</param>
    public void Unsubscribe(UnityAction action)
    {
        m_subscribers.RemoveListener(action);
    }

    /// <summary>
    /// 현재 클라이언트에서 모든 구독을 해제합니다.
    /// </summary>
    public void UnsubscribeAll()
    {
        m_subscribers.RemoveAllListeners();
    }
    #endregion

    #region Server-to-Client Communication
    /// <summary>
    /// 서버에서 특정 클라이언트의 구독자들을 실행합니다.
    /// </summary>
    /// <param name="targetClientId">대상 클라이언트 ID</param>
    public void InvokeClientRpc(ulong targetClientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

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
    [ClientRpc]
    public void InvokeAllClientRpc()
    {
        InvokeSubscribersClientRpc();
    }
    #endregion

    #region Client-to-Server Communication
    /// <summary>
    /// 클라이언트에서 서버의 구독자들을 실행합니다.
    /// </summary>
    [ServerRpc]
    public void InvokeServerRpc()
    {
        m_subscribers?.Invoke();
    }
    #endregion

    #region Client-to-Client Communication
    /// <summary>
    /// 클라이언트에서 다른 클라이언트의 구독자들을 실행합니다.
    /// </summary>
    /// <param name="targetClientId">대상 클라이언트 ID</param>
    public void InvokeOtherClientRpc(ulong targetClientId)
    {
        SendToServerRpc(targetClientId);
    }

    [ServerRpc]
    private void SendToServerRpc(ulong targetClientId)
    {
        InvokeSubscribersClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { targetClientId }
            }
        });
    }
    #endregion

    #region Internal Methods
    /// <summary>
    /// 현재 클라이언트에서 구독자들을 실행합니다.
    /// </summary>
    public void Invoke()
    {
        m_subscribers?.Invoke();
    }

    /// <summary>
    /// 클라이언트의 구독자들을 실행하는 내부 메서드입니다.
    /// </summary>
    /// <param name="clientRpcParams">클라이언트 RPC 매개변수</param>
    [ClientRpc]
    private void InvokeSubscribersClientRpc(ClientRpcParams clientRpcParams = default)
    {
        m_subscribers?.Invoke();
    }
    #endregion
}