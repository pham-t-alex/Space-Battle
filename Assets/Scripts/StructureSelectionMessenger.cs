using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;

public class StructureSelectionMessenger : NetworkBehaviour
{
    private static StructureSelectionMessenger _instance;
    public static StructureSelectionMessenger Instance => _instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            JoinLobbyRpc(SessionManager.Username, default);
        }
    }

    [Rpc(SendTo.Server)]
    public void JoinLobbyRpc(string username, RpcParams rpcParams)
    {
        SessionManager.Instance.Players[rpcParams.Receive.SenderClientId] = username;
        ClientLobbyUpdate();
    }

    public void ClientLobbyUpdate()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
        {
            ClientLobbyUpdateRpc(
                SessionManager.Instance.Players[NetworkManager.Singleton.ConnectedClientsIds[0]],
                null, StructureSelectionController.Instance.P1Ready, false, default
            );
        }
        else
        {
            ClientLobbyUpdateRpc(
                SessionManager.Instance.Players[NetworkManager.Singleton.ConnectedClientsIds[0]],
                SessionManager.Instance.Players[NetworkManager.Singleton.ConnectedClientsIds[1]],
                StructureSelectionController.Instance.P1Ready, StructureSelectionController.Instance.P2Ready, default
            );
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ClientLobbyUpdateRpc(string p1Name, string p2Name, bool p1Ready, bool p2Ready, RpcParams rpcParams)
    {
        StructureSelectionMenu.Instance.UpdatePlayers(p1Name, p2Name, p1Ready, p2Ready);
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