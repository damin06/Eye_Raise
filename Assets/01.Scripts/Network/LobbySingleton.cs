using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbySingleton
{
    private static LobbySingleton instance;
    public static LobbySingleton Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LobbySingleton();
            }

            return instance;
        }
    }

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if(instance == null)
            instance = new LobbySingleton();

        UnityServices.InitializeAsync();
        Debug.Log("Lobby Initialized!");
    }

    public void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null)
            return;

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0)
        {
            float heartbeatTimerMax = 15;
            heartbeatTimer = heartbeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (hostLobby != null || joinedLobby == null)
            return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            float lobbyUpdateTimerMax = 1.25f;
            lobbyUpdateTimer = lobbyUpdateTimerMax;

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
        }
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers, string serverIP, string serverPort)
    {
        //AuthenticationService.Instance.SignedIn += () =>
        //{
        //    Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        //};

        //await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Lobby!!!");
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                //IsLocked = false,
                //IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "server_ip", new DataObject(DataObject.VisibilityOptions.Public, serverIP)},
                    { "server_port", new DataObject(DataObject.VisibilityOptions.Public, serverPort) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log($"Created Lobby! LobbyName : {lobby.Name} MaxPlayers : {lobby.MaxPlayers} LobbyId : {lobby.Id} LobbyCode : {lobby.LobbyCode}");

            hostLobby = lobby;
            joinedLobby = lobby;
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
            Debug.Log("Lobby Failed!");
        }

        PrintLobbies();
    }

    public async Task<List<Lobby>> GetLobbiesList()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            return queryResponse.Results;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
            return null;
        }
    }

    public async void PrintLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log($"LobbyName : {lobby.Name} LobbyMaxPlayers : {lobby.MaxPlayers}");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode, string playerName)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(playerName)
            };

            Debug.Log($"P : {joinLobbyByCodeOptions.Player.Id} A : {AuthenticationService.Instance.PlayerId}");

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby =lobby;

            Debug.Log($"Joined Lobby with code {lobbyCode}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private Player GetPlayer(string playerName)
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                    }
        };
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void UpdatePlayerName(string playerID,string newPlayerName)
    {
        //AuthenticationService.Instance.PlayerId
        LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerID, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newPlayerName)}
                    }
        });
    }

    public void UpdatePlayerName(string newPlayerName)
    {
        //AuthenticationService.Instance.PlayerId
        LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newPlayerName)}
                    }
        });
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public async void LeaveLobby(string playerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public async void KickPlayer(string playerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Created Lobby! LobbyName : {lobby.Name} MaxPlayers : {lobby.MaxPlayers} LobbyId : {lobby.Id} LobbyCode : {lobby.LobbyCode}");
        foreach(Player player in lobby.Players)
        {
            Debug.Log($" playerId : {player.Id} playerName : {player.Data["PlayerName"].Value}");
        }
    }

    public string GetLobbyData(string key)
    {
        if(joinedLobby.Data.TryGetValue(key, out var data))
        {
            return data.Value;
        }
        else
        {
            return null;
        }
    }

    public void PrintJoinedPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    public async void MigrateLobbyHost(string newHostID)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHostID
            });

            joinedLobby = hostLobby;
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning(ex);
        }
    }
}
