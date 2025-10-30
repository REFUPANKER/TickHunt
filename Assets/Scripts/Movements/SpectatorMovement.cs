using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorMovement : tMovement
{
    [Range(300, 1000)]
    public float mouseSens = 300;
    private float camRx, camRy;

    public float boostedSpeed = 20;
    public KeyCode boostKey = KeyCode.LeftShift;
    private float preSpeed;

    void Start()
    {
        preSpeed = MovementSpeed;
    }

    void Update()
    {
        SyncFromClientSide();
        if (!CheckCanMove()) { return; }

        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;
        camRx += mouseX;
        camRy -= mouseY;
        camRy = Mathf.Clamp(camRy, -90, 90);
        transform.localRotation = Quaternion.Euler(camRy, camRx, 0f);

        float mV = Input.GetAxis("Vertical");
        float mH = Input.GetAxis("Horizontal");

        Vector3 m = cam.transform.forward * mV + cam.transform.right * mH;
        if (m.magnitude > 1) { m.Normalize(); }

        if (Input.GetKeyDown(boostKey)) { MovementSpeed = boostedSpeed; }
        if (Input.GetKeyUp(boostKey)) { MovementSpeed = preSpeed; }

        ctrl.Move(m * MovementSpeed * Time.deltaTime);

        mStruct newMstruct = GetNewValuesStruct();
        SyncServerRpc(newMstruct);
    }
}
