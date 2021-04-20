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
        firstPersonController = GetFirstPersonController();
        if (firstPersonController == null)
            Debug.Log("firstPersonController was not found by speedhack at start");
        else originalSpeed = firstPersonController.GetRunSpeed();
    }

    private void OnEnable()
    {
        firstPersonController = GetFirstPersonController();
        if (firstPersonController == null)
            Debug.Log("firstPersonController was not found by speedhack on enabled");
    }

    private FirstPersonController GetFirstPersonController()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Player")).Find(
                delegate(GameObject playerObject)
                {
                    return playerObject.GetComponent<FirstPersonController>().isActiveAndEnabled;
                }
            ).GetComponent<FirstPersonController>();
    }

    public void ToggleSpeedHack(bool enable)
    {
        if(enable)
            firstPersonController.SetRunSpeed(hackedSpeed);
        else firstPersonController.SetRunSpeed(originalSpeed);
    }
}
