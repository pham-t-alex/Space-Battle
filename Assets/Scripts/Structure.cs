using Unity.Netcode;
using UnityEngine;

public abstract class Structure : NetworkBehaviour
{
    [SerializeField] private string structureName;
    [SerializeField] private int cost;
    public int Cost => cost;

    [SerializeField] private GameObject[] upgradePrefabs;
    public GameObject UpgradePrefab(int i) => upgradePrefabs[i];
    public int UpgradeCount => upgradePrefabs.Length;

    [SerializeField] private int value;
    public int Value => value;

    protected Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeValue()
    {
        value = cost;
    }

    public void InitializePlayer(Player p)
    {
        player = p;
    }

    // should only be called in upgrade
    public void AddValue(int prevCost)
    {
        value += prevCost;
    }

    public StructureUpgradeInfo UpgradeInfo
    {
        get
        {
            if (upgradePrefabs.Length == 1)
            {
                return new StructureUpgradeInfo(1, upgradePrefabs[0].GetComponent<Structure>().Info, StructureInfo.NoStructure);
            }
            else if (upgradePrefabs.Length == 2)
            {
                return new StructureUpgradeInfo(2, upgradePrefabs[0].GetComponent<Structure>().Info, upgradePrefabs[1].GetComponent<Structure>().Info);
            }
            else return new StructureUpgradeInfo(0, StructureInfo.NoStructure, StructureInfo.NoStructure);
        }
    }

    public StructureInfo Info => new StructureInfo(structureName, cost);

    public virtual void OnBuild() { }
    public virtual void OnSell() { }
}

public struct StructureInfo : INetworkSerializable
{
    public string Name;
    public int Cost;
    private static StructureInfo noStructure = new StructureInfo("", 0);

    public static StructureInfo NoStructure => noStructure;

    public bool IsNothing(StructureInfo info)
    {
        return info.Name == "";
    }

    public StructureInfo(string name, int cost)
    {
        Name = name;
        Cost = cost;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Cost);
    }
}

public struct StructureUpgradeInfo : INetworkSerializable
{
    public int UpgradeCount;
    public StructureInfo Upgrade1;
    public StructureInfo Upgrade2;

    public StructureUpgradeInfo(int count, StructureInfo upgrade1, StructureInfo upgrade2)
    {
        UpgradeCount = count;
        Upgrade1 = upgrade1;
        Upgrade2 = upgrade2;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UpgradeCount);
        serializer.SerializeValue(ref Upgrade1);
        serializer.SerializeValue(ref Upgrade2);
    }
}