﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public sealed class DamageReceivedCondition: ICondition
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

    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    public static readonly float[] damageReceivedDiff = new float[] { 0.6f, 0.85f, 1.15f, 1.4f }; // how much more the player got damaged than other players
    private static readonly float[] dpgAdditiveValues = new float[] { 1.5f, 1f, 0f, -1f, -1.5f }; // additive values to DPG point

    public float localPlayerTotalDamageReceived = 0;

    private float comparisonWithTeam = 1f; // smaller value - better player

    public float ConditionValue
    {
        get => comparisonWithTeam;
        set
        {
            // should start the configuration of every DDAA here
            comparisonWithTeam = value;
            DDAEngine.AdjustDPG(value, damageReceivedDiff, dpgAdditiveValues);
        }
    }

    public void UpdateConditionalValue(List<GameObject> teamPlayers)
    {
        if (teamPlayers.Count > 0)
        {
            float totalTeamDamageReceived = 0; 
            teamPlayers.ForEach(player =>
            {
                totalTeamDamageReceived += player.GetComponent<PlayerManager>().totalDamageReceived - 100; //we subtract 100 not to devide by 0 and for the system to be smoother. This is mathematically valid
                player.GetComponent<PlayerManager>().totalDamageReceived = 0f;
            });
            float teamDamageReceivedAverage = totalTeamDamageReceived / teamPlayers.Count;
            ConditionValue = (localPlayerTotalDamageReceived - 100) / teamDamageReceivedAverage;
            localPlayerTotalDamageReceived = 0f;
            PlayerManager.LocalPlayerManager.totalDamageReceived = 0f;
        }
        else
        {
            ConditionValue = 1f;
        }
    }

    public void Reset()
    {
        localPlayerTotalDamageReceived = 0f;
        comparisonWithTeam = 1f;
    }
}
