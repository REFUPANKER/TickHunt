using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PartridgeMovement : tMovement
{
    void Update()
    {
        if (!CheckCanMove()) { return; }
        PerformStandartMovement();
    }
}
