using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public sealed class DamageReceivedCondition
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static DamageReceivedCondition instance = null;
    private static readonly object padlock = new object();

    private DamageReceivedCondition() { }

    public static DamageReceivedCondition Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new DamageReceivedCondition();
                }
                return instance;
            }
        }
    }

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    private static readonly float[] dpgValues = new float[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f }; // stun count difference, by which the DDAA's additive values would be mapped
    private static readonly float[] dpgAdditiveValues = new float[] { 1.5f, 1f, 0f, -1f, -1.5f }; // additive values to DPG point

    public float localPlayerTotalDamageReceived = 0;

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
            float totalTeamDamageReceived = 0;
            teamPlayers.ForEach(player => totalTeamDamageReceived += player.GetComponent<PlayerManager>().totalDamageReceived);
            float teamDamageReceivedAverage = totalTeamDamageReceived / teamPlayers.Count;
            ConditionValue = localPlayerTotalDamageReceived / teamDamageReceivedAverage;
        }
        else
        {
            ConditionValue = 1f;
        }
    }
}
