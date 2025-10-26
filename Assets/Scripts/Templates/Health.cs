using Unity.Netcode;
using UnityEngine;

public class tHealth : NetworkBehaviour
{
    /*
        ---Instruction---
        + simple health script 
        + players can get effected by something 
        and it can keep effecting for a while 

        -- Regeneration [Ability for Partridge as default] (optional : it can be triggered with collecting items)


        ---------- what happens if heroes get hit by dart ----------
        tick : dies
        partridge : gets fainted for X seconds (sprayer can help to revive)
        insect sprayer : per X shots adds more slowness effect for X seconds
        hunter : hunter morph is limited with 1 player and player cant hit itself

        ---------- what happens if heroes get sprayed ----------
        tick : slowness effect and damage (increased by the time stayed in sprayed place) 
        partridge : slight slowness effect with health boost (limited with X health point)
        insect sprayer : doesnt effects to self
        hunter : gets slowness and damage but less than tick
    */

    [SerializeField] Player player;

    private int defaultHealth;
    public int Health;

    public Player.MorphTypes morph;

    [Tooltip("World ui setup")]
    public Vector3 WorldUiMargin;
    public Vector2 WorldUiSize;

    public AudioSource sfxDamage, sfxHeal, sfxDeath, sfxEffected;


    public virtual void Start()
    {
        defaultHealth = Health;
        player.HealthBarUi.value = player.HealthBarUi.maxValue = defaultHealth;
        player.HealthBarWorld.value = player.HealthBarWorld.maxValue = defaultHealth;
        player.HealthWorldUi.transform.SetParent(transform);
        RectTransform wrt = player.HealthWorldUi.GetComponent<RectTransform>();
        wrt.sizeDelta = WorldUiSize;
        player.HealthWorldUi.transform.position = WorldUiMargin;
        if (IsOwner)
        {
            if (morph != Player.MorphTypes.Spectator)// ui and world bars are disabled on Player.Start() 
            {
                SetPanelsVisibility(true, false);
                SetPanelsServerRpc();
            }
        }
    }

    [ServerRpc]
    void SetPanelsServerRpc()
    {
        SetPanelsClientRpc();
    }
    [ClientRpc]
    void SetPanelsClientRpc()
    {
        if (!IsOwner)
        {
            SetPanelsVisibility(false, true);
        }
    }

    void SetPanelsVisibility(bool uiBar, bool worldBar)
    {
        player.HealthBarUi.gameObject.SetActive(uiBar);
        player.HealthBarWorld.gameObject.SetActive(worldBar);
    }

    ///<summary> call base functions </summary> 
    public virtual void Damage(int damage)
    {
        // Animations
        if (sfxDamage != null) { sfxDamage.Play(); }

        SetHealthServerRpc(Health - damage);
        if (Health - damage <= 0)
        {
            sfxDeath?.Play();
        }
    }

    ///<summary> call base functions </summary> 
    public virtual void Heal(int heal)
    {
        // Animations
        sfxHeal?.Play();
        SetHealthServerRpc(Health + heal);
    }

    private void ReFill()
    {
        sfxHeal?.Play();
        SetHealthServerRpc(defaultHealth);
    }

    ///<summary> Base functions uses sfxEffected audio source</summary>
    public virtual void Effect_Poision()
    {
        sfxEffected?.Play();
    }

    ///<summary> Base functions uses sfxEffected audio source</summary>
    public virtual void Effect_Fainted()
    {
        sfxEffected?.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(int healthPoint)
    {
        int val = Mathf.Clamp(healthPoint, 0, defaultHealth);
        Health = val;
        DisplayHealth(val);
        SetHealthClientRpc(val);
    }
    [ClientRpc]
    private void SetHealthClientRpc(int healthPoint)
    {
        if (!IsServer)
        {
            Health = healthPoint;
            DisplayHealth(healthPoint);
        }
    }

    private void DisplayHealth(int health)
    {
        player.HealthBarUi.value = health;
        player.HealthBarWorld.value = health;
    }
}
