using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Interface, which defines the generic structure of every specific condition to be implemented */
public interface ICondition
{
    // Value holder for the current value of the condition. 
    // This value is set from object, which controls some variable in DDAA, so adjustment is started.
    float ConditionValue { get; set; }
}
