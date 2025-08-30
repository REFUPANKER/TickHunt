using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class tMovement : NetworkBehaviour
{
    public Player player;
    public CharacterController ctrl;
    public Animator animator;
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
}