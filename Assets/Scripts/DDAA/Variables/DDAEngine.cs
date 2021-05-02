using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This class takes care of the calculating the in-game value by using formula.

    Also, it holds and takes care of Difficulty Point Global variable
*/

public static class DDAEngine
{
    public static bool isDynamicAdjustmentEnabled;

    public static float difficultiesPointGlobal = 10f;

    public static void ResetConditionsAndVariables()
    {
        difficultiesPointGlobal = 10f;
        LevelProgressionCondition.Instance.Reset();
        DefeatedEnemiesCountCondition.Instance.Reset();
        DamageReceivedCondition.Instance.Reset();
        StunCondition.Instance.Reset();
        EnemySpawnDDAA.Instance.Reset();
        HealingRateDDAA.Instance.Reset();
        PlayerPainballDamageDDAA.Instance.Reset();
        EnemyBulletDamageDDAA.Instance.Reset();
        EnemyMeleeDamageDDAA.Instance.Reset();
    }
    
    public static void AdjustDPG(float conditionValue, float[] conditionalValues, float[] additiveValues)
    {
        difficultiesPointGlobal = Mathf.Max(
            0f,
            difficultiesPointGlobal + GetAdditiveValue(conditionValue, conditionalValues, additiveValues)
        );
    }

    // Based on ConditionalValue this function should return what adjustment should be done to DDAA's multiplier/point
    public static float GetAdditiveValue(float conditionValue, float[] conditionalValues, float[] additiveValues)
    {
        //conditionValue = 1.6
        //conditionalValues = { 0.5, 0.75, 1.25, 1.5 }
        //additiveValues = { -1, -0.5, 0, 0.5, 1 }
        if (conditionalValues.Length + 1 != additiveValues.Length)
            Debug.LogError("IMPORTANT! Conditional and additive values are not of the required length");

        for (int i = 0; i < additiveValues.Length; i++)
        {
            float? lower = null;
            float? upper = null;

            if (i > 0)
                lower = conditionalValues[i - 1];
            if (i < conditionalValues.Length - 1)
                upper = conditionalValues[i];

            //conditionalValues.Length = 4
            //additiveValues.Length = 5

            // i = 4
            //1.5 <= 1.6 <= null
            if (conditionValue.Between(lower, upper))
                // 1
                return additiveValues[i];
        }
        Debug.LogError("IMPORTANT! No additive value found. Something went wrong with the system");
        return 0;
    }

    public static float CalculateInGameValue(float point, float pointContribution, float dpgContribution, float minValue = 0f) 
    {
        return minValue + (point * pointContribution) + (difficultiesPointGlobal * dpgContribution);
    }
}
