using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ReloadDDAA: IDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static ReloadDDAA instance = null;
    private static readonly object padlock = new object();

    private ReloadDDAA() { }

    public static ReloadDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ReloadDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float baseReloadPoint = 1f;
    private static readonly float minReloadTime = 2f;
    private static readonly float dpgContribution = 0.5f;
    private static readonly float reloadPointContribution = 1f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly int[] killCountValues = new int[] { 0, 5, 10, 15, 20 }; // kill count numbers at which the DDAA's additive values would be mapped

    private static readonly float[] killCountPointAdditiveValues = new float[] { -1, 0, 1, 2, 3 }; // additive values to point directly
    private static readonly float[] killCountMultiplierAdditiveValues = new float[] { -0.2f, 0f, 0.2f, 0.4f, 0.6f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, it will change during the gameplay
    private float reloadMultiplier = 1f; 
    private float reloadPoint = baseReloadPoint; 

    // THE IN-GAME VALUE USED
    // I gave it initial value as minimum reload time, though it could be any we wish (probably somewhere in the middle)
    public float reloadTime = DDAEngine.Instance.CalculateInGameValue(baseReloadPoint, reloadPointContribution, dpgContribution, minReloadTime);

    // This listener is important if some action has to take place, when the reload time is changed.
    // Otherwise the variable, which holds reload time, will not be notified.
    private IValueChangeListener reloadListener;

    public void SetReloadListener(IValueChangeListener listener)
    {
        reloadListener = listener;
    }

    public void AdjustInGameValue()
    {
        reloadMultiplier += DDAEngine.Instance.GetAdditiveValue(
                KillCountCondition.Instance.ConditionValue, 
                killCountValues, 
                killCountMultiplierAdditiveValues
            );
        // adjust multiplier and point values
        reloadPoint = baseReloadPoint * reloadMultiplier; // possible to add value directly
        Debug.Log("Reload point = " + reloadPoint);

        //set reloadTime
        reloadTime = DDAEngine.Instance.CalculateInGameValue(reloadPoint, reloadPointContribution, dpgContribution, minReloadTime);

        if (reloadListener != null)
        {
            reloadListener.OnValueChanged(reloadTime);
        }
    }
}

