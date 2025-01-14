using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("We have connected and spawned");
    }
}