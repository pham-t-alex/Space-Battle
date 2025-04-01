using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
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

    public void GameUISetup(ulong client1, ulong client2)
    {
        if (!IsServer) return;
        GameUISetupRpc(1, RpcTarget.Single(client1, RpcTargetUse.Temp));
        GameUISetupRpc(2, RpcTarget.Single(client2, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GameUISetupRpc(int player, RpcParams rpcParams)
    {
        GameUI.Instance.Setup(player);
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

    public void UpdateStructure(ulong clientId, StructureUpgradeInfo info)
    {
        UpdateStructureRpc(info, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateStructureRpc(StructureUpgradeInfo info, RpcParams rpcParams)
    {
        GameUI.Instance.OpenStructureUI(info);
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
}