using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;


public class PlayerIdentifier : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void RequestPlayerIdentifier();

    public string playerIdentifier = "";

    void GetPlayerIdentifier(string data)
    {
        playerIdentifier = data;
    }

    public void SetPlayerIdentifiers()
    {
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerManager>().IsPlayerLocal())
            {
                player.GetComponent<PlayerManager>().CallGetPlayerIdentifier(playerIdentifier);            //we send other players our player identifier
                Debug.Log("Identified player: " + playerIdentifier);
            }

        }
    }
}
