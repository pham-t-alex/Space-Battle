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

    public void GameUISetup(ulong client1, ulong client2)
    {
        GameUISetupRpc(1, RpcTarget.Single(client1, RpcTargetUse.Temp));
        GameUISetupRpc(2, RpcTarget.Single(client2, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GameUISetupRpc(int player, RpcParams rpcParams)
    {
        GameUI.Setup(player);
    }
}