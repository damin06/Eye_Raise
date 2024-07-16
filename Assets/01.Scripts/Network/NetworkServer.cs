using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;

public class UserData
{
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
    //여기에 아이디로 플레이어를 찾을 수 있는 것도 만들어야 한다.

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
        //여기서 위쪽에 플레이어를 스폰하는 코드를 잘 참조해서 리스폰 해주면 된다.
        // 기본 리스폰은 Vector3.zero에서 하되, 플러스로 해보고 싶은 사람들은
        // 스폰포인트를 랜덤으로 만들어서 그중에 한 곳에서 나오게 해봐라.
        //여기까지 오면 플레이어를 생성할 준비가 끝난거다.
        NetworkObject instance = GameObject.Instantiate(_playerPrefab, ServerSingleton.Instance._respawnPostions._spawnPositions[UnityEngine.Random.Range(0, ServerSingleton.Instance._respawnPostions._spawnPositions.Count)], Quaternion.identity);

        //자 1번과제 여기서 PlayerPrefab을 만들고 알맞게 오너쉽을 설정하세요.
        instance.SpawnAsPlayerObject(clientID);

        UserData userData = _clientIdToUserDataDictionary[clientID];

        if (instance.TryGetComponent<Player>(out Player player))
        {
            Debug.Log($"{userData.username} is Create complete!");
            player.SetUserName(userData.username);
            player.SetEyeColor(userData.color);
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
