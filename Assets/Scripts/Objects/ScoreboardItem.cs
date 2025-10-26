using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardItem : NetworkBehaviour
{
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

    [ServerRpc]
    public void SetMorphServerRpc(int morph)
    {
        iMorph.sprite = morphIcons[morph-1];
        SetMorphClientRpc(morph);
    }

    [ClientRpc]
    void SetMorphClientRpc(int morph)
    {
        iMorph.sprite = morphIcons[morph-1];
    }
}
