using UnityEngine;
using Unity.Netcode;

public class Module : NetworkBehaviour
{
    private Structure structure;
    public Structure ModuleStructure => structure;
}
