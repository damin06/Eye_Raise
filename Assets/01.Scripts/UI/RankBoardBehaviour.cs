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

    private NetworkList<RankBoardEntityState> _rankList = new NetworkList<RankBoardEntityState>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<RecordUI> _rankUIList = new List<RecordUI>();

    public override void OnNetworkSpawn()
    {
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

        for (int i = 1; i < _rankList.Count; i++)
        {
            for (int j = i; j < _rankList.Count - i; j++)
            {
                if (_rankList[i - 1].score > _rankList[i].score)
                {
                    var newRank = _rankList[i];
                    _rankList[i] = _rankList[i - 1];
                    _rankList[i - 1] = newRank;
                }
            }
        }
        //AdjustScoreToUIList();
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
        _rankList.Add(new RankBoardEntityState
        {
            clientID = clientID,
            playerName = userData.username,
            score = 0
        });
    }

    private void HandleUserLeft(ulong clientID, UserData userData)
    {
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
        // _rankUIList를 점수를 기준으로 내림차순으로 정렬
        _rankUIList.Sort((a, b) =>
        {
            // 각 UI 항목에 해당하는 점수를 _rankList에서 찾기
            int scoreA = 0;
            int scoreB = 0;

            // 수동으로 _rankList를 순회하여 점수 찾기
            foreach (var rank in _rankList)
            {
                if (rank.clientID == a.clientID)
                    scoreA = rank.score;
                if (rank.clientID == b.clientID)
                    scoreB = rank.score;
            }

            // 점수가 높은 순으로 정렬 (내림차순)
            return scoreB.CompareTo(scoreA);
        });

        // 정렬된 순서에 따라 UI 요소 재배치
        for (int i = 0; i < _rankUIList.Count; i++)
        {
            // 순위 업데이트를 위해 현재 레코드 찾기
            RankBoardEntityState currentRecord = default;
            foreach (var rank in _rankList)
            {
                if (rank.clientID == _rankUIList[i].clientID)
                {
                    currentRecord = rank;
                    break;
                }
            }

            // 텍스트 업데이트 (순위는 1부터 시작)
            _rankUIList[i].SetText(i + 1, currentRecord.playerName.ToString(), currentRecord.score);

            // 부모 트랜스폼에서 위치 업데이트
            _rankUIList[i].transform.SetSiblingIndex(i);
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
