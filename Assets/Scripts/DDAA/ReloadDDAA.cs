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

    // Implementation for ReloadDDAA (parameters, used functions etc.)

    private static float minReloadTime = 2f;
    private float reloadTime = minReloadTime;
}
