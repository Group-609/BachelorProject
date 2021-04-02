using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyMeleeDamageDDAA: IDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static EnemyMeleeDamageDDAA instance = null;
    private static readonly object padlock = new object();

    private EnemyMeleeDamageDDAA() { }

    public static EnemyMeleeDamageDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new EnemyMeleeDamageDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float baseMeleeDamagePoint = 1f;
    private static readonly float minMeleeDamage = 5f;
    private static readonly float dpgContribution = 0.1f;
    private static readonly float meleeDamagePointContribution = 1.5f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] stunCountDiff = new float[] { 0.5f, 0.75f, 0f, 1.25f, 1.5f }; // how many times were they faster than needed

    private static readonly float[] stunCountDiffPointAdditiveValues = new float[] { -2f, -1f, 0f, 1f, 2f }; // additive values to point directly
    private static readonly float[] stunCountDiffMultiplierAdditiveValues = new float[] { -2f, -1.5f, 0f, 1f, 2f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, they will change during the gameplay
    private float meleeDamageMultiplier = 1f;
    private float meleeDamagePoint = baseMeleeDamagePoint;

    // THE IN-GAME VALUE USED
    public float healingRate = DDAEngine.CalculateInGameValue(baseMeleeDamagePoint, meleeDamagePointContribution, dpgContribution, minMeleeDamage);

    // This listener is important if some action has to take place, when the IN-GAME VALUE is changed.
    // Otherwise the variable, which uses it (e.g. PlayerController), will not be notified.
    private IValueChangeListener healingListener;

    public void SetHealingListener(IValueChangeListener listener)
    {
        healingListener = listener;
    }
    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        meleeDamageMultiplier += DDAEngine.GetAdditiveValue(
                LevelProgressionCondition.Instance.ConditionValue,
                stunCountDiff,
                stunCountDiffMultiplierAdditiveValues
            );
        // adjust multiplier and point values
        meleeDamagePoint = baseMeleeDamagePoint * meleeDamageMultiplier; // possible to add value directly

        //set healing rate
        healingRate = DDAEngine.CalculateInGameValue(meleeDamagePoint, meleeDamagePointContribution, dpgContribution, minMeleeDamage + addToInGameValue);

        if (healingListener != null)
        {
            healingListener.OnValueChanged(healingRate);
        }
    }
}
