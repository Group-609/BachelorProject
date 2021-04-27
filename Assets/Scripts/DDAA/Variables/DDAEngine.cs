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
        LevelProgressionCondition.Instance.Reset();
        DefeatedEnemiesCountCondition.Instance.Reset();
        DamageReceivedCondition.Instance.Reset();
        StunCondition.Instance.Reset();
        EnemySpawnDDAA.Instance.Reset();
        HealingRateDDAA.Instance.Reset();
        PlayerPainballDamageDDAA.Instance.Reset();
        EnemyBulletDamageDDAA.Instance.Reset();
        EnemyMeleeDamageDDAA.Instance.Reset();

        difficultiesPointGlobal = 10f;
    }

    public static void AdjustDPG(float conditionValue, int[] conditionalValues, float[] additiveValues)
    {
        difficultiesPointGlobal = Mathf.Max(
            0f,
            difficultiesPointGlobal + GetAdditiveValue(conditionValue, conditionalValues, additiveValues)
        );
    }
    
    public static void AdjustDPG(float conditionValue, float[] conditionalValues, float[] additiveValues)
    {
        difficultiesPointGlobal = Mathf.Max(
            0f,
            difficultiesPointGlobal + GetAdditiveValue(conditionValue, conditionalValues, additiveValues)
        );
    }

    // Based on ConditionalValue this function should return what adjustment should be done to DDAA's multiplier/point
    public static float GetAdditiveValue(float conditionValue, int[] conditionalValues, float[] additiveValues)
    {
        for (int i = 0; i < conditionalValues.Length; i++)
        {
            if (conditionalValues[i] >= conditionValue)
                return additiveValues[i];
        }
        return additiveValues[additiveValues.Length - 1];
    }

    // Based on ConditionalValue this function should return what adjustment should be done to DDAA's multiplier/point
    public static float GetAdditiveValue(float conditionValue, float[] conditionalValues, float[] additiveValues)
    {
        for (int i = 0; i < conditionalValues.Length; i++)
        {
            if (conditionalValues[i] >= conditionValue)
                return additiveValues[i];
        }
        return additiveValues[additiveValues.Length - 1];
    }

    public static float CalculateInGameValue(float point, float pointContribution, float dpgContribution, float minValue = 0f) 
    {
        return minValue + (point * pointContribution) + (difficultiesPointGlobal * dpgContribution);
    }
}
