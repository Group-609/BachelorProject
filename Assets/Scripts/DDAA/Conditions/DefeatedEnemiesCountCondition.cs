using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class DefeatedEnemiesCountCondition : ICondition
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static DefeatedEnemiesCountCondition instance = null;
    private static readonly object padlock = new object();

    private DefeatedEnemiesCountCondition() { }

    public static DefeatedEnemiesCountCondition Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new DefeatedEnemiesCountCondition();
                }
                return instance;
            }
        }
    }

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    public static readonly float[] defeatedEnemiesDiff = new float[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f }; // how many times did the player defeat more enemies than other players
    private static readonly float[] dpgAdditiveValues = new float[] { 1.5f, 1f, 0f, -1f, -1.5f }; // additive values to DPG point

    public int localPlayerDefeatsCount = 0;

    private float comparisonWithTeam = 1f; // bigger value - better player

    public float ConditionValue
    {
        get => comparisonWithTeam;
        set
        {
            // should start the configuration of every DDAA here
            comparisonWithTeam = value;
            DDAEngine.AdjustDPG(value, defeatedEnemiesDiff, dpgAdditiveValues);
        }
    }

    public void UpdateConditionalValue(List<GameObject> teamPlayers)
    {
        if (teamPlayers.Count > 0)
        {
            int totalTeamDefeatedEnemiesCount = 1; //we add 1 to not devide by 0. This is mathematically valid
            teamPlayers.ForEach(player =>
            {
                totalTeamDefeatedEnemiesCount += player.GetComponent<PlayerManager>().defeatedEnemiesCount;
                player.GetComponent<PlayerManager>().defeatedEnemiesCount = 0;
            });
            float teamDefeatedEnemiesAverage = totalTeamDefeatedEnemiesCount / teamPlayers.Count;
            ConditionValue = (localPlayerDefeatsCount + 1) / teamDefeatedEnemiesAverage;
            localPlayerDefeatsCount = 0;
        }
        else
        {
            ConditionValue = 1f;
        }
    }

    public void Reset()
    {
        localPlayerDefeatsCount = 0;
        comparisonWithTeam = 1f;
    }
}
