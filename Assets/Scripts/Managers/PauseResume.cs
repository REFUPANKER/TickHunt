using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PauseResume : NetworkBehaviour
{
    public bool Paused;
    public KeyCode PauseKey = KeyCode.P;

    public delegate void stateChanged(bool paused);
    public event stateChanged OnStateChanged;

    public bool ResumeAtStart = true;

    public List<GameObject> disableOnPause;

    void Start()
    {
        if (ResumeAtStart)
        {
            Resume();
        }
    }
    public void Pause()
    {
        Paused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = Paused;
        for (int i = 0; i < disableOnPause.Count; i++)
        {
            if (disableOnPause[i] != null)
            {
                disableOnPause[i]?.SetActive(false);
            }
        }
        OnStateChanged?.Invoke(true);
    }
    public void Resume()
    {
        Paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = Paused;
        for (int i = 0; i < disableOnPause.Count; i++)
        {
            if (disableOnPause[i] != null)
            {
                disableOnPause[i]?.SetActive(true);
            }
        }
        OnStateChanged?.Invoke(false);
    }

    public void Auto()
    {
        if (Paused) { Resume(); } else { Pause(); }
    }

    void Update()
    {
        if (!IsOwner) { return; }
        if (Input.GetKeyDown(PauseKey))
        {
            Auto();
        }
    }
}
