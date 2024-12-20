using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Lobby : UI_Panel
{
    private VerticalLayoutGroup content;
    [SerializeField] private Button lobbyBtn;

    protected override void Init()
    {
        base.Init();
        Bind<VerticalLayoutGroup>();

        content = Get<VerticalLayoutGroup>("Content_Lobbies");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        RefreshLobbiesList();
        InvokeRepeating("RefreshLobbiesList", 3, 2);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        CancelInvoke();
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }

    [Command]
    private async void RefreshLobbiesList()
    {
        foreach(Transform lobby in content.transform)
        {
            Destroy(lobby.gameObject);
        }

        List<Lobby> lobbies = await LobbySingleton.Instance.GetLobbiesList();
        foreach (Lobby newLobby in lobbies)
        {
            Button_Lobby newButton = (Button_Lobby)Instantiate(lobbyBtn, content.transform);
            newButton.SetLobby(newLobby);
        }
    }

    protected override IEnumerator ActiveSceneRoutine()
    {
        yield return null;
    }

    protected override IEnumerator DeactiveSceneRoutine()
    {
        yield return null;
    }
}
