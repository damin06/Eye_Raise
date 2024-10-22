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

    protected override void Start()
    {
        base.Start();

        lobbyName = transform.Find("TMP_ServerName")?.GetComponent<TextMeshProUGUI>();
        playerCount = transform.Find("TMP_PlayerCount")?.GetComponent<TextMeshProUGUI>();

        onClick.AddListener(OnButtonClick);
    }
    
    public void SetLobby(Lobby newLobby)
    {
        Lobby = newLobby;

        lobbyName.text = newLobby.Name;
        playerCount.text = $"{newLobby.Players.Count}/{newLobby.MaxPlayers}";
    }

    private async void OnButtonClick()
    {
        string name = await CloudManager.Instance.LoadPlayerData<string>("name");

        try
        {
            LobbySingleton.Instance.JoinLobbyByCode(Lobby.Id, name);
            ApplicationController.Instance.StartClient(Lobby.Data["server_ip"].Value);
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }
}
