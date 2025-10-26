using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

    // public TickMovement gTick;
    // public PartridgeMovement gPartridge;

    /// <summary>
    /// order must be like this<br></br>
    /// Tick<br></br>
    /// Partridge<br></br>
    /// Hunter<br></br>
    /// InsectSprayer<br></br>
    /// </summary>
    [Tooltip("Order : Tick,Partridge,Hunter,InsectSprayer")]
    public tMovement[] Heroes;

    public GameObject PickHeroScreen;
    public GameObject NamingScreen;// temporrary
    public TextMeshProUGUI NameWarnings;
    public TMP_InputField NameInput;

    [Header("Health system")]
    public Canvas HealthWorldUi;
    public Slider HealthBarUi, HealthBarWorld;

    [Header("Network variables")]
    public NetworkVariable<tMovement.mStruct> nwMovement = new NetworkVariable<tMovement.mStruct>();
    public NetworkVariable<bool> nwHeroSelected = new NetworkVariable<bool>();
    public NetworkVariable<int> nwMorphStatus = new NetworkVariable<int>();


    [Header("Scoreboard")]
    [SerializeField] ScoreboardItem scoreboardItemPrefab;
    ScoreboardItem scoreboardItem;
    bool canDisplayScoreboard;

    [Header("SOAM")]
    public SceneObjectAccessManager soam;

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
        HealthBarUi.gameObject.SetActive(false);
        HealthBarWorld.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NamingScreen.SetActive(false);
        PickHeroScreen.SetActive(false);
        if (IsOwner)
        {
            pauseResume.OnPaused += () => CanMove = false;
            pauseResume.OnResumed += () => CanMove = true;
            nwMorphStatus.OnValueChanged += (int o, int n) => { if (scoreboardItem != null) { scoreboardItem.SetMorph(n); } };
        }
        Morph(-1); // disable all hero objects for hero selection system
        StartCoroutine(SpawnInterval());
    }
    IEnumerator SpawnInterval()
    {
        yield return null;
        FindSoam();
        if (IsOwner)
        {
            //TODO: dont forget to change this (username should be readed from json file)
            // players shouldnt have to rename their selfs whenever join to game
            // this structure for auto naming the user
            System.Random r = new System.Random();
            string randomName = "User ";
            for (int i = 0; i < 4; i++) { randomName += Convert.ToChar(r.Next(65, 91)); }
            SetNameCaller(randomName);

            NamingScreen.SetActive(false);
            PickHeroScreen.SetActive(true);// disable for setting player name in game
        }
    }

    void Update()
    {
        if (soam != null)
        {
            if (soam.scoreboard != null && canDisplayScoreboard)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    soam.scoreboard.alpha = 1;
                }
                else if (Input.GetKeyUp(KeyCode.Tab))
                {
                    soam.scoreboard.alpha = 0;
                }
            }
        }
    }
    void FindSoam()
    {
        if (soam != null) { return; }
        GameObject findSoam = GameObject.FindGameObjectWithTag("SOAM");
        soam = findSoam.GetComponent<SceneObjectAccessManager>();
    }
    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        gameObject.name = name;
        scoreboardItem = Instantiate(scoreboardItemPrefab);
        scoreboardItem.NetworkObject.Spawn();
        scoreboardItem.transform.SetParent(soam.scoreboard.transform);
        scoreboardItem.SetValuesServerRpc(name, 0, 0, 0);
        SetNameClientRpc(name);
    }
    [ClientRpc]
    public void SetNameClientRpc(string name)
    {
        gameObject.name = name;
    }
    void SetNameCaller(string name)
    {
        SetNameServerRpc(name);
        NamingScreen.SetActive(false);
        PickHeroScreen.SetActive(true);
        canDisplayScoreboard = true;
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
                SetNameCaller(NameInput.text);
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
        Morph(choice);
        nwMorphStatus.Value = choice;
        nwHeroSelected.Value = true;
        PickHeroClientRpc(choice);
    }

    [ClientRpc]
    public void PickHeroClientRpc(int choice)
    {
        Morph(choice);
    }

    public void PickHeroBtn(int choice)
    {
        if (IsOwner)
        {
            PickHeroScreen.SetActive(false);
            pauseResume.Resume();
            PickHeroServerRpc(choice);
            Heroes[choice - 1].cam.enabled = true;
        }
    }

    public void Morph(int choice)
    {
        if (choice == -1 || choice > Heroes.Length)
        {
            for (int i = 0; i < Heroes.Length; i++)
            {
                Heroes[i].cam.enabled = false;
                Heroes[i].gameObject.SetActive(false);
            }
        }
        else
        {
            Heroes[choice - 1].gameObject.SetActive(true);
        }
    }

}
