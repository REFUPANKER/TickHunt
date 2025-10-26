using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HunterAttack : NetworkBehaviour
{
    [SerializeField] tMovement m;
    public Transform dartGunMuzzle;
    public Bullet dartGunBullet;
    public bool canFire = true;
    public float distance = 15, MaxDistance = 100;
    public KeyCode FireKey = KeyCode.Mouse0;
    public LayerMask exceptDart;
    [Tooltip("Consider fire sound effect length")]
    public int fireCooldown = 1;
    public AudioSource fireSfx;

    void Update()
    {
        if (!m.CheckCanMove()) { return; }
        if (canFire && Input.GetKeyDown(FireKey))
        {
            RaycastHit hit;
            bool hitMade = Physics.Raycast(dartGunMuzzle.position, dartGunMuzzle.forward, out hit, distance, exceptDart);
            SpawnBulletServerRpc(hitMade ? hit.point : dartGunMuzzle.position + dartGunMuzzle.forward * MaxDistance, NetworkObject.OwnerClientId);
            StartCoroutine(fireCooldownCoroutine());
        }
    }
    IEnumerator fireCooldownCoroutine()
    {
        canFire = false;
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    [ServerRpc]
    void SpawnBulletServerRpc(Vector3 target, ulong whoFired)
    {
        Bullet nb = Instantiate(dartGunBullet,
        dartGunMuzzle.position,
        Quaternion.LookRotation(target - dartGunMuzzle.position));
        nb.NetworkObject.Spawn();
        nb.transform.SetParent(m.player.soam.TempObjHolder);
        nb.Fire(dartGunMuzzle.position, target, whoFired);
        PlayFireSfxClientRpc();
    }
    [ClientRpc]
    void PlayFireSfxClientRpc()
    {
        fireSfx.Play();
    }
}
