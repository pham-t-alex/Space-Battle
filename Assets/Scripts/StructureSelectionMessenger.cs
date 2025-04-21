using Unity.Netcode;
using UnityEngine;

public class StructureSelectionMessenger : NetworkBehaviour
{
    private static StructureSelectionMessenger _instance;
    public static StructureSelectionMessenger Instance => _instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _instance = this;
    }

    public void SelectStructure(bool selected, int structure)
    {
        SelectStructureRpc(selected, structure, default);
    }

    [Rpc(SendTo.Server)]
    public void SelectStructureRpc(bool selected, int structure, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StructureSelectionController.Instance.SelectStructure(clientId, selected, structure);
    }

    public void StructureSelected(ulong clientId, int structure)
    {
        StructureSelectedRpc(structure, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void StructureSelectedRpc(int structure, RpcParams rpcParams)
    {
        StructureSelectionMenu.Instance.StructureButton(structure).Select();
    }

    public void TryReady()
    {
        TryReadyRpc(default);
    }

    [Rpc(SendTo.Server)]
    public void TryReadyRpc(RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StructureSelectionController.Instance.TryReady(clientId);
    }

    public void ReadyLock(ulong clientId)
    {
        ReadyLockRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void ReadyLockRpc(RpcParams rpcParams)
    {
        StructureSelectionMenu.Instance.ReadyLock();
    }
}