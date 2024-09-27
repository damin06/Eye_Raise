using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerSingleton : MonoBehaviour
{
    public static ServerSingleton Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(Instance);
    }

    public NetworkServer NetServer { get; private set; }
    public RespawnPostions _respawnPostions;

    public void StartServer(NetworkObject playerPrefab, string ipAddress, ushort port)
    {
        NetServer = new NetworkServer(playerPrefab);

        if(NetServer.OpenConnection(ipAddress, port))
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneList.Game, LoadSceneMode.Single);
            Debug.Log($"{ipAddress} : {port.ToString()} : Server launching!!");
        }
        else
        {
            Debug.LogError($"{ipAddress} : {port.ToString()} : Server launching failed!");
        }

        
    }

    private void OnDestroy()
    {
        NetServer?.Dispose();
    }
}
