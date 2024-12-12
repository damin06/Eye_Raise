using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new MapRange", menuName = "SO/MapRange")]
public class MapRange : ScriptableObject
{
    [SerializeField] private PolygonCollider2D mapCollider;
    [SerializeField] private Vector2 minSpawnPos;
    [SerializeField] private Vector2 maxSpawnPos;

    public PolygonCollider2D MapColider => mapCollider;
    public Vector2 MinSpawnPos => minSpawnPos;
    public Vector2 MaxSpawnPos => maxSpawnPos;

    public Vector2 GetRandomSpawnPos()
    {
        return new Vector2(Random.Range(minSpawnPos.x, maxSpawnPos.x), Random.Range(minSpawnPos.y, maxSpawnPos.y));
    }
}
