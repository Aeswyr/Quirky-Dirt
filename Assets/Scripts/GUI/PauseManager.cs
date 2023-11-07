using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    [SerializeField] private GameObject pauseParent;

    public void SetActive(bool toggle) {
        pauseParent.SetActive(toggle);
    }

    public void OnQuit() {
        Application.Quit();
    }
}
