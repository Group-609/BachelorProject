using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class ConditionSwitcher : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FirstConditionFinished();

    public void SwitchCondition()
    {
        //TODO: show UI to fill out second part of survey
        FirstConditionFinished();
    }

    public void LoadNextLevel()
    {
        //Reload level with condition switched or sth. Take look at GameManager script. Might also need to reset the DDA system.
    }
}
