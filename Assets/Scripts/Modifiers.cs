using UnityEngine;
using Unity.Netcode;

public struct Modifiers : INetworkSerializable
{
    public bool front;
    public bool shielded;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref front);
        serializer.SerializeValue(ref shielded);
    }
}