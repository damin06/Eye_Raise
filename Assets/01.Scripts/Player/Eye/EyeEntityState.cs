using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public struct EyeEntityState : INetworkSerializable, IEquatable<EyeEntityState>
{
    public ulong networkObjectId;
    public int score;

    public bool Equals(EyeEntityState other)
    {
        return networkObjectId == other.networkObjectId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref score);
    }
}
