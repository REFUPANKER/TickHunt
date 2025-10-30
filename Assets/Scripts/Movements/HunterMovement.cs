using Unity.Netcode;
using UnityEngine;

public class HunterMovement : tMovement
{
    [Range(300, 1000)]
    public float mouseSens;
    public float verCamMax = 40, verCamMin = -40;
    private float fpsRx;

    [Header("Cam Head Rig")]
    public Transform dartGun, hunterHead;
    public Vector3 camMargin;
    public Transform HeadRigPointer;
    public Vector3 headRigMargin;

    [Header("Dart Gun")]
    public Vector3 DartGunMargin;
    public Vector3 DartGunAimMargin;
    private Vector3 previousDartGunMargin;

    [Header("Arm Rig")]
    public Transform ArmRigPointer;
    public Transform ArmRigMidPointerRight;
    public Transform ArmRigMidPointerLeft;
    public Vector3 armRigMidMargin;

    public struct CamHeadMovementValues : INetworkSerializable
    {
        public Vector3 camPos, headRigPos, dartGunPos, armRmLeft, armRmRight;
        public Quaternion camRot;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref camPos);
            s.SerializeValue(ref camRot);
            s.SerializeValue(ref headRigPos);
            s.SerializeValue(ref dartGunPos);
            s.SerializeValue(ref armRmLeft);
            s.SerializeValue(ref armRmRight);
        }
    }
    NetworkVariable<CamHeadMovementValues> nvCamHeadMv = new NetworkVariable<CamHeadMovementValues>();

    void Update()
    {
        if (!CheckCanMove()) { return; }

        PerformMovementOpt2(false);

        cam.transform.position = hunterHead.position + (hunterHead.forward * camMargin.z) + (hunterHead.up * camMargin.y);

        HeadRigPointer.position = hunterHead.position + (cam.transform.forward * headRigMargin.z) + (cam.transform.up * headRigMargin.y);

        ArmRigMidPointerLeft.localPosition = new Vector3(-armRigMidMargin.x, armRigMidMargin.y, armRigMidMargin.z);
        ArmRigMidPointerRight.localPosition = armRigMidMargin;

        dartGun.localPosition = DartGunMargin;
        dartGun.rotation = Quaternion.LookRotation(cam.transform.forward);

        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;
        fpsRx -= mouseY;
        fpsRx = Mathf.Clamp(fpsRx, verCamMin, verCamMax);
        transform.Rotate(Vector3.up * mouseX);
        cam.transform.localRotation = Quaternion.Euler(fpsRx, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            previousDartGunMargin = DartGunMargin;
            DartGunMargin = DartGunAimMargin;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            DartGunMargin = previousDartGunMargin;
        }

        CamHeadMovementValues nChmVals = new CamHeadMovementValues()
        {
            camPos = cam.transform.position,
            camRot = cam.transform.localRotation,
            headRigPos = HeadRigPointer.transform.position,
            armRmLeft = ArmRigMidPointerLeft.localPosition,
            armRmRight = ArmRigMidPointerRight.localPosition,
            dartGunPos = DartGunMargin
        };

        SyncCameraServerRpc(nChmVals);
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

        ArmRigMidPointerLeft.localPosition = chmVals.armRmLeft;
        ArmRigMidPointerRight.localPosition = chmVals.armRmRight;

        dartGun.localPosition = chmVals.dartGunPos;
        dartGun.rotation = Quaternion.LookRotation(cam.transform.forward);
    }

}
