using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished();

    private bool firstCondition = true;

    void Update()
    {
        if (LevelProgressionCondition.Instance.isGameFinished && firstCondition)
        {
            Debug.Log("First game finished!");
            firstCondition = false;
            LevelProgressionCondition.Instance.isGameFinished = false;
            FirstConditionFinished();
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
