using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionSetter : MonoBehaviour
{
    public string condition;

    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void GetCondition(string condition)
    {
        this.condition = condition;
        if(condition == "Failed to receive condition from server!")
        {
            Debug.LogError(condition + " Please contant the developers!");
        }
        
    }

    public bool IsDDACondition()
    {
        if (condition == "DDA")
        {
            return true;
        }
        else return false;
    }
}
