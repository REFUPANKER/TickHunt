using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// simple movement code sample is in <br></br> <see cref="PerformStandartMovement"/>
/// </summary>
public class tMovement : NetworkBehaviour
{
    public Player player;
    public CharacterController ctrl;
    public Animator animator;
    public CinemachineVirtualCameraBase cam;

    public Vector3 moveDirection;
    public float MovementSpeed = 4;
    public float RotationSpeed = 200;
    public float gravity = -9.81f;

    public struct mStruct : INetworkSerializable
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 velocity;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref pos);
            s.SerializeValue(ref rot);
            s.SerializeValue(ref velocity);
        }
    }

    /// <summary>
    /// only call from server rpc methods
    /// </summary>
    public virtual mStruct GetNewValuesStruct()
    {
        mStruct newValues = new mStruct()
        {
            pos = transform.position,
            rot = transform.rotation,
            velocity = ctrl.velocity
        };
        return newValues;
    }
    public virtual void SetNewValues(mStruct newValues)
    {
        transform.position = newValues.pos;
        transform.rotation = newValues.rot;
        animator.SetFloat("Velocity", newValues.velocity.magnitude);
    }

    [ServerRpc]
    public void SyncServerRpc(mStruct newM)
    {
        player.nwMovement.Value = newM;
        SyncClientRpc(newM);
    }

    [ClientRpc]
    public void SyncClientRpc(mStruct newM)
    {
        if (!player.IsOwner)
        {
            SetNewValues(newM);
        }
    }

    public bool CheckCanMove()
    {
        return IsOwner && player.CanMove;
    }

    /// <summary>
    /// horizontal values rotates player
    /// </summary>
    /// <param name="CheckOwner"></param>
    public void PerformStandartMovement(bool CheckOwner = true)
    {
        if (CheckOwner && !CheckCanMove()) { return; }

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



    /// <summary>
    /// horizontal values , moves player
    /// </summary>
    /// <param name="CheckOwner"></param>
    public void PerformMovementOpt2(bool CheckOwner = true)
    {
        if (CheckOwner && !CheckCanMove()) { return; }

        float mV = Input.GetAxis("Vertical");
        float mH = Input.GetAxis("Horizontal");

        Vector3 m = transform.forward * mV + transform.right * mH;
        if (m.magnitude > 1) { m.Normalize(); }

        moveDirection.x = m.x * MovementSpeed;
        moveDirection.z = m.z * MovementSpeed;

        if (ctrl.isGrounded) { moveDirection.y = -2f; }
        else { moveDirection.y += gravity * Time.deltaTime; }

        ctrl.Move(moveDirection * Time.deltaTime);

        animator.SetFloat("Velocity", ctrl.velocity.magnitude);

        mStruct newMstruct = GetNewValuesStruct();
        SyncServerRpc(newMstruct);
    }
}