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
        Debug.LogError("Got condition: " + condition);
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
