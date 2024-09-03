using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public Vector2 minSpawnPos;
    public Vector2 maxSpawnPos;
    [SerializeField] private int maxSpawnCount;
    [SerializeField] private int minSpawnCount;

    private float m_lastSpawnedTime = 0;

    private List<Point> points = new List<Point>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            
        }
    }

    private void Update()
    {
        if (!IsServer || points.Count > maxSpawnCount)
            return;

        if(points.Count < minSpawnCount || m_lastSpawnedTime + 2 < Time.time)
        {
            m_lastSpawnedTime = Time.time;
            SpawnPoint(new Vector2
                (
                    Random.Range(minSpawnPos.x, maxSpawnPos.x),
                    Random.Range(minSpawnPos.y, maxSpawnPos.y)
                ),
                    Random.Range(0.55f, 0.75f),
                    Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f)
                );
        }
    }

    public void SpawnPoint(Vector2 position, float scale, Color color)
    {
        var _newPoint = NetworkObjectPool.Instance.GetNetworkObject("Point", position, Quaternion.identity);

        if(_newPoint.TryGetComponent(out Point _point))
        {
            points.Add(_point);
            _point.color.Value = color;
            _point.point.Value = Random.Range(1, 6);
            _point.transform.localScale = new Vector2(scale, scale); 
        }
    }

    public void SpawnPlayerPoint(int point,Vector3 position, Color color)
    {
        var _newPoint = NetworkObjectPool.Instance.GetNetworkObject("Point", position, Quaternion.identity);

        if (_newPoint.TryGetComponent(out Point _point))
        {
            _point.point.Value = point;
            _point.isUserPoint.Value = true;
            _point.color.Value = color;
        }
    }

    public void ReturnPoint(NetworkObject point)
    {
        if(point.TryGetComponent(out Point _point))
        {
            points.Remove(_point);
            _point.ActiveClientRpc(false);
        }

        NetworkObjectPool.Instance.ReturnNetworkObject(point);
    }

}

#if (UNITY_EDITOR) 
[CustomEditor(typeof(ScoreManager)), CanEditMultipleObjects]
public class ScoreManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        ScoreManager manager = (ScoreManager)target;
        manager.minSpawnPos = Handles.PositionHandle(manager.minSpawnPos, Quaternion.identity);
        manager.maxSpawnPos = Handles.PositionHandle(manager.maxSpawnPos, Quaternion.identity);
    }
}
#endif