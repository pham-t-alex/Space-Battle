using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct Modifiers : INetworkSerializable
{
    public bool shielded;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shielded);
    }
}