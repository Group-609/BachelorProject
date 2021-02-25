using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Interface, which defines the generic structure of every specific condition to be implemented */
public interface ICondition
{
    // value holder for the current value of the condition
    float ConditionValue { get; set; }

    // based on ConditionalValues this function should return what adjustment should be done to DDAA's multiplier
    float GetAdditiveValue(int[] conditionalValues, float[] additiveValues);
}
