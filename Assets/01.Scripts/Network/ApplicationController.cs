using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ApplicationController : MonoBehaviour
{
    public static ApplicationController Instance { get; private set; }

    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private ServerSingleton _serverPrefab;
    [SerializeField] private ClientSingleton _clientPrefab;

    [SerializeField] private string _ipAddress;
    [SerializeField] private ushort _port;

    [SerializeField] private TMP_InputField inputField;

    private async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(gameObject);

        await UnityServices.InitializeAsync();

        //LaunchByMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            StartServer();
        }
    }

    private void Update()
    {
        LobbySingleton.Instance.Update();
    }

    private async void StartServer()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress address in ipEntry.AddressList)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                _ipAddress = address.ToString();
                break;
            }
        }

        var lobbies = await LobbySingleton.Instance.GetLobbiesList();


        ////무료버전 요금 한도 ㅠㅠ
        //await Task.Delay(1100);

        await LobbySingleton.Instance.CreateLobby($"Lobby{lobbies.Count + 1}", 51, _ipAddress, _port.ToString());
        //await LobbySingleton.Instance.CreateLobby($"Lobby Test", 50, _ipAddress, _port.ToString());

        Debug.Log($"current IP ({_ipAddress})");
        ServerSingleton server = Instantiate(_serverPrefab, transform);
        server.StartServer(_playerPrefab, _ipAddress, _port);
        NetworkManager.Singleton.SceneManager.LoadScene(SceneList.Game, LoadSceneMode.Single);
    }

    private string GetLocalIP()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress address in host.AddressList)
        {
            //인터넷트워크 IP의 경우만
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return address.ToString();
            }
        }
        return string.Empty;
    }

    public async void StartClient(string ipAddress)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return;
        _ipAddress = ipAddress;


        ClientSingleton client = Instantiate(_clientPrefab, transform);
        client.CreateClient(_ipAddress, _port);

        UserData userData = new UserData
        {
            username = await CloudManager.Instance.LoadPlayerData<string>("name"),
            userId = AuthenticationService.Instance.PlayerId,
            color = Random.ColorHSV()
        };

        ClientSingleton.Instance.StartClient(userData);
        //SceneManager.LoadScene(SceneList.Menu);
    }

    private void LaunchByMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            string ipAdress = GetLocalIP();
            if (!string.IsNullOrEmpty(ipAdress))
            {
                _ipAddress = ipAdress;
            }
            //서버 만들어주고.
            ServerSingleton server = Instantiate(_serverPrefab, transform);
            server.StartServer(_playerPrefab, _ipAddress, _port);
        }
        else
        {
            //클라이언트 만들어주고
            ClientSingleton client = Instantiate(_clientPrefab, transform);
            client.CreateClient(_ipAddress, _port);

            SceneManager.LoadScene(SceneList.Menu);
        }
    }
}
