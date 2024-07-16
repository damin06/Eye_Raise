using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private ServerSingleton _serverPrefab;
    [SerializeField] private ClientSingleton _clientPrefab;

    [SerializeField] private string _ipAddress;
    [SerializeField] private ushort _port;

    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //LaunchByMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            StartServer();
        }
    }

    private void StartServer()
    {
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress address in ipEntry.AddressList)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                _ipAddress = address.ToString();
                break;
            }
        }
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

    public void StartGame()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return;
        _ipAddress = inputField.text;


        ClientSingleton client = Instantiate(_clientPrefab, transform);
        client.CreateClient(_ipAddress, _port);

        SceneManager.LoadScene(SceneList.Menu);
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
