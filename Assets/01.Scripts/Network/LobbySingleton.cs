using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Unity Gaming Services의 Lobby 기능을 관리하는 싱글톤 클래스입니다.
/// 로비 생성, 참가, 업데이트 및 플레이어 관리 기능을 제공합니다.
/// </summary>
public class LobbySingleton
{
    #region Singleton
    private static LobbySingleton instance;
    public static LobbySingleton Instance => instance ??= new LobbySingleton();
    #endregion

    #region Private Fields
    private Lobby hostLobby;    // 호스트로 있는 로비
    private Lobby joinedLobby;  // 참가한 로비
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private const float HEARTBEAT_TIMER_MAX = 15f;
    private const float LOBBY_UPDATE_TIMER_MAX = 1.25f;
    #endregion

    #region Initialization
    /// <summary>
    /// Unity 실행 시 자동으로 호출되어 로비 서비스를 초기화합니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        instance ??= new LobbySingleton();
        UnityServices.InitializeAsync();
        Debug.Log("Lobby Service Initialized!");
    }
    #endregion

    #region Update Methods
    /// <summary>
    /// 로비 상태를 주기적으로 업데이트합니다.
    /// </summary>
    public void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    /// <summary>
    /// 호스트 로비의 연결 상태를 유지하기 위한 하트비트를 전송합니다.
    /// </summary>
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null) return;

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0)
        {
            heartbeatTimer = HEARTBEAT_TIMER_MAX;
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    /// <summary>
    /// 참가한 로비의 정보를 주기적으로 업데이트합니다.
    /// </summary>
    private async void HandleLobbyPollForUpdates()
    {
        if (hostLobby != null || joinedLobby == null) return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            lobbyUpdateTimer = LOBBY_UPDATE_TIMER_MAX;
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        }
    }
    #endregion

    #region Lobby Creation & Management
    /// <summary>
    /// 새로운 로비를 생성합니다.
    /// </summary>
    /// <param name="lobbyName">로비 이름</param>
    /// <param name="maxPlayers">최대 플레이어 수</param>
    /// <param name="serverIP">서버 IP 주소</param>
    /// <param name="serverPort">서버 포트</param>
    public async Task CreateLobby(string lobbyName, int maxPlayers, string serverIP, string serverPort)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "server_ip", new DataObject(DataObject.VisibilityOptions.Public, serverIP)},
                    { "server_port", new DataObject(DataObject.VisibilityOptions.Public, serverPort) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Created Lobby! Name: {lobby.Name}, MaxPlayers: {lobby.MaxPlayers}, ID: {lobby.Id}, Code: {lobby.LobbyCode}");

            hostLobby = joinedLobby = lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
        }
    }

    /// <summary>
    /// 사용 가능한 로비 목록을 조회합니다.
    /// </summary>
    public async Task<List<Lobby>> GetLobbiesList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
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

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(options);
            return response.Results;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to get lobbies: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region Player Management
    /// <summary>
    /// 로비 코드를 사용하여 로비에 참가합니다.
    /// </summary>
    public async void JoinLobbyByCode(string lobbyCode, string playerName)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(playerName)
            };

            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log($"Joined Lobby with code {lobbyCode}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
        }
    }

    /// <summary>
    /// 플레이어의 이름을 업데이트합니다.
    /// </summary>
    public async void UpdatePlayerName(string playerID, string newPlayerName)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerID, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newPlayerName) }
                }
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to update player name: {ex.Message}");
        }
    }

    /// <summary>
    /// 로비에서 플레이어를 제거합니다.
    /// </summary>
    public async void LeaveLobby(string playerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to remove player: {ex.Message}");
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// 로비의 특정 데이터를 조회합니다.
    /// </summary>
    public string GetLobbyData(string key)
    {
        return joinedLobby.Data.TryGetValue(key, out var data) ? data.Value : null;
    }

    /// <summary>
    /// 새로운 Player 객체를 생성합니다.
    /// </summary>
    private Player GetPlayer(string playerName)
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
            }
        };
    }

    /// <summary>
    /// 로비를 삭제합니다.
    /// </summary>
    public async void DeleteLobby()
    {
        try
        {
            if (hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
                hostLobby = null;
                joinedLobby = null;
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to delete lobby: {ex.Message}");
        }
    }
    #endregion
}
