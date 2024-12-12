using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class Button_Lobby : Button
{
    public Lobby Lobby { private set; get; }
    private TextMeshProUGUI lobbyName;
    private TextMeshProUGUI playerCount;

    protected override void Awake()
    {
        base.Awake();

        lobbyName = transform.Find("TMP_ServerName")?.GetComponent<TextMeshProUGUI>();
        playerCount = transform.Find("TMP_PlayerCount")?.GetComponent<TextMeshProUGUI>();

        onClick.AddListener(OnButtonClick);
    }
    
    public void SetLobby(Lobby newLobby)
    {
        Lobby = newLobby;

        lobbyName.text = Lobby.Name;
        playerCount.text = $"{Lobby.Players.Count - 1}/{Lobby.MaxPlayers - 1}";
    }

    private async void OnButtonClick()
    {
        string name = await CloudManager.Instance.LoadPlayerData<string>("name");


        try
        {
            LobbySingleton.Instance.JoinLobbyByCode(Lobby.Id, name);
            ApplicationController.Instance.StartClient(Lobby.Data["server_ip"].Value, Convert.ToUInt16(Lobby.Data["server_port"].Value));
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }
}
