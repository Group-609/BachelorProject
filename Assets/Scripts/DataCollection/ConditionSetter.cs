using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionSetter : MonoBehaviour
{
    public string condition;

    [Tooltip("Check if we should receive condition from the server")]
    public bool isLiveTest = true;

    [Tooltip("Force control condition")]
    public bool forceControl;

    [Tooltip("Force DDA condition")]
    public bool forceDDA;

    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void GetCondition(string condition)
    {
        this.condition = condition;
        if(condition == "Failed to receive condition from server!")
        {
            Debug.LogError(condition + " Please contact the developers!");
        }
    }

    public void ChangeCondition()
    {
        forceControl = !forceControl;
        forceDDA = !forceDDA;
    }

    public void ShouldGetConditionFromServer()
    {
        isLiveTest = true;
        forceControl = false;
        forceDDA = false;
    }

    public void OnControlConditionButtonClicked()
    {
        isLiveTest = false;
        forceControl = true;
        forceDDA = false;
    }

    public void OnDDAConditionButtonClicked()
    {
        isLiveTest = false;
        forceControl = false;
        forceDDA = true;
    }

    public bool IsDDACondition()
    {
        if (forceControl || forceDDA)
        {
            return forceDDA;
        }
        else
        {
            return condition == "DDA";
        }
    }
}
