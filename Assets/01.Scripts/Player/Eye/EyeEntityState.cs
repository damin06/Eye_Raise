using System;
using Unity.Netcode;

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
