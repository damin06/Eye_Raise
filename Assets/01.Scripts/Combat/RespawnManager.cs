using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Player에 있는  OnPlayerDespawn을 구독하고
        // 또 구독해제도 해야해. 누가? 서버만 해야해
        if (!IsServer) return;
        Player.OnPlayerDeSpawned += HandlePlayerDeSpawn;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        Player.OnPlayerDeSpawned -= HandlePlayerDeSpawn;
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
            
            //실제로 서버에서 3초후 리스폰 되도록 함수를 만들어
            StartCoroutine(DelayRespawn(player.OwnerClientId));
        }

    }


    IEnumerator DelayRespawn(ulong clientID)
    {
        yield return new WaitForSeconds(3f);
        ServerSingleton.Instance.NetServer.RespawnPlayer(clientID);
    }
}
