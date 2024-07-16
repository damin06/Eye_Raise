using TMPro;
using UnityEngine;

public class RecordUI : MonoBehaviour
{
    private TextMeshProUGUI _recordText;

    public ulong clientID;

    private void Awake()
    {
        _recordText = GetComponent<TextMeshProUGUI>();
    }

    public void SetOwner(ulong ownerID)
    {
        clientID = ownerID;
    }

    public void SetText(int rank, string username, int score)
    {
        _recordText.SetText($"{rank.ToString()} . {username} [ {score.ToString()} ]");
    }
}
