using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;
using UnityStandardAssets.Characters.FirstPerson;

public class SettingsController : MonoBehaviour
{
    [SerializeField]
    GameObject settingsWindow;
    bool settingsEnabled;


    void Start()
    {
        settingsWindow.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown("o"))
        {
            if (settingsEnabled)
            {
                settingsWindow.SetActive(false);
                PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(true);
                PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = false;
            } 
            else
            {
                settingsWindow.SetActive(true);
                PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(false);
                PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = true;
            }
            settingsEnabled = !settingsEnabled;
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().musicVolume = value;
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().ChangeBackgroundMusic();
    }

    public void SetSfxVolume(float value)
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().volume = value;
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetAudioLevel();
    }

    public void SetMouseSensitivity(float value)
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().mouseSensitivity = value;
    }
}
