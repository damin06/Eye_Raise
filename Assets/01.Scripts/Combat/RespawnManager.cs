using System.Collections;
using Unity.Netcode;
using UnityEngine;
using PlayerInGame;

public class RespawnManager : NetCodeSingleton<RespawnManager>
{
    private Coroutine delayRespawnCoroutin;
    [SerializeField] private MapRange mapRange;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Eye_Brain.OnPlayerDeSpawned += HandlePlayerDeSpawn;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            Eye_Brain.OnPlayerDeSpawned -= HandlePlayerDeSpawn;
        }
    }

    private void HandlePlayerDeSpawn(Eye_Brain player)
    {
        if (player == null)
            return;

        UserData victimUserData = ServerSingleton.Instance.NetServer.GetUserDataByClientID(player.OwnerClientId);

        // Update score first
        RankBoardBehaviour.Instance.UpdateScore(player.OwnerClientId, 0);

        // Only process killer info if there is one
        if (player.LastHitDealerID != 0)
        {
            UserData killerUserdata = ServerSingleton.Instance.NetServer.GetUserDataByClientID(player.LastHitDealerID);

            if (victimUserData != null && killerUserdata != null)
            {
                Debug.Log($"{victimUserData.username} is dead by {killerUserdata.username} [{player.LastHitDealerID}]");
            }
        }
        else if (victimUserData != null)
        {
            Debug.Log($"{victimUserData.username} died");
        }

        delayRespawnCoroutin = StartCoroutine(DelayRespawn(player));
    }


    IEnumerator DelayRespawn(Eye_Brain player)
    {
        if (player == null)
            yield break;

        yield return new WaitForSeconds(3f);

        if (player != null && player.isActiveAndEnabled)
        {
            player.CreateAgent(100, mapRange.GetRandomSpawnPos());
        }
    }
}
