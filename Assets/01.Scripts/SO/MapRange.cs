using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "new MapRange", menuName = "SO/MapRange")]
public class MapRange : ScriptableObject
{
    [FormerlySerializedAs("LeftTop")]
    [SerializeField] private Vector2 leftTop;
    [FormerlySerializedAs("RightBottom")]
    [SerializeField] private Vector2 rightBottom;

    public Vector2 LeftTop => leftTop;
    public Vector2 RightBottom => rightBottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 GetRandomSpawnPos()
    {
        float randomX = Random.Range(leftTop.x, rightBottom.x);
        float randomY = Random.Range(rightBottom.y, leftTop.y);
        return new Vector2(randomX, randomY);
    }
}
