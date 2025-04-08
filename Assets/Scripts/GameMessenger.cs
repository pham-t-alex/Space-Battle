using System;
using Unity.Netcode;
using UnityEngine;

public class GameMessenger : NetworkBehaviour
{
    private static GameMessenger _instance;
    public static GameMessenger Instance => _instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // per client event indicating whether they are victorious
    public event Action<bool> ClientGameEndUpdate;

    // client wave update
    public event Action<int> WaveUpdate;

    // client income multiplier update
    public event Action<float> IncomeMultiplierUpdate;

    // server/client timestamp
    private int incomeCounter = 0;

    public void GameUISetup(ulong client1, ulong client2, int moduleCost, int levelCost, 
        StructureInfo p1First, StructureInfo p1Second, StructureInfo p1Third,
        StructureInfo p2First, StructureInfo p2Second, StructureInfo p2Third)
    {
        if (!IsServer) return;
        GameUISetupRpc(1, moduleCost, levelCost, p1First, p1Second, p1Third, RpcTarget.Single(client1, RpcTargetUse.Temp));
        GameUISetupRpc(2, moduleCost, levelCost, p2First, p2Second, p2Third, RpcTarget.Single(client2, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GameUISetupRpc(int player, int moduleCost, int levelCost, 
        StructureInfo first, StructureInfo second, StructureInfo third, RpcParams rpcParams)
    {
        GameUI.Instance.Setup(player, moduleCost, levelCost, first, second, third);
    }

    public void SendAliens(int sendIndex, bool front)
    {
        SendAliensRpc(sendIndex, front, default);
    }

    [Rpc(SendTo.Server)]
    public void SendAliensRpc(int sendIndex, bool front, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.TrySendAliens(clientId, sendIndex, front);
    }

    // Winner is 1 or 2
    public void TriggerGameEnd(ulong client1, ulong client2, int winner)
    {
        if (!IsServer) return;
        if (winner == 1)
        {
            GameEndRpc(true, RpcTarget.Single(client1, RpcTargetUse.Temp));
            GameEndRpc(false, RpcTarget.Single(client2, RpcTargetUse.Temp));
        }
        else
        {
            GameEndRpc(true, RpcTarget.Single(client2, RpcTargetUse.Temp));
            GameEndRpc(false, RpcTarget.Single(client1, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GameEndRpc(bool victorious, RpcParams rpcParams)
    {
        ClientGameEndUpdate?.Invoke(victorious);
    }

    public void AddModule(bool right)
    {
        AddModuleRpc(right, default);
    }

    [Rpc(SendTo.Server)]
    public void AddModuleRpc(bool right, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.TryAddModule(clientId, right);
    }

    public void LevelUp()
    {
        LevelUpRpc(default);
    }

    [Rpc(SendTo.Server)]
    public void LevelUpRpc(RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.TryLevelUp(clientId);
    }

    public void BuildStructure(int module, int structure)
    {
        BuildStructureRpc(module, structure, default);
    }

    [Rpc(SendTo.Server)]
    public void BuildStructureRpc(int module, int structure, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.TryBuildStructure(clientId, module, structure);
    }

    public void UpdateStructure(ulong clientId, int value, StructureUpgradeInfo info)
    {
        UpdateStructureRpc(value, info, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateStructureRpc(int value, StructureUpgradeInfo info, RpcParams rpcParams)
    {
        GameUI.Instance.OpenStructureUI(value, info);
    }

    public void UpgradeStructure(int module, bool right)
    {
        UpgradeStructureRpc(module, right, default);
    }

    [Rpc(SendTo.Server)]
    public void UpgradeStructureRpc(int module, bool right, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.TryUpgradeStructure(clientId, module, right);
    }

    public void SellStructure(int module)
    {
        SellStructureRpc(module, default);
    }

    [Rpc(SendTo.Server)]
    public void SellStructureRpc(int module, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameController.Instance.SellStructure(clientId, module);
    }

    public void UpdateModule(ulong clientId)
    {
        UpdateModuleRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateModuleRpc(RpcParams rpcParams)
    {
        GameUI.Instance.OpenModuleUI();
    }

    public void TriggerWaveUpdate(int newWave)
    {
        WaveUpdateRpc(newWave);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WaveUpdateRpc(int wave)
    {
        WaveUpdate?.Invoke(wave);
    }

    public void UpdateModuleCost(ulong clientId, int cost)
    {
        UpdateModuleCostRpc(cost, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateModuleCostRpc(int cost, RpcParams rpcParams)
    {
        GameUI.Instance.UpdateModuleCost(cost);
    }

    public void MaxModules(ulong clientId)
    {
        MaxModuleRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void MaxModuleRpc(RpcParams rpcParams)
    {
        GameUI.Instance.MaxModules();
    }

    public void UpdateLevelCost(ulong clientId, int cost, int level)
    {
        UpdateLevelCostRpc(cost, level, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateLevelCostRpc(int cost, int level, RpcParams rpcParams)
    {
        GameUI.Instance.UpdateLevelCost(cost);
        GameUI.Instance.UpdateLevel(level);
    }

    public void MaxLevel(ulong clientId, int level)
    {
        MaxLevelRpc(level, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void MaxLevelRpc(int level, RpcParams rpcParams)
    {
        GameUI.Instance.MaxLevel();
        GameUI.Instance.UpdateLevel(level);
    }

    public void TriggerIncomeMultiplierUpdate(ulong clientId, float newMultiplier)
    {
        int incomeTime = ++incomeCounter;
        IncomeMultiplierRpc(newMultiplier, incomeTime, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void IncomeMultiplierRpc(float newMultiplier, int timestamp, RpcParams rpcParams)
    {
        if (timestamp <= incomeCounter) return;
        incomeCounter = timestamp;
        IncomeMultiplierUpdate?.Invoke(newMultiplier);
    }
}