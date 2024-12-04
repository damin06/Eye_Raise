using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;

public class ScoreManager : NetCodeSingleton<ScoreManager>
{
    [SerializeField] private MapRange mapRange;
    [SerializeField] private int maxSpawnCount;
    [SerializeField] private int minSpawnCount;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;

    private float m_lastSpawnedTime = 0;

    private List<Point> points = new List<Point>();


    private void Update()
    {
        if (!IsServer || points.Count > maxSpawnCount)
            return;

        if (points.Count < minSpawnCount || m_lastSpawnedTime + 2 < Time.time)
        {
            m_lastSpawnedTime = Time.time;
            SpawnPoint(new Vector2
                (
                    mapRange.GetRandomSpawnPos().x,
                    mapRange.GetRandomSpawnPos().y
                ),
                    Random.Range(minScale, maxScale),
                    Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f)
                );
        }
    }

    public void SpawnPoint(Vector2 position, float scale, Color color)
    {
        var _newPoint = NetworkObjectPool.Instance.GetNetworkObject("Point", position, Quaternion.identity);

        if (_newPoint.TryGetComponent(out Point _point))
        {
            points.Add(_point);
            _point.PointColor.Value = color;
            _point.Score.Value = Random.Range(1, 6);
            _point.transform.localScale = new Vector2(scale, scale);
        }
    }

    public void SpawnPlayerPoint(int point, Vector3 position, Color color)
    {
        var _newPoint = NetworkObjectPool.Instance.GetNetworkObject("Point", position, Quaternion.identity);

        if (_newPoint.TryGetComponent(out Point _point))
        {
            _point.Score.Value = point;
            _point.PointOwner.Value = OwnerClientId;
            _point.PointColor.Value = color;
        }
    }

    public void ReturnPoint(NetworkObject point)
    {
        if (point.TryGetComponent(out Point _point))
        {
            points.Remove(_point);
            _point.ActiveClientRpc(false);
        }

        NetworkObjectPool.Instance.ReturnNetworkObject(point);
    }

}