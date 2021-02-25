using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ReloadDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static ReloadDDAA instance = null;
    private static readonly object padlock = new object();

    private ReloadDDAA(){}

    public static ReloadDDAA Instance
    {
        get
        {
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

    // Implementation for ReloadDDAA itself

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float minReloadTime = 2f;
    private static readonly float dpgContribution = 0.5f;
    private static readonly float pointContribution = 0.7f;

    //mutable parameters
    private float reloadMultiplier = 1f;
    private float reloadPoint = 1f;

    // THE IN-GAME VALUE USED
    // I gave it initial value as minimum reload time, though it could be any we wish (probably somewhere in the middle)
    private float reloadTime = minReloadTime;
}
