using Unity.Netcode;
using UnityEngine;

public class HunterAttack : NetworkBehaviour
{
    public Transform dartGunMuzzle;
    public Bullet dartGunBullet;
    public float distance = 15, MaxDistance = 100;
    public KeyCode FireKey = KeyCode.Mouse0;
    public LayerMask exceptDart;

    void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(FireKey))
        {
            RaycastHit hit;
            if (Physics.Raycast(dartGunMuzzle.position, dartGunMuzzle.forward, out hit, distance, exceptDart))
            {
                SpawnBulletServerRpc(hit.point);
            }
            else
            {
                SpawnBulletServerRpc(dartGunMuzzle.position + dartGunMuzzle.forward * MaxDistance);
            }
        }
    }

    [ServerRpc]
    void SpawnBulletServerRpc(Vector3 target)
    {
        Bullet nb = Instantiate(dartGunBullet);
        nb.NetworkObject.Spawn();
        nb.Fire(dartGunMuzzle.position, target);
    }
}
