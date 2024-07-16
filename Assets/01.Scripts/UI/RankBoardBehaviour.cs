using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RankBoardBehaviour : NetworkBehaviour
{
    public Action<ulong, int> onUserScoreChanged;
    public static RankBoardBehaviour _instance;
    public static RankBoardBehaviour Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<RankBoardBehaviour>();

            if (_instance == null)
            {
                Debug.LogError("Server singleton does not exists");
            }
            return _instance;
        }
    }



    [SerializeField] private RecordUI _recordPrefab;
    [SerializeField] private RectTransform _recordParentTrm;

    private NetworkList<RankBoardEntityState> _rankList;

    private List<RecordUI> _rankUIList = new List<RecordUI>();

    private void Awake()
    {
        _rankList = new NetworkList<RankBoardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        //클라이언트면 
        // 랭크리스트에 변화에 리스닝을 해줘야 겠지?
        // 맨 처음 접속시에는 리스트에 있는 모든 애들을 추가하는 작업도 해야해
        if(IsClient)
        {
            _rankList.OnListChanged += HandleRankListChanged;
            foreach(var entity in _rankList)
            {
                HandleRankListChanged(new NetworkListEvent<RankBoardEntityState>
                {
                    Type = NetworkListEvent<RankBoardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }


        //서버면 

        if(IsServer)
        {
            ServerSingleton.Instance.NetServer.OnUserJoin += HandleUserJoin;
            ServerSingleton.Instance.NetServer.OnUserLeft += HandleUserLeft;
            onUserScoreChanged += UpdateScore;
        }
    }

    public void UpdateScore(ulong clientID, int score)
    {
        for (int i = 0; i < _rankList.Count; i++)
        {
            if (_rankList[i].clientID != clientID)
                continue;

            var oldItem = _rankList[i];
            _rankList[i] = new RankBoardEntityState
            {
                clientID = clientID,
                score = score,
                playerName = oldItem.playerName,
            };
            break;
        }
        AdjustScoreToUIList();
    }

    public override void OnNetworkDespawn()
    {
        //여기다 클라도 알잘딱 끊어줘야 한다.
        if(IsClient)
        {
            _rankList.OnListChanged -= HandleRankListChanged;
        }

        if (IsServer)
        {
            ServerSingleton.Instance.NetServer.OnUserJoin -= HandleUserJoin;
            ServerSingleton.Instance.NetServer.OnUserLeft -= HandleUserLeft;
        }
    }

    private void HandleUserJoin(ulong clientID, UserData userData)
    {
        //랭킹보드에 추가를 해줘야겠지? 알잘딱으로(리스트에서)
        _rankList.Add(new RankBoardEntityState
        {
            clientID = clientID,
            playerName = userData.username,
            score = 0
        });
    }

    private void HandleUserLeft(ulong clientID, UserData userData)
    {
        //랭킹보드에서 해당 클라이언트 아이디를 제거해줘야겠지?(리스트에서)
        foreach(RankBoardEntityState entity in _rankList)
        {
            if (entity.clientID != clientID) continue;

            try
            {
                _rankList.Remove(entity);
            }catch (Exception ex)
            {
                Debug.LogError(
                    $"{entity.playerName} [ {entity.clientID} ] : 삭제중 오류발생\n {ex.Message}");
            }
            break;
        }
    }


    //서버가 이걸 실행하는거임. 클라는 안건드려
    public void HandleChangeScore(ulong clientID, int score)
    {
        for(int i = 0; i < _rankList.Count; ++i)
        {
            if (_rankList[i].clientID != clientID) continue;

            var oldItem = _rankList[i];
            _rankList[i] = new RankBoardEntityState
            {
                clientID = clientID,
                playerName = oldItem.playerName,
                score = score
            };
            break;
        }
    }


    private void HandleRankListChanged(NetworkListEvent<RankBoardEntityState> evt)
    {
        switch (evt.Type)
        {
            case NetworkListEvent<RankBoardEntityState>.EventType.Add:
                AddUIToList(evt.Value);
                break;
            case NetworkListEvent<RankBoardEntityState>.EventType.Remove:
                RemoveFromUIList(evt.Value.clientID);
                break;
            case NetworkListEvent<RankBoardEntityState>.EventType.Value:
                AdjustScoreToUIList();
                break;
        }
    }

    private void AdjustScoreToUIList()
    {
        //값을 받아서 해당 UI를 찾아서 (올바른 클라이언트 ID) score를 갱신한다.
        // 선택 : 갱신후에는 UIList를 정렬하고 
        // 정렬된 순서에 맞춰서 실제 UI의 순서도 변경한다.
        // RemoveFromParent => Add
        for(int i = 1; i < _rankList.Count; i++)
        {
            for(int j = i; j < _rankList.Count - i; j++)
            {
                if (_rankList[i-1].score > _rankList[i].score)
                {
                    var newRank = _rankList[i];
                    _rankList[i] = _rankList[i - 1];
                    _rankList[i - 1] = newRank;
                }
            }
        }

        foreach (var item in _rankUIList)
            item.gameObject.transform.SetParent(null);

        for (int i = 0; i < _rankList.Count; i++)
        {
            foreach(var ui in _rankUIList)
            {
                if(ui.clientID == _rankList[i].clientID)
                {
                    ui.transform.SetParent(_recordParentTrm);
                    ui.SetText(i+1, _rankList[i].playerName.ToString(), _rankList[i].score);
                    break;
                }
            }
        }

    }

    private void AddUIToList(RankBoardEntityState value)
    {
        //중복이 있는지 검사후에 만들어서 
        var target = _rankUIList.Find(x => x.clientID == value.clientID);
        if (target != null) return;

        RecordUI newUI = Instantiate(_recordPrefab, _recordParentTrm);
        newUI.SetOwner(value.clientID);
        newUI.SetText(1, value.playerName.ToString(), value.score);
        //만들때 clientID넣어주는거 잊지말자.
        //UI에 추가하고 차후 중복검사를 위해서 _rankUIList 에도 넣어준다.
        _rankUIList.Add(newUI);
    }

    private void RemoveFromUIList(ulong clientID)
    {
        //_rankUIList 에서 clientID가 일치하는 녀석을 찾아서 리스트에서 제거하고
        var target = _rankUIList.Find(x => x.clientID == clientID);
        if(target != null)
        {
            _rankUIList.Remove(target);
            Destroy(target.gameObject);
        }
        // 해당 게임오브젝트를 destroy()
    }

    private RecordUI FindrecordUI(ulong clientID)
    {
        RecordUI recordUI = null;
        foreach (var ui in _rankUIList)
            if (ui.clientID == clientID)
                recordUI = ui;
        return recordUI;
    }
}
