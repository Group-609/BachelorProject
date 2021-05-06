using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished(string gatheredData);

    [DllImport("__Internal")]
    private static extern void SecondConditionFinished(string gatheredData);
    float gameCloseDelay = 10.0f;

    private readonly string secondConditionSceneName = "SecondConditionLauncher";

    private bool isFirstCondition = true;
    private bool bothConditionsFinished = false;

    public bool shouldSendDataToServer;

    private ConditionSetter conditionSetter;

    void Start()
    {
        conditionSetter = GetComponent<ConditionSetter>();
        DontDestroyOnLoad(transform.gameObject);
    }

    void Update()
    {
        if (!bothConditionsFinished)
        {
            if (LevelProgressionCondition.Instance.isGameFinished && isFirstCondition)
            {
                Debug.Log("First game finished!");
                isFirstCondition = false;
                LevelProgressionCondition.Instance.isGameFinished = false;
                StartCoroutine(ShowFirstConditionEnd());
                if (!conditionSetter.shouldChangeCondition)
                {
                    bothConditionsFinished = true;
                    //TODO show some "YOU WIN" screen or something
                }

            }
            if (LevelProgressionCondition.Instance.isGameFinished && !isFirstCondition)
            {
                Debug.Log("Second game finished!");
                StartCoroutine(ShowSecondConditionEnd());

                //TODO show some "YOU WIN" screen or something
                bothConditionsFinished = true;
            }
        }
    }

    private IEnumerator ChangeConditionAndLoadSecondCondition()
    {
        conditionSetter.ChangeCondition();
        Debug.Log("Changed condition. Is DDA condition: " + conditionSetter.IsDDACondition());

        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        UnlockPlayersMouse(players);
        StopMusic(players);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(secondConditionSceneName);
        }
        yield return null;
    }
    private void StopMusic(List<GameObject> players)
    {
        players.ForEach(player => player.GetComponent<PlayerManager>().DisableMusic());
    }

    private void UnlockPlayersMouse(List<GameObject> players)
    {
        players.ForEach(player => player.GetComponent<PlayerManager>().SetMouseLock(false));
    }

    IEnumerator ShowFirstConditionEnd()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        ShowFirstConditionEndScreen();
    }

    public void ShowFirstConditionEndScreen()
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(false);
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = true;
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().areSettingsEnabled = true;
        GameObject overlay = GameObject.Find("Canvas/Player HUD/Condition1EndOverlay");
        overlay.SetActive(true);
        Button button = overlay.transform.Find("Button").GetComponent<Button>();
        button.onClick.AddListener(EndFirstCondition);
    }

    public void EndFirstCondition()
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(true);
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = false;
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().areSettingsEnabled = false;
        GameObject overlay = GameObject.Find("Canvas/Player HUD/Condition1EndOverlay");
        overlay.SetActive(false);
        if (!Application.isEditor && shouldSendDataToServer)
            FirstConditionFinished(GetJsonToSend());
        else Debug.Log("Json data to send: " + GetJsonToSend());
        if (conditionSetter.shouldChangeCondition)
            StartCoroutine(ChangeConditionAndLoadSecondCondition());
    }

    public void EndSecondCondition()
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(true);
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = false;
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().areSettingsEnabled = false;
        GameObject overlay = GameObject.Find("Canvas/Player HUD/Condition2EndOverlay");
        overlay.SetActive(false);
        StartCoroutine(CloseAfterSecondCondition());
    }

    IEnumerator CloseAfterSecondCondition()
    {
        if (!Application.isEditor && shouldSendDataToServer)
            SecondConditionFinished(GetJsonToSend());
        else Debug.Log("Json data to send: " + GetJsonToSend());
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        UnlockPlayersMouse(players);
        StopMusic(players);
        yield return new WaitForSeconds(gameCloseDelay);
        Application.Quit();
    }

    IEnumerator ShowSecondConditionEnd()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        ShowSecondConditionEndScreen();
    }

    public void ShowSecondConditionEndScreen()
    {
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().SetMouseLock(false);
        PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().areSettingsEnabled = true;
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().areSettingsEnabled = true;
        GameObject overlay = GameObject.Find("Canvas/Player HUD/Condition2EndOverlay");
        overlay.SetActive(true);
        Button button = overlay.transform.Find("Button").GetComponent<Button>();
        button.onClick.AddListener(EndSecondCondition);
    }

    public void SetSendDataToServer(bool shouldSendData)
    {
        shouldSendDataToServer = shouldSendData;
    }

    string GetJsonToSend()
    {
        if(PlayerManager.LocalPlayerInstance != null)
        {
            return PlayerManager.LocalPlayerInstance.GetComponent<PlayerDataRecorder>().GetJsonToSend();
        }
        return "ERROR: Didnt find player data";
    }
}
