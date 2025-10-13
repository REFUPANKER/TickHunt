using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class tHealth : NetworkBehaviour
{
    /*
        ---Instruction---
        + simple health script 
        + players can get effected by something 
        and it can keep effecting for a while 
        so it contains ApplyEffect method

        -- ApplyEffect 
        > tick , hunter : gets poisioned 
            - InsectSprayer s spray , effetcs both
        > partridge , insectSprayer : gets fainted by hunter s darts
            - InsectSprayer needs to get X darts to get fainted
            - Single dart can faint partridge

        -- Regeneration [Ability for Partridge as default] (optional : it can be triggered with collecting items)
    */

    [SerializeField] Player player;

    private int defaultHealth;
    [SerializeField] NetworkVariable<int> Health = new NetworkVariable<int>();

    public Canvas WorldUi;
    public Slider UiHealthBar, WorldHealthBar;

    /// <summary>
    /// follow this order
    /// <list type="number">
    /// <item><description>Tick</description></item>
    /// <item><description>Partridge</description></item>
    /// <item><description>InsectSpraye</description></item>
    /// <item><description>Hunte</description></item>
    /// </list>
    /// </summary>
    [Tooltip("World ui setup")]
    public Vector3[] WorldUiMargins;
    public Vector2[] WorldUiSizes;
    void Start()
    {
        WorldUi.gameObject.SetActive(false);
        UiHealthBar.gameObject.SetActive(false);
        WorldHealthBar.gameObject.SetActive(false);
    }
    public override void OnNetworkSpawn()
    {
        player.HeroSelected += HeroSelected;
    }

    void HeroSelected(Player.MorphTypes morph, Transform hero)
    {
        if (morph == Player.MorphTypes.Spectator)
        {
            UiHealthBar.enabled = false;
            WorldHealthBar.enabled = false;
            player.HeroSelected -= HeroSelected;
            return;
        }
        
        WorldUi.transform.SetParent(hero);
        WorldUi.gameObject.SetActive(!IsOwner);

        UiHealthBar.gameObject.SetActive(true);
        WorldHealthBar.gameObject.SetActive(true);
        defaultHealth = Health.Value;
        UiHealthBar.maxValue = Health.Value;
        WorldHealthBar.maxValue = Health.Value;
        UiHealthBar.value = Health.Value;
        WorldHealthBar.value = Health.Value;

        RectTransform wrt = WorldUi.GetComponent<RectTransform>();
        wrt.sizeDelta = WorldUiSizes[(int)morph - 1];
        WorldUi.transform.position = WorldUiMargins[(int)morph - 1];

        player.HeroSelected -= HeroSelected;
    }

    public void Damage(int damage)
    {
        // Animations
        SetHealthServerRpc(Health.Value - damage);
    }

    public void Heal(int heal)
    {
        // Animations
        SetHealthServerRpc(Health.Value + heal);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(int healthPoint)
    {
        Health.Value = Mathf.Clamp(healthPoint, 0, defaultHealth);
        DisplayHealth();
    }

    private void ReFill()
    {
        SetHealthServerRpc(defaultHealth);
    }

    private void DisplayHealth()
    {
        if (IsOwner)
        {
            UiHealthBar.value = Health.Value;
        }
        else
        {
            WorldHealthBar.value = Health.Value;
        }
    }
}
