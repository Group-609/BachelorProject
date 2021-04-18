using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;

public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished(string gatheredData);

    [DllImport("__Internal")]
    private static extern void SecondConditionFinished(string gatheredData);
    float gameCloseDelay = 10.0f;

    private string secondConditionSceneName = "SecondConditionLauncher";

    private bool firstCondition = true;

    private ConditionSetter conditionSetter;

    void Start()
    {
        conditionSetter = GameObject.Find("ConditionSetter").GetComponent<ConditionSetter>();
        DontDestroyOnLoad(transform.gameObject);
    }

    void Update()
    {
        //Debug.Log("LevelProgressionCondition.Instance.isGameFinished: " + LevelProgressionCondition.Instance.isGameFinished + " firstCondition: " + firstCondition);
        if (LevelProgressionCondition.Instance.isGameFinished && firstCondition)
        {
            Debug.Log("First game finished!");
            firstCondition = false;
            LevelProgressionCondition.Instance.isGameFinished = false;
            StartCoroutine(CallFirstConditionFinished());
        }
        if (LevelProgressionCondition.Instance.isGameFinished && !firstCondition)
        {
            Debug.Log("Second game finished!");
            StartCoroutine(CallSecondConditionFinished());
        }

    }

    private IEnumerator ChangeConditionAndLoadSecondCondition()
    {
        conditionSetter.ChangeCondition();
        Debug.Log("Changed condition. Is DDA condition: " + conditionSetter.IsDDACondition());
        yield return new WaitForSeconds(2f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(secondConditionSceneName);
        }
        yield return null;
    }

    IEnumerator CallFirstConditionFinished()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        //FirstConditionFinished(GetJsonToSend());
        LoadNextLevel();
    }

    IEnumerator CallSecondConditionFinished()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        //SecondConditionFinished(GetJsonToSend());
    }

    string GetJsonToSend()
    {
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach(GameObject player in players)
        {
            if(player.GetComponent<PlayerManager>().IsPlayerLocal())
            {
                return player.GetComponent<PlayerDataRecorder>().GetJsonToSend();
            }
        }
        return "ERROR: Didnt find player data";
    }


    public void LoadNextLevel()
    {
        StartCoroutine(ChangeConditionAndLoadSecondCondition());
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        players.ForEach(player =>
        {
            player.GetComponent<PlayerManager>().SetMouseLock(false);
        });
        //Reload level with condition switched or sth. Take look at GameManager script. Might also need to reset the DDA system.
    }
}
