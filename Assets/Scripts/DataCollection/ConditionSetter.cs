using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionSetter : MonoBehaviour
{
    public string condition;

    [Tooltip("Check if both conditions should be played simultaneously")]
    public bool shouldChangeCondition;

    [Tooltip("Check if we should receive condition from the server")]
    public bool receiveConditionFromServer = true;

    [Tooltip("Force control condition")]
    public bool forceControl;

    [Tooltip("Force DDA condition")]
    public bool forceDDA;

    private readonly string ddaString = "DDA";
    private readonly string controlString = "control";

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
        //if DDA was enabled in the first condition, we want to force Control condition in the second condition
        forceControl = DDAEngine.isDynamicAdjustmentEnabled;
        forceDDA = !DDAEngine.isDynamicAdjustmentEnabled;
        condition = DDAEngine.isDynamicAdjustmentEnabled ? controlString : ddaString;
    }

    public void ShouldGetConditionFromServer()
    {
        receiveConditionFromServer = true;
        forceControl = false;
        forceDDA = false;
    }

    public void OnControlConditionButtonClicked()
    {
        receiveConditionFromServer = false;
        forceControl = true;
        forceDDA = false;
    }

    public void OnDDAConditionButtonClicked()
    {
        receiveConditionFromServer = false;
        forceControl = false;
        forceDDA = true;
    }

    public void SetChangingCondition(bool shouldChangeCondition)
    {
        this.shouldChangeCondition = shouldChangeCondition;
    }

    //This should only be called by master. Master gives the condition to all players
    public bool IsDDACondition()
    {
        if (forceControl || forceDDA)
        {
            return forceDDA;
        }
        else
        {
            return condition == ddaString;
        }
    }
}
