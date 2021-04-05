using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class KeyLocationSphere : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        PaintBall paintBall = other.gameObject.GetComponent<PaintBall>();
        if (paintBall != null && !paintBall.playerWhoShot.GetComponent<PlayerManager>().isPlayerInKeyLocZone)
        {
            Destroy(other.gameObject);
        }
    }
}
