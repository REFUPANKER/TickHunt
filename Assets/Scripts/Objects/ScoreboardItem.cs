using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardItem : NetworkBehaviour
{
    //TODO: add > when user disconnects , destroy its scoreboard item
    [SerializeField] TextMeshProUGUI tName, tKill, tDeath;
    [SerializeField] Image iMorph;

    [SerializeField] Sprite[] morphIcons;

    public void SetValues(string name, int kill, int death, int morph)
    {
        tName.text = name;
        tKill.text = kill.ToString();
        tDeath.text = death.ToString();
        iMorph.sprite = morphIcons[morph];
    }

    [ServerRpc]
    public void SetValuesServerRpc(string name, int kill, int death, int morph)
    {
        SetValues(name, kill, death, morph);
        SetValuesClientRpc(name, kill, death, morph);
    }

    [ClientRpc]
    void SetValuesClientRpc(string name, int kill, int death, int morph)
    {
        SetValues(name, kill, death, morph);
    }

    public void SetKill(int kill) => tKill.text = kill.ToString();
    public void SetDeath(int death) => tDeath.text = death.ToString();
    public void SetMorph(int morph) => SetMorphServerRpc(morph);
    public void SetName(FixedString32Bytes username) => SetNameServerRpc(username);

    [ServerRpc]
    public void SetMorphServerRpc(int morph)
    {
        iMorph.sprite = morphIcons[morph - 1];
        SetMorphClientRpc(morph);
    }

    [ClientRpc]
    void SetMorphClientRpc(int morph)
    {
        iMorph.sprite = morphIcons[morph - 1];
    }
    [ServerRpc]
    public void SetNameServerRpc(FixedString32Bytes username)
    {
        tName.text = username.ToString();
        SetNameClientRpc(username);
    }

    [ClientRpc]
    void SetNameClientRpc(FixedString32Bytes username)
    {
        tName.text = username.ToString();
    }
}
