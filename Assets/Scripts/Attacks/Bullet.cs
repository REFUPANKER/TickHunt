using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : NetworkBehaviour
{
    public float Speed = 20f;
    public float MaxLifeTime = 5f;
    public float AfterCollidedDestroyTime = 2f;
    public Rigidbody Rb;
    private ulong bulletOwnerId;
    public readonly NetworkVariable<bool> IsActive = new NetworkVariable<bool>(true);

    public Image hitMarker;
    public AudioSource sfxHit;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        hitMarker.enabled = false;
        if (IsServer)
        {
            StartCoroutine(LifeTimeout());
        }

        IsActive.OnValueChanged += OnActiveStateChanged;
    }

    public void Fire(Vector3 spawn, Vector3 target, ulong whoFired)
    {
        if (!IsServer) return;
        bulletOwnerId = whoFired;

        Vector3 direction = (target - spawn).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        transform.position = spawn;
        transform.rotation = rotation;

        Rb.linearVelocity = direction * Speed;
        SyncRbClientRpc(Rb.linearVelocity);
    }

    [ClientRpc]
    void SyncRbClientRpc(Vector3 v)
    {
        if (!IsServer)
        {
            Rb.linearVelocity = v;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!IsServer) return;
        Rb.linearVelocity = Vector3.zero;
        SyncRbClientRpc(Vector3.zero);
        IsActive.Value = false;
        tHealth fH = col.GetComponent<tHealth>();
        if (fH)
        {
            fH.Damage(10);
            HitMarkerClientRpc(bulletOwnerId);
        }
    }

    [ClientRpc]
    void HitMarkerClientRpc(ulong whoFired)
    {
        if (whoFired == NetworkManager.Singleton.LocalClientId)
        {
            StartCoroutine(hitMarkerCoroutine());
        }
    }

    private void OnActiveStateChanged(bool oldVal, bool newVal)
    {
        if (IsServer && !newVal)
        {
            StartCoroutine(DespawnAfterDelay(AfterCollidedDestroyTime));
        }
    }

    IEnumerator hitMarkerCoroutine()
    {
        hitMarker.enabled = true;
        sfxHit.Play();
        yield return new WaitForSeconds(sfxHit.clip.length);
        hitMarker.enabled = false;
    }

    IEnumerator LifeTimeout()
    {
        yield return new WaitForSeconds(MaxLifeTime);
        if (IsActive.Value)
        {
            IsActive.Value = false;
        }
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}