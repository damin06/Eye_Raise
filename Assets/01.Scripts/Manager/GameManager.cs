using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 게임의 전반적인 상태와 진행을 관리하는 싱글톤 매니저입니다.
/// </summary>
public class GameManager : NetCodeSingleton<GameManager>
{
    private NetworkVariable<ulong> serverId = new NetworkVariable<ulong>();
    public ulong ServerId => serverId.Value;

    [SerializeField] private PolygonCollider2D mapColider;
    public PolygonCollider2D MapColider => mapColider;

    protected override void Awake()
    {
        base.Awake();

        if (IsServer)
        {
            serverId.Value = OwnerClientId;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }
}
