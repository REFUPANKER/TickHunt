using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InsectSprayerMovement : tMovement
{
    [Range(300, 1000)]
    public float mouseSens;
    public float verCamMax = 40, verCamMin = -40;
    private float fpsRx;

    [Header("Cam Head Rig")]
    public Transform sprayGun, InsectSprayerHead;
    public Vector3 camMargin;
    public Transform HeadRigPointer;
    public Vector3 headRigMargin;
    public Vector3 sprayGunMarginToCam;

    public Transform SprayPointer;
    public float SprayDistance = 10;
    public LayerMask sprayableSurfaces;

    public struct CamHeadMovementValues : INetworkSerializable
    {
        public Vector3 camPos, headRigPos, sprayGunPos, sprayPos;
        public Quaternion camRot;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref camPos);
            s.SerializeValue(ref headRigPos);
            s.SerializeValue(ref sprayGunPos);
            s.SerializeValue(ref camRot);
            s.SerializeValue(ref sprayPos);
        }
    }
    NetworkVariable<CamHeadMovementValues> nvCamHeadMv = new NetworkVariable<CamHeadMovementValues>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SprayPointer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!CheckCanMove()) { return; }

        PerformMovementOpt2(false);

        cam.transform.position = InsectSprayerHead.position + (InsectSprayerHead.forward * camMargin.z) + (InsectSprayerHead.up * camMargin.y);
        HeadRigPointer.position = InsectSprayerHead.position + (cam.transform.forward * headRigMargin.z) + (cam.transform.up * headRigMargin.y);

        sprayGun.position = cam.transform.position + (cam.transform.forward * sprayGunMarginToCam.z) + (cam.transform.up * sprayGunMarginToCam.y) + (cam.transform.right * sprayGunMarginToCam.x);
        sprayGun.rotation = Quaternion.LookRotation(cam.transform.forward);

        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;
        fpsRx -= mouseY;
        fpsRx = Mathf.Clamp(fpsRx, verCamMin, verCamMax);
        transform.Rotate(Vector3.up * mouseX);
        cam.transform.localRotation = Quaternion.Euler(fpsRx, 0f, 0f);




        RaycastHit sprayHit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out sprayHit, SprayDistance, sprayableSurfaces))
        {
            if (!SprayPointer.gameObject.activeSelf) { SprayPointer.gameObject.SetActive(true); }
            SprayPointer.position = sprayHit.point;
            SprayPointer.rotation = Quaternion.LookRotation(Vector3.up);
        }
        else
        {
            if (SprayPointer.gameObject.activeSelf) { SprayPointer.gameObject.SetActive(false); }
        }

        CamHeadMovementValues nChmVals = new CamHeadMovementValues()
        {
            camPos = cam.transform.position,
            camRot = cam.transform.localRotation,
            headRigPos = HeadRigPointer.transform.position,
            sprayGunPos = sprayGun.position,
            sprayPos = SprayPointer.position
        };

        SyncCameraServerRpc(nChmVals);
    }

    void LateUpdate()
    {
        if (!CheckCanMove()) { return; }
    }

    [ServerRpc]
    void SyncCameraServerRpc(CamHeadMovementValues chmVals)
    {
        nvCamHeadMv.Value = chmVals;
        SyncCameraClientRpc(chmVals);
    }

    [ClientRpc]
    void SyncCameraClientRpc(CamHeadMovementValues chmVals)
    {
        if (!IsOwner)
        {
            SetCamSyncValues(chmVals);
        }
    }

    void SetCamSyncValues(CamHeadMovementValues chmVals)
    {
        cam.transform.position = chmVals.camPos;
        cam.transform.localRotation = chmVals.camRot;
        HeadRigPointer.transform.position = chmVals.headRigPos;
        sprayGun.position = chmVals.sprayGunPos;
        SprayPointer.position=chmVals.sprayPos;
    }

}
