using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerPainballDamageDDAA : IDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static PlayerPainballDamageDDAA instance = null;
    private static readonly object padlock = new object();

    private PlayerPainballDamageDDAA() { }

    public static PlayerPainballDamageDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new PlayerPainballDamageDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float basePaintballDamagePoint = 1f;
    public static readonly float minPaintballDamage = 2f;
    public static readonly float maxPaintballDamage = 20f;
    private static readonly float dpgContribution = 0.08f;
    private static readonly float paintballDamagePointContribution = 3.2f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] stunCountDiffMultiplierAdditiveValues = new float[] { -0.1f, -0.05f, 0f, 0.05f, 0.1f }; // additive values to multiplier
    private static readonly float[] damageReceivedDiffMultiplierAdditiveValues = new float[] { -0.1f, -0.05f, 0f, 0.05f, 0.1f }; // additive values to multiplier
    private static readonly float[] defeatedEnemiesDiffMultiplierAdditiveValues = new float[] { 0.2f, 0.1f, 0f, -0.1f, -0.2f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, they will change during the gameplay
    public float painballDamageMultiplier = 1f;
    private float paintballDamagePoint = basePaintballDamagePoint;

    // THE IN-GAME VALUE USED
    public float paintballDamage = DDAEngine.CalculateInGameValue(basePaintballDamagePoint, paintballDamagePointContribution, dpgContribution, minPaintballDamage);

    // This listener is important if some action has to take place, when the IN-GAME VALUE is changed.
    // Otherwise the variable, which uses it (e.g. PlayerController), will not be notified.
    private IValueChangeListener paintballDamageListener;

    public void SetPainballDamageListener(IValueChangeListener listener)
    {
        paintballDamageListener = listener;
    }
    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        // adjust multiplier and point values
        paintballDamagePoint = basePaintballDamagePoint * UpdatedMultiplier(); // possible to add value directly as well

        CalculateInGameValue(addToInGameValue);
    }

    private float UpdatedMultiplier()
    {
        painballDamageMultiplier = Mathf.Max(
            0f,
            painballDamageMultiplier +
            DDAEngine.GetAdditiveValue(
                StunCondition.Instance.ConditionValue,
                StunCondition.stunCountDiff,
                stunCountDiffMultiplierAdditiveValues
            ) +
            DDAEngine.GetAdditiveValue(
                DamageReceivedCondition.Instance.ConditionValue,
                DamageReceivedCondition.damageReceivedDiff,
                damageReceivedDiffMultiplierAdditiveValues
            ) +
            DDAEngine.GetAdditiveValue(
                DefeatedEnemiesCountCondition.Instance.ConditionValue,
                DefeatedEnemiesCountCondition.defeatedEnemiesDiff,
                defeatedEnemiesDiffMultiplierAdditiveValues
            )
        );
        return painballDamageMultiplier;
    }

    private void CalculateInGameValue(int addToInGameValue = 0)
    {
        //set healing rate
        paintballDamage = Mathf.Min(
            DDAEngine.CalculateInGameValue(paintballDamagePoint, paintballDamagePointContribution, dpgContribution, minPaintballDamage + addToInGameValue),
            maxPaintballDamage
        );

        if (paintballDamageListener != null)
        {
            paintballDamageListener.OnValueChanged(paintballDamage);
        }
    }

    public void Reset()
    {
        painballDamageMultiplier = 1f;
        paintballDamagePoint = basePaintballDamagePoint;
        CalculateInGameValue();
    }
}
