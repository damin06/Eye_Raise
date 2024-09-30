using System.Collections;
using Unity.Netcode;
using UnityEngine;
using PlayerInGame;

public class RespawnManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Player.OnPlayerDeSpawned += HandlePlayerDeSpawn;
        Eye_Brain.OnPlayerSpawned += HandlePlayerDeSpawn;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        Player.OnPlayerDeSpawned -= HandlePlayerDeSpawn;
        Eye_Brain.OnPlayerSpawned -= HandlePlayerDeSpawn;
    }


    private void HandlePlayerDeSpawn(Player player)
    {
        ulong killerID = player.HealthCompo.LastHitDealerID;
        UserData killerUserdata = ServerSingleton.Instance.NetServer.GetUserDataByClientID(killerID);
        UserData victimUserData = ServerSingleton.Instance.NetServer.GetUserDataByClientID(player.OwnerClientId);

        RankBoardBehaviour.Instance.UpdateScore(player.OwnerClientId, 0);

        if (victimUserData != null)
        {
            Debug.Log($"{victimUserData.username} is dead by {killerUserdata.username} [{killerID}]");
            
            StartCoroutine(DelayRespawn(player.OwnerClientId));
        }
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

            StartCoroutine(DelayRespawn(player.OwnerClientId));
        }

    }


    IEnumerator DelayRespawn(ulong clientID)
    {
        yield return new WaitForSeconds(3f);
        ServerSingleton.Instance.NetServer.RespawnPlayer(clientID);
    }
}
