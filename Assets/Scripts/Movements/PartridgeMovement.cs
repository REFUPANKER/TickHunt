using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PartridgeMovement : tMovement
{
    public float MovementSpeed = 4;
    public float RotationSpeed = 200;
    public float gravity = -9.81f;
    private Vector3 moveDirection;
    public CinemachineFreeLook cam;

    void Update()
    {
        if (!player.IsOwner || (player.IsOwner && !player.CanMove)) { return; }

        float mV = Input.GetAxis("Vertical");
        float mH = Input.GetAxis("Horizontal");

        Vector3 m = transform.forward * mV;

        moveDirection.x = m.x * MovementSpeed;
        moveDirection.z = m.z * MovementSpeed;

        if (ctrl.isGrounded) { moveDirection.y = -2f; }
        else { moveDirection.y += gravity * Time.deltaTime; }
        
        ctrl.Move(moveDirection * Time.deltaTime);

        transform.Rotate(Vector3.up * mH * RotationSpeed * Time.deltaTime);

        animator.SetFloat("Velocity", ctrl.velocity.magnitude);

        mStruct newMstruct = GetNewValuesStruct();
        SyncServerRpc(newMstruct);

    }
}
