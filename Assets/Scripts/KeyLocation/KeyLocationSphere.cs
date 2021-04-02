using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class KeyLocationSphere : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        PaintBall paintBall = other.gameObject.GetComponent<PaintBall>();
        if (paintBall != null && !paintBall.playerWhoShot.GetComponent<FirstPersonController>().isPlayerInKeyLocZone)
        {
            Destroy(other.gameObject);
        }
    }
}
