using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float Speed = 5, MaxDistance = 100, CollisionDistance = 0.1f;
    private Vector3 StartPoint, EndPoint;
    public bool Fired, Collided, WaitingToDestroy;
    public float AfterCollidedDestroyTime = 2;

    public void Fire(Vector3 spawn, Vector3 target)
    {
        if (!IsServer) { return; }
        transform.position = spawn;
        StartPoint = spawn;
        EndPoint = target;
        transform.LookAt(target);
        SyncServerRpc(target);
        Fired = true;
    }

    void FixedUpdate()
    {
        //TODO: fix bullet hits
        if (!IsServer || !Fired || WaitingToDestroy) { return; }
        if (!Collided)
        {
            transform.position = Vector3.MoveTowards(transform.position, EndPoint, Speed * Time.deltaTime);
            SyncServerRpc(transform.position);
            if (Vector3.Distance(StartPoint, transform.position) >= MaxDistance) { Collided = true; }
        }
        else
        {
            WaitingToDestroy = true;
            StartCoroutine(destroyTimeout());
        }
    }
    IEnumerator destroyTimeout()
    {
        yield return new WaitForSeconds(AfterCollidedDestroyTime);
        DestroyServerRpc();
    }
    void OnTriggerEnter(Collider col)
    {
        Collided = true;
        Debug.Log(col.transform.name);
    }

    [ServerRpc]
    void SyncServerRpc(Vector3 pos)
    {
        SyncClientRpc(pos);
    }

    [ClientRpc]
    void SyncClientRpc(Vector3 pos)
    {
        transform.position = pos;
    }

    [ServerRpc]
    void DestroyServerRpc()
    {
        NetworkObject.Despawn(true);
        Destroy(gameObject);
    }
}