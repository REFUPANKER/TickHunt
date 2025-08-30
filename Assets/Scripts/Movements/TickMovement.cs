using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TickMovement : tMovement
{
    public float MovementSpeed = 4;
    public float RotationSpeed = 200;
    public float gravity = -9.81f;
    private Vector3 moveDirection;
    public LayerMask climbableSurfaces;

    public float FlipLimit = -0.2f;
    public KeyCode FlipKey = KeyCode.R;

    public CinemachineFreeLook cam;

    public bool climbing;

    public float twoPointAlignDist = 0.3f;
    public float twoPointShortAlignDist = 0.2f;
    public float groundAligmentDist = 0.2f;
    public Vector3 groundAlignOffset;
    public Vector3 verticalMargin;
    public Vector3 boundAngle;
    public Vector3 boundShortAngle;

    void Update()
    {
        //TODO:remove if (!player.IsOwner || (player.IsOwner && !player.CanMove)) { return; }

        float mV = Input.GetAxis("Vertical");
        float mH = Input.GetAxis("Horizontal");

        Vector3 m = transform.forward * mV;
        moveDirection = m * MovementSpeed;
        // moveDirection.x = m.x * MovementSpeed;
        // moveDirection.z = m.z * MovementSpeed;

        // if (ctrl.isGrounded) { moveDirection.y = -2f; }
        // else { moveDirection.y += gravity * Time.deltaTime; }

        ctrl.Move(moveDirection * Time.deltaTime);

        transform.Rotate(Vector3.up * mH * RotationSpeed * Time.deltaTime);

        animator.SetFloat("Velocity", ctrl.velocity.magnitude);

        if (Input.GetKeyDown(FlipKey) && Vector3.Dot(transform.up, Vector3.up) <= FlipLimit)
        {
            transform.rotation *= Quaternion.FromToRotation(transform.up, Vector3.up);
        }

        mStruct newMstruct = GetNewValuesStruct();
        SyncServerRpc(newMstruct);
    }

    void FixedUpdate()
    {
        AlignmentWithFrontBackRays();
    }

    void AlignWithHit(RaycastHit hit)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        //transform.position = hit.point + transform.up * colliderHeight.bounds.extents.y;
    }

    void AlignmentWithFrontBackRays()
    {
        float mV = Input.GetAxis("Vertical");

        Vector3 sourcePoint = transform.position + transform.up * verticalMargin.y;
        RaycastHit frontHit;
        bool FrontPhysic = Physics.Raycast(sourcePoint, -transform.up + (transform.forward * boundAngle.z), out frontHit, twoPointAlignDist, climbableSurfaces);
        RaycastHit backHit;
        bool BackPhysic = Physics.Raycast(sourcePoint, -transform.up + (-transform.forward * boundAngle.z), out backHit, twoPointAlignDist, climbableSurfaces);

        if (BackPhysic) { Debug.DrawLine(sourcePoint, backHit.point, Color.red, 0.1f); }
        if (FrontPhysic) { Debug.DrawLine(sourcePoint, frontHit.point, Color.green, 0.1f); }


        RaycastHit bottomFront, bottomBack;
        Vector3 frontPoint = transform.position + transform.forward * boundShortAngle.z;
        Vector3 backPoint = transform.position - transform.forward * boundShortAngle.z;
        bool bottomCheckFront = Physics.Raycast(frontPoint, -transform.up + transform.forward * boundShortAngle.y, out bottomFront, twoPointShortAlignDist, climbableSurfaces);
        bool bottomCheckBack = Physics.Raycast(backPoint, -transform.up - transform.forward * boundShortAngle.y, out bottomBack, twoPointShortAlignDist, climbableSurfaces);

        Debug.DrawLine(frontPoint, bottomFront.point, Color.yellow, 0.1f);
        Debug.DrawLine(backPoint, bottomBack.point, Color.cyan, 0.1f);

        if (mV > 0)
        {
            AlignWithHit(FrontPhysic ? frontHit : bottomFront);
        }
        if (mV < 0)
        {
            AlignWithHit(BackPhysic ? backHit : bottomBack);
        }

        RaycastHit groundAligner;
        if (Physics.Raycast(transform.position, -transform.up, out groundAligner, groundAligmentDist, climbableSurfaces))
        {
            transform.position = groundAligner.point + transform.up * groundAlignOffset.y;
        }

        // if (bottomCheckFront || bottomCheckBack)
        // {
        //     Vector3 averageNormal = (bottomFront.normal + bottomBack.normal).normalized;
        //     transform.rotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;
        // }

    }
}