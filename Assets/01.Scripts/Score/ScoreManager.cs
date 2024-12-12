using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 팩토리 패턴으로 바꾸기
/// </summary>
/// 

public class ScoreManager : NetCodeSingleton<ScoreManager>
{
    [SerializeField] private MapRange mapRange;
    [SerializeField] private int maxSpawnCount;
    [SerializeField] private int minSpawnCount;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;

    private float m_lastSpawnedTime = 0;

    private List<Point> points = new List<Point>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            for(int i = 0; i < minSpawnCount; i++)
            {
                SpawnRandomPoint();
            }

            Invoke("SpawnRoutine", Random.Range(1, 6));
        }
    }
    
    private void SpawnRoutine()
    {
        if(points.Count > maxSpawnCount)
        {
            CancelInvoke();
            return;
        }

        SpawnRandomPoint();

        Invoke("SpawnRoutine", Random.Range(1, 6));
    }

    public void SpawnRandomPoint()
    {
        SpawnPoint
        (
            new Vector2
            (
                mapRange.GetRandomSpawnPos().x,
                mapRange.GetRandomSpawnPos().y
            ),
            Random.Range(minScale, maxScale),
            Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f)
        );
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

    public void SpawnPlayerPoint(ulong ownerClientId, int point, Vector3 position, Vector2 dir, Color color)
    {
        var _newPoint = NetworkObjectPool.Instance.GetNetworkObject("Point", position, Quaternion.identity);

        if (_newPoint.TryGetComponent(out Point _point))
        {
            _point.Score.Value = point;
            _point.PointOwner.Value = ownerClientId;
            _point.PointColor.Value = color;
        }

        if( _newPoint.TryGetComponent(out Rigidbody2D _rb))
        {
            _rb.AddForce(dir * 2, ForceMode2D.Impulse);
        }
    }

    public void ReturnPoint(NetworkObject point)
    {
        if (point.TryGetComponent(out Point _point))
        {
            if (points.Contains(_point))
            {
                points.Remove(_point);
            }
            _point.ActiveClientRpc(false);
        }

        NetworkObjectPool.Instance.ReturnNetworkObject(point);

        if(points.Count < maxSpawnCount)
        {
            CancelInvoke();
            Invoke("SpawnRoutine", Random.Range(1, 6));
        }
    }

}