using UnityEngine;
using Unity.Netcode;

public class Module : NetworkBehaviour
{
    private Structure structure;
    public Structure ModuleStructure => structure;

    public void SetStructure(Structure s)
    {
        if (structure != null) return;
        structure = s;
    }

    public void Upgrade(Structure s)
    {
        if (structure == null) return;
        Destroy(structure.gameObject);
        structure = s;
    }

    public void Destroy()
    {
        if (!IsServer) return;
        if (structure != null) Destroy(structure.gameObject);
        Destroy(gameObject);
    }
}
