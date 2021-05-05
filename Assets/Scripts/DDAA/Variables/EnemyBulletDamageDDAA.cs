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
    private static readonly float baseBulletDamagePoint = 1f;
    public static readonly float minBulletDamage = 15f;
    public static readonly float maxBulletDamage = 80f;
    private static readonly float dpgContribution = 0.3f;
    private static readonly float bulletDamagePointContribution = 20f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] stunCountDiffMultiplierAdditiveValues = new float[] { 0.1f, 0.05f, 0f, -0.05f, -0.1f }; // additive values to multiplier
    private static readonly float[] damageReceivedDiffMultiplierAdditiveValues = new float[] { 0.1f, 0.05f, 0f, -0.05f, -0.1f }; // additive values to multiplier
    private static readonly float[] defeatedEnemiesDiffMultiplierAdditiveValues = new float[] { -0.05f, -0.025f, 0f, 0.025f, 0.05f }; // additive values to multiplier

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

        CalculateInGameValue(addToInGameValue);
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

    public void CalculateInGameValue(int addToInGameValue = 0)
    {
        //set bullet damage
        bulletDamage = Mathf.Min(
            DDAEngine.CalculateInGameValue(bulletDamagePoint, bulletDamagePointContribution, dpgContribution, minBulletDamage + addToInGameValue),
            maxBulletDamage
        );
            

        if (bulletDamageListener != null)
        {
            bulletDamageListener.OnValueChanged(bulletDamage);
        }
    }

    public void Reset()
    {
        bulletDamageMultiplier = 1f;
        bulletDamagePoint = baseBulletDamagePoint;
        CalculateInGameValue();
    }
}
