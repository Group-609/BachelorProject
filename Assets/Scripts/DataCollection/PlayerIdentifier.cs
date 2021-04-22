using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;


public class PlayerIdentifier : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void RequestPlayerIdentifier();

    public string playerIdentifier;

    void GetPlayerIdentifier(string data)
    {
        playerIdentifier = data;
    }
}
