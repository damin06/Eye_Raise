using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton _instance;
    public static ServerSingleton Instance
    {
        get {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<ServerSingleton>();

            if(_instance == null)
            {
                Debug.LogError("Server singleton does not exists");
            }
            return _instance;
        }
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
