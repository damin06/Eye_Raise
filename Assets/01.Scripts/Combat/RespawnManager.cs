using System.Collections;
using Unity.Netcode;
using UnityEngine;
using PlayerInGame;

public class RespawnManager : NetCodeSingleton<RespawnManager>
{
    [SerializeField] private MapRange mapRange;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Eye_Brain.OnPlayerDeSpawned += HandlePlayerDeSpawn;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        Eye_Brain.OnPlayerDeSpawned -= HandlePlayerDeSpawn;
    }

    private void HandlePlayerDeSpawn(Eye_Brain player)
    {
        ulong killerID = player.LastHitDealerID;
        UserData killerUserdata = ServerSingleton.Instance.NetServer.GetUserDataByClientID(killerID);
        UserData victimUserData = ServerSingleton.Instance.NetServer.GetUserDataByClientID(player.OwnerClientId);

        RankBoardBehaviour.Instance.UpdateScore(player.OwnerClientId, 0);

        if (victimUserData != null)
        {
            Debug.Log($"{victimUserData.username} is dead by {killerUserdata.username} [{killerID}]");

            StartCoroutine(DelayRespawn(player));
        }

    }


    IEnumerator DelayRespawn(Eye_Brain player)
    {
        yield return new WaitForSeconds(3f);
        player.CreateAgent(100, mapRange.GetRandomSpawnPos());
    }
}
