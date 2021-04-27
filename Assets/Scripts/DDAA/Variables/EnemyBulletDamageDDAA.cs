using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyBulletDamageDDAA : IDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static EnemyBulletDamageDDAA instance = null;
    private static readonly object padlock = new object();

    private EnemyBulletDamageDDAA() { }

    public static EnemyBulletDamageDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new EnemyBulletDamageDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float baseBulletDamagePoint = 10f;
    public static readonly float minBulletDamage = 15f;
    private static readonly float dpgContribution = 0.3f;
    private static readonly float bulletDamagePointContribution = 2f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] stunCountDiffPointAdditiveValues = new float[] { 2f, 1f, 0f, -1f, -2f }; // additive values to point directly
    private static readonly float[] stunCountDiffMultiplierAdditiveValues = new float[] { 0.5f, 0.2f, 0f, -0.2f, -0.5f }; // additive values to multiplier

    private static readonly float[] damageReceivedDiffPointAdditiveValues = new float[] { 2f, 1f, 0f, -1f, -2f }; // additive values to point directly
    private static readonly float[] damageReceivedDiffMultiplierAdditiveValues = new float[] { 0.5f, 0.2f, 0f, -0.2f, -0.5f }; // additive values to multiplier


    private static readonly float[] defeatedEnemiesDiffPointAdditiveValues = new float[] { -2f, -1f, 0f, 1f, 2f }; // additive values to point directly
    private static readonly float[] defeatedEnemiesDiffMultiplierAdditiveValues = new float[] { -0.5f, -0.2f, 0f, 0.2f, 0.5f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, they will change during the gameplay
    public float bulletDamageMultiplier = 1f;
    private float bulletDamagePoint = baseBulletDamagePoint;

    // THE IN-GAME VALUE USED
    public float bulletDamage = DDAEngine.CalculateInGameValue(baseBulletDamagePoint, bulletDamagePointContribution, dpgContribution, minBulletDamage);

    // This listener is important if some action has to take place, when the IN-GAME VALUE is changed.
    // Otherwise the variable, which uses it (e.g. PlayerController), will not be notified.
    private IValueChangeListener bulletDamageListener;

    public void SetBulletDamageListener(IValueChangeListener listener)
    {
        bulletDamageListener = listener;
    }
    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        // adjust multiplier and point values
        bulletDamagePoint = baseBulletDamagePoint * UpdatedMultiplier(); // possible to add value directly as well

        //set healing rate
        bulletDamage = DDAEngine.CalculateInGameValue(bulletDamagePoint, bulletDamagePointContribution, dpgContribution, minBulletDamage + addToInGameValue);

        if (bulletDamageListener != null)
        {
            bulletDamageListener.OnValueChanged(bulletDamage);
        }
    }

    private float UpdatedMultiplier()
    {
        bulletDamageMultiplier = Mathf.Max(
            0f,
            bulletDamageMultiplier +
            DDAEngine.GetAdditiveValue(
                StunCondition.Instance.ConditionValue,
                StunCondition.stunCountDiff,
                stunCountDiffMultiplierAdditiveValues
            ) +
            DDAEngine.GetAdditiveValue(
                DamageReceivedCondition.Instance.ConditionValue,
                DamageReceivedCondition.damageReceivedDiff,
                damageReceivedDiffMultiplierAdditiveValues
            )+
            DDAEngine.GetAdditiveValue(
                DefeatedEnemiesCountCondition.Instance.ConditionValue,
                DefeatedEnemiesCountCondition.defeatedEnemiesDiff,
                defeatedEnemiesDiffMultiplierAdditiveValues
            )
        );
        return bulletDamageMultiplier;
    }

    public void Reset()
    {
        bulletDamageMultiplier = 1f;
    }
}
