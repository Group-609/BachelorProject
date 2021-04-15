using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished();
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
            Invoke(nameof(FirstConditionFinished), gameCloseDelay);
        }
        if (LevelProgressionCondition.Instance.isGameFinished && !firstCondition)
        {
            Debug.Log("Second game finished!");
            //SecondConditionFinished();
        }

    }

    public void LoadNextLevel()
    {
        //Reload level with condition switched or sth. Take look at GameManager script. Might also need to reset the DDA system.
    }
}
