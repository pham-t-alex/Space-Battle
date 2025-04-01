using Unity.Netcode;
using UnityEngine;

public abstract class Structure : NetworkBehaviour
{
    [SerializeField] private int cost;
    public int Cost => cost;

    [SerializeField] private GameObject[] upgradePrefabs;
    public GameObject UpgradePrefab(int i) => upgradePrefabs[i];
    public int UpgradeCount => upgradePrefabs.Length;

    [SerializeField] private int value;
    public int Value => value;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // should only be called in build
    public void InitializeValue()
    {
        value = cost;
    }

    // should only be called in upgrade
    public void AddValue(int prevCost)
    {
        value += prevCost;
    }

    public abstract void Upgrade();
    public abstract void Sell();

    public StructureUpgradeInfo UpgradeInfo
    {
        get
        {
            if (upgradePrefabs.Length == 1)
            {
                return new StructureUpgradeInfo(1, upgradePrefabs[0].GetComponent<Structure>().cost, 0);
            }
            else if (upgradePrefabs.Length == 2)
            {
                return new StructureUpgradeInfo(2, upgradePrefabs[0].GetComponent<Structure>().cost, upgradePrefabs[0].GetComponent<Structure>().cost);
            }
            else return new StructureUpgradeInfo(0, 0, 0);
        }
    }
}

public struct StructureUpgradeInfo : INetworkSerializable
{
    public int UpgradeCount;
    public int Upgrade1Cost;
    public int Upgrade2Cost;

    public StructureUpgradeInfo(int count, int one, int two)
    {
        UpgradeCount = count;
        Upgrade1Cost = one;
        Upgrade2Cost = two;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UpgradeCount);
        serializer.SerializeValue(ref Upgrade1Cost);
        serializer.SerializeValue(ref Upgrade2Cost);
    }
}