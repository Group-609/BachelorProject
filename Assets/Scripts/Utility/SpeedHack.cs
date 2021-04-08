using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class SpeedHack : MonoBehaviour
{
    public bool isEnabled = false;
    FirstPersonController firstPersonController;
    private float hackedSpeed = 20f;
    private float originalSpeed;

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> playerObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach(GameObject playerObject in playerObjects)
        {
            if(playerObject.GetComponent<FirstPersonController>().isActiveAndEnabled)
            {
                firstPersonController = playerObject.GetComponent<FirstPersonController>();
            }
        }
        if(firstPersonController == null)
        {
            Debug.Log("firstPersonController was not found by speedhack");
        }
        else
        {
            originalSpeed = firstPersonController.GetRunSpeed();
        }
    }

    public void ToggleSpeedHack(bool enable)
    {
        if(enable)
            firstPersonController.SetRunSpeed(hackedSpeed);
        else firstPersonController.SetRunSpeed(originalSpeed);
    }
}
