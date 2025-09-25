using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseResume : MonoBehaviour
{
    public bool Paused;
    public KeyCode PauseKey = KeyCode.P;

    public delegate void pausedEvent();
    public event pausedEvent OnPaused;

    public delegate void resumedEvent();
    public event resumedEvent OnResumed;

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
        OnPaused?.Invoke();
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
        OnResumed?.Invoke();
    }

    public void Auto()
    {
        if (Paused) { Resume(); } else { Pause(); }
    }

    void Update()
    {
        if (Input.GetKeyDown(PauseKey))
        {
            Auto();
        }
    }
}
