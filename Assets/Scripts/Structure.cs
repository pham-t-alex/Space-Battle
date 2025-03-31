using Unity.Netcode;
using UnityEngine;

public abstract class Structure : NetworkBehaviour
{
    [SerializeField] private int cost;
    public int Cost => cost;

    [SerializeField] private GameObject upgradePrefab;
    public GameObject UpgradePrefab => upgradePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void Upgrade();
    public abstract void Sell();
}

public struct StructureUpgradeInfo : INetworkSerializable
{
    public int Upgrade1Cost;
    public int Upgrade2Cost;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Upgrade1Cost);
        serializer.SerializeValue(ref Upgrade2Cost);
    }
}