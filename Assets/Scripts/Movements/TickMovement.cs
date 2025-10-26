using Unity.Mathematics;
using UnityEngine;

public class TickMovement : tMovement
{
    public LayerMask climbableSurfaces;

    public float FlipLimit = -0.2f;
    public KeyCode FlipKey = KeyCode.R;

    public bool Grounded;
    public bool Falling;
    public float GroundCheckerDist = 0.2f;

    public float twoPointAlignDist = 0.3f;
    public float twoPointShortAlignDist = 0.2f;
    public Vector3 groundAlignOffset;
    public Vector3 verticalMargin;
    public Vector3 boundAngle;
    public Vector3 boundShortAngle;
    public float Width;

    float mV, mH;
    void Update()
    {
        if (!CheckCanMove()) { return; }

        mV = Input.GetAxis("Vertical");
        mH = Input.GetAxis("Horizontal");

        Vector3 m = transform.forward * mV;

        //moveDirection = m * MovementSpeed;
        moveDirection.x = m.x * MovementSpeed;
        moveDirection.z = m.z * MovementSpeed;

        if (Grounded) { moveDirection.y = m.y * MovementSpeed; }
        else if (!Falling)
        {
            Falling = true;
        }
        //TODO: when user climbs wall and reaches to corner , it can stand at the edge of it and it causes to movement on air , so make ground checker and falling system
        //TODO: FIX
        if (Falling)
        {
            moveDirection.y += gravity * Time.deltaTime;
            RaycastHit FallingSurfaceAlign;
            Physics.Raycast(transform.position, -Vector3.up, out FallingSurfaceAlign, math.INFINITY, climbableSurfaces);
            if (ctrl.velocity.y <= 0.1f)
            {
                Falling = false;
                transform.position = FallingSurfaceAlign.point;
            }
        }

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

    bool GroundChecking()
    {
        RaycastHit groundCheckingHit;
        bool r = Physics.Raycast(transform.position, -transform.up, out groundCheckingHit, GroundCheckerDist, climbableSurfaces);
        return r;
    }
    void FixedUpdate()
    {
        AlignmentWithFrontBackRays();
    }

    void AlignWithHit(RaycastHit hit)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 50f);
        //transform.position = hit.point + transform.up * groundAlignOffset.y;
    }

    void AlignmentWithFrontBackRays()
    {
        Vector3 FrontSourcePoint = transform.position + transform.up * verticalMargin.y + transform.forward * boundAngle.y;
        Vector3 BackSourcePoint = transform.position + transform.up * verticalMargin.y - transform.forward * boundAngle.y;
        RaycastHit frontHit;
        bool FrontPhysic = Physics.Raycast(FrontSourcePoint, -transform.up + (transform.forward * boundAngle.z), out frontHit, twoPointAlignDist, climbableSurfaces);
        RaycastHit backHit;
        bool BackPhysic = Physics.Raycast(BackSourcePoint, -transform.up + (-transform.forward * boundAngle.z), out backHit, twoPointAlignDist, climbableSurfaces);

        if (FrontPhysic) { Debug.DrawLine(FrontSourcePoint, frontHit.point, Color.green, 0.1f); }
        if (BackPhysic) { Debug.DrawLine(BackSourcePoint, backHit.point, Color.red, 0.1f); }

        RaycastHit bottomFront, bottomBack;
        Vector3 frontPoint = transform.position + transform.forward * boundShortAngle.z;
        Vector3 backPoint = transform.position - transform.forward * boundShortAngle.z;
        bool bottomCheckFront = Physics.Raycast(frontPoint, -transform.up + transform.forward * boundShortAngle.y, out bottomFront, twoPointShortAlignDist, climbableSurfaces);
        bool bottomCheckBack = Physics.Raycast(backPoint, -transform.up - transform.forward * boundShortAngle.y, out bottomBack, twoPointShortAlignDist, climbableSurfaces);

        if (bottomCheckFront) { Debug.DrawLine(frontPoint, bottomFront.point, Color.yellow, 0.1f); }
        if (bottomCheckBack) { Debug.DrawLine(backPoint, bottomBack.point, Color.cyan, 0.1f); }

        /* 
        ground alignment is too close to surfaces and it causes to block climbing system
        while climbing , tick gets little bit up from ground and with that gap code decides to apply gravity
        */
        Grounded = FrontPhysic || BackPhysic || bottomCheckFront || bottomCheckBack || GroundChecking();

        if (mV > 0)
        {
            if (FrontPhysic)
            {
                AlignWithHit(frontHit);
            }
            else if (bottomCheckFront && Vector3.Distance(transform.position, bottomFront.point) >= Width / 2)
            {
                AlignWithHit(bottomFront);
            }
        }
        if (mV < 0)
        {
            //AlignWithHit(BackPhysic ? backHit : bottomBack);
            if (BackPhysic)
            {
                AlignWithHit(backHit);
            }
            else if (bottomCheckBack && Vector3.Distance(transform.position, bottomBack.point) >= Width / 2)
            {
                AlignWithHit(bottomBack);
            }
        }

    }
}