using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new respawnPostions", menuName = "SO/Respawn")]
public class RespawnPostions : ScriptableObject
{
    public List<Vector3> _spawnPositions = new List<Vector3>();
}
