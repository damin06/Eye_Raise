using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;

public class UserData
{
    public string userId;
    public string username;
    public Color color;
}

public class NetworkServer : IDisposable
{
    public delegate void UserChanged(ulong clientID, UserData userData);

    public event UserChanged OnUserJoin;
    public event UserChanged OnUserLeft;

    private NetworkObject _playerPrefab;
    private NetworkManager _networkManager;

    private Dictionary<ulong, UserData> _clientIdToUserDataDictionary = new Dictionary<ulong, UserData>();

    public NetworkServer(NetworkObject playerPrefab)
    {
        _playerPrefab = playerPrefab;
        _networkManager = NetworkManager.Singleton;
        _networkManager.ConnectionApprovalCallback += HandleConnectionApproval;
        _networkManager.OnServerStarted += HandleServerStarted;
    }

    private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest req, 
                            NetworkManager.ConnectionApprovalResponse res)
    {
        string json = Encoding.UTF8.GetString(req.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(json);

        _clientIdToUserDataDictionary[req.ClientNetworkId] = userData;

        res.Approved = true;
        res.CreatePlayerObject = false;

        Debug.Log($"{userData.username} [ {req.ClientNetworkId} ] is logined!");
    }

    private void HandleServerStarted()
    {
        _networkManager.OnClientConnectedCallback += HandleClientConnect;
        _networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleClientConnect(ulong clientID)
    {
        RespawnPlayer(clientID);
        if (_clientIdToUserDataDictionary.TryGetValue(clientID, out UserData userData))
        {
            OnUserJoin?.Invoke(clientID, userData);
        }
    }

    private void HandleClientDisconnect(ulong clientID)
    {
        if(_clientIdToUserDataDictionary.TryGetValue(clientID, out UserData userData))
        {
            OnUserLeft?.Invoke(clientID, userData);
            _clientIdToUserDataDictionary.Remove(clientID);
        }
    }

    public void RespawnPlayer(ulong clientID)
    {
        NetworkObject instance = GameObject.Instantiate(_playerPrefab, ServerSingleton.Instance._respawnPostions._spawnPositions[UnityEngine.Random.Range(0, ServerSingleton.Instance._respawnPostions._spawnPositions.Count)], Quaternion.identity);


        instance.SpawnAsPlayerObject(clientID);

        UserData userData = _clientIdToUserDataDictionary[clientID];

        if (instance.TryGetComponent(out Eye_Brain brain))
        {
            Debug.Log($"{userData.username} is Create complete!");
            brain.SetEyeColor(userData.color);
            brain.SetUserName(userData.username);
        }
        else
        {
            Debug.LogError($"{userData.username} : create failed!");
        }
    }

    public bool OpenConnection(string ipAddress, ushort port)
    {
        UnityTransport transport = _networkManager.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);
        return _networkManager.StartServer();
    }

    public UserData GetUserDataByClientID(ulong clientID)
    {
        if (_clientIdToUserDataDictionary.TryGetValue(clientID, out UserData userData))
        {
            return userData;
        }
        else
            return null;
    }


    public void Dispose()
    {
        if (_networkManager == null) return;
        _networkManager.ConnectionApprovalCallback  -= HandleConnectionApproval;
        _networkManager.OnServerStarted             -= HandleServerStarted;
        _networkManager.OnClientConnectedCallback   -= HandleClientConnect;
        _networkManager.OnClientDisconnectCallback  -= HandleClientDisconnect;

        if(_networkManager.IsListening)  //서버가 리스닝
        {
            _networkManager.Shutdown();
        }

    }
}
