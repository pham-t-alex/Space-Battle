using System;
using Unity.Netcode;
using UnityEngine;

public class GameMessenger : NetworkBehaviour
{
    private static GameMessenger _instance;
    public static GameMessenger Instance => _instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // per client event indicating whether they are victorious
    public event Action<bool> ClientGameEndUpdate;

    public void GameUISetup(ulong client1, ulong client2)
    {
        if (!IsServer) return;
        GameUISetupRpc(1, RpcTarget.Single(client1, RpcTargetUse.Temp));
        GameUISetupRpc(2, RpcTarget.Single(client2, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GameUISetupRpc(int player, RpcParams rpcParams)
    {
        GameUI.Setup(player);
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
}