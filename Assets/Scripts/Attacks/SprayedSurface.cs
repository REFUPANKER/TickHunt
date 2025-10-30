using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SprayedSurface : NetworkBehaviour
{
    public int effectTime=3;
    ulong whoSprayed;
    public void Spray(ulong who, Vector3 pos)
    {
        whoSprayed = who;
        transform.position = pos;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            StartCoroutine(effectCooldown());
        }
    }
    IEnumerator effectCooldown()
    {
        yield return new WaitForSeconds(effectTime);
        DestroyServerRpc();
    }

    [ServerRpc]
    void DestroyServerRpc()
    {
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}
