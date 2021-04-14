using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public sealed class StunCondition
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static StunCondition instance = null;
    private static readonly object padlock = new object();

    private StunCondition() { }

    public static StunCondition Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new StunCondition();
                }
                return instance;
            }
        }
    }

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    private static readonly float[] dpgValues = new float[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f }; // stun count difference, by which the DDAA's additive values would be mapped
    private static readonly float[] dpgAdditiveValues = new float[] { 1.5f, 1f, 0f,-1f, -1.5f }; // additive values to DPG point

    public int localPlayerStuntCount = 0;

    private float comparisonWithTeam; // smaller value - better player

    public float ConditionValue
    {
        get => comparisonWithTeam;
        set
        {
            // should start the configuration of every DDAA here
            comparisonWithTeam = value;
            DDAEngine.AdjustDPG(value, dpgValues, dpgAdditiveValues);
        }
    }

    public void UpdateConditionalValue(List<GameObject> teamPlayers)
    {
        if (teamPlayers.Count > 0)
        {
            int totalTeamStunCount = 0;
            teamPlayers.ForEach(player => totalTeamStunCount += player.GetComponent<PlayerManager>().stunCount);
            float teamStunCountAverage = totalTeamStunCount / teamPlayers.Count;
            ConditionValue = localPlayerStuntCount / teamStunCountAverage;
        }
        else
        {
            ConditionValue = 1f;
        }
    }
}
