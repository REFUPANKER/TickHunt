using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    /// <summary>
    /// ----------- ENTERING GAME FLOW -----------
    /// 1- Decide the connection option (server,host,client)
    /// 2- Select Name
    ///     - After selecting name , player joins as spectator as default
    ///     and becomes visible on scoreboard as spectator
    /// 3- Select Hero
    /// </summary>
    public bool CanMove = true;
    public PauseResume pauseResume;

    public TickMovement gTick;
    public PartridgeMovement gPartridge;

    public GameObject PickHeroScreen;
    public GameObject NamingScreen;
    public TextMeshProUGUI NameWarnings;
    public TMP_InputField NameInput;

    public NetworkVariable<tMovement.mStruct> nwMovement = new NetworkVariable<tMovement.mStruct>();
    public NetworkVariable<bool> nwHeroSelected = new NetworkVariable<bool>();
    public NetworkVariable<int> nwMorphStatus = new NetworkVariable<int>();

    Canvas scoreboard;
    [SerializeField] ScoreboardItem scoreboardItemPrefab;
    ScoreboardItem scoreboardItem;
    bool canDisplayScoreboard;
    public enum MorphTypes
    {
        Spectator,
        Tick,
        Partirdge,
        InsectSprayer,
        Hunter
    }

    void Start()
    {
        GameObject sbObj = GameObject.FindGameObjectWithTag("Scoreboard");
        scoreboard = sbObj.GetComponent<Canvas>();
        scoreboard.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        PickHeroScreen.SetActive(false);
        gTick.gameObject.SetActive(false);
        gPartridge.gameObject.SetActive(false);
        gTick.cam.enabled = false;
        gPartridge.cam.enabled = false;

        if (IsOwner)
        {
            pauseResume.OnPaused += () => CanMove = false;
            pauseResume.OnResumed += () => CanMove = true;
        }
        else
        {
            NamingScreen.SetActive(false);
        }
    }
    void Update()
    {
        if (scoreboard != null && canDisplayScoreboard)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                scoreboard.enabled = true;
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                scoreboard.enabled = false;
            }
        }
    }

    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        gameObject.name = name;
        scoreboardItem = Instantiate(scoreboardItemPrefab);
        scoreboardItem.NetworkObject.Spawn();
        scoreboardItem.transform.SetParent(scoreboard.transform);
        scoreboardItem.SetValuesServerRpc(name, 0, 0, 0);
        nwMorphStatus.OnValueChanged += (int o, int n) => { scoreboardItem.SetMorph(n); };
        SetNameClientRpc(name);
    }
    [ClientRpc]
    public void SetNameClientRpc(string name)
    {
        gameObject.name = name;
    }

    public void SetNameBtn()
    {
        if (IsOwner)
        {
            if (String.IsNullOrEmpty(NameInput.text) || String.IsNullOrWhiteSpace(NameInput.text))
            {
                NameWarnings.text = "thats not allowed";
            }
            else
            {
                SetNameServerRpc(NameInput.text);
                canDisplayScoreboard = true;
                NamingScreen.SetActive(false);
                PickHeroScreen.SetActive(true);
            }
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
        gTick.gameObject.SetActive(choice == 1);
        gPartridge.gameObject.SetActive(choice == 2);
        nwMorphStatus.Value = choice;
        nwHeroSelected.Value = true;
        PickHeroClientRpc(choice);
    }

    [ClientRpc]
    public void PickHeroClientRpc(int choice)
    {
        gTick.gameObject.SetActive(choice == 1);
        gPartridge.gameObject.SetActive(choice == 2);
    }

    public void PickHeroBtn(int choice)
    {
        if (IsOwner)
        {
            PickHeroScreen.SetActive(false);
            pauseResume.Resume();
            PickHeroServerRpc(choice);
            gTick.cam.enabled = choice == 1;
            gPartridge.cam.enabled = choice == 2;
        }
    }



}
