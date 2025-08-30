using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public bool CanMove = true;
    public PauseResume pauseResume;

    public TickMovement gTick;
    public PartridgeMovement gPartridge;

    public GameObject PickHeroScreen;
    public NetworkVariable<tMovement.mStruct> nwMovement = new NetworkVariable<tMovement.mStruct>();

    public override void OnNetworkSpawn()
    {
        PickHero(0);
        if (IsOwner)
        {
            pauseResume.OnPaused += () => CanMove = false;
            pauseResume.OnResumed += () => CanMove = true;
        }
        else
        {
            PickHeroScreen.SetActive(false);
            gTick.cam.enabled = false;
            gPartridge.cam.enabled = false;
        }
    }

    /// <summary>
    /// 1 : tick <br></br>
    /// 2 : partridge
    /// </summary>
    /// <param name="choice"></param>
    [ServerRpc]
    public void PickHeroServerRpc(int choice)
    {
        PickHero(choice);
        PickHeroClientRpc(choice);
    }

    [ClientRpc]
    public void PickHeroClientRpc(int choice)
    {
        PickHero(choice);
    }

    public void PickHero(int choice)
    {
        gTick.gameObject.SetActive(choice == 1);
        gPartridge.gameObject.SetActive(choice == 2);

        if (IsOwner && choice != 0)
        {
            gTick.cam.enabled = choice == 1;
            gPartridge.cam.enabled = choice == 2;

            PickHeroScreen.SetActive(false);
            pauseResume.Resume();
        }
    }
    public void PickHeroBtn(int choice)
    {
        if (IsOwner)
            PickHeroServerRpc(choice);
    }
}
