using Unity.Netcode;
using UnityEngine;

public class InsectSprayerAttack : NetworkBehaviour
{
    [SerializeField] tMovement m;
    public KeyCode sprayKey;

    public Transform sprayGunMuzzle;
    public Transform SprayPointer;
    public float SprayDistance = 10;
    public LayerMask sprayableSurfaces;
    public string poisonedSurfaceTag;

    public SprayedSurface PoisonedSurfacePrefab;

    public AudioSource sfxSpray;
    /**
    - opt 1 -
    spray gun can be used on all directions so we shouldnt limit with only sprayable surfaces
    we just gonna play particle effect if player is spraying the air 
    we can also add direct spraying to alives , when player directly aims to enemy and sprays
    the enemy gets poisioned
    */
    void Start()
    {
        SprayPointer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!m.CheckCanMove()) { return; }
        RaycastHit sprayHit;
        bool hitMade = Physics.Raycast(sprayGunMuzzle.position, sprayGunMuzzle.forward, out sprayHit, SprayDistance, sprayableSurfaces);
        bool canSprayToSurface = hitMade ? sprayHit.transform.tag != poisonedSurfaceTag : false;

        if (Input.GetKeyDown(sprayKey)) { sfxSpraySyncServerRpc(true); }
        if (Input.GetKeyUp(sprayKey)) { sfxSpraySyncServerRpc(false); }

        // if not hits are made but sprayed directly to enemy > apply poision effect on them and place poisioned surface
        if (canSprayToSurface && Input.GetKey(sprayKey))
        {
            SprayServerRpc(NetworkObject.OwnerClientId, sprayHit.point, hitMade);
        }

        if (hitMade)
        {
            if (!SprayPointer.gameObject.activeSelf) { SprayPointer.gameObject.SetActive(true); }
            SprayPointer.position = sprayHit.point;
            SprayPointer.rotation = Quaternion.LookRotation(Vector3.up);
        }
        else
        {
            if (SprayPointer.gameObject.activeSelf) { SprayPointer.gameObject.SetActive(false); }
        }
    }

    [ServerRpc]
    void SprayServerRpc(ulong byWho, Vector3 pos, bool hitMade)
    {
        SprayedSurface nPS = Instantiate(PoisonedSurfacePrefab);
        pos = !hitMade ? sprayGunMuzzle.forward * SprayDistance : pos;
        nPS.NetworkObject.Spawn();
        nPS.Spray(byWho, pos);
        nPS.transform.SetParent(m.player.soam.TempObjHolder);
    }

    [ServerRpc]
    void sfxSpraySyncServerRpc(bool play)
    {
        sfxSpraySyncClientRpc(play);
    }

    [ClientRpc]
    void sfxSpraySyncClientRpc(bool play)
    {
        if (play)
        {
            sfxSpray.Play();
        }
        else
        {
            sfxSpray.Stop();
        }
    }
}
