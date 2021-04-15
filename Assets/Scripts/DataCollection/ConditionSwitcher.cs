using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Photon.Pun.Demo.PunBasics;


public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished(string gatheredData);

    [DllImport("__Internal")]
    private static extern void SecondConditionFinished(string gatheredData);
    float gameCloseDelay = 7.0f;

    private bool firstCondition = true;

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
    
    IEnumerator CallFirstConditionFinished()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        FirstConditionFinished(GetJsonToSend());
    }

    IEnumerator CallSecondConditionFinished()
    {
        yield return new WaitForSeconds(gameCloseDelay);
        SecondConditionFinished(GetJsonToSend());
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
        //Reload level with condition switched or sth. Take look at GameManager script. Might also need to reset the DDA system.
    }
}
