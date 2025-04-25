using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct Modifiers : INetworkSerializable
{
    public bool shielded;
    public bool berserk;
    public bool invisible;
    public bool regenerating;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shielded);
        serializer.SerializeValue(ref berserk);
        serializer.SerializeValue(ref invisible);
        serializer.SerializeValue(ref regenerating);
    }
}