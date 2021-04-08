using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;

public class DebugWindow : MonoBehaviour
{
    Text debugText;
    float debugStartDelay = 1f;     //How long we wait until debugging starts
    bool debugInfoCollected = false;
    List<PlayerManager> players = new List<PlayerManager>();

    SpeedHack speedHack;
    bool speedHackEnabled = false;
    private float originalSpeed;



    // Start is called before the first frame update
    void Start()
    {
        debugText = GetComponent<Text>();
        debugText.enabled = false;
        speedHack = GetComponent<SpeedHack>();
        StartCoroutine(GetInfoSources());    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("q"))
        {
            if (Input.GetKeyDown("p"))
            {
                debugText.enabled = !debugText.enabled;
            }
        }
        if(debugText.enabled)
        {
            if (Input.GetKeyDown("i"))
            {
                List<GameObject> enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
                foreach (GameObject enemy in enemies)
                {
                    enemy.GetComponent<EnemyController>().Die();
                }
            }
            

            if (Input.GetKeyDown("o"))
            {
                speedHackEnabled = !speedHackEnabled;
                speedHack.ToggleSpeedHack(speedHackEnabled);
            }
        }
        if(debugText.enabled && debugInfoCollected)
            debugText.text = GetDebugWindowText(GetDebugInfo());
    }

    IEnumerator GetInfoSources()
    {
        yield return new WaitForSeconds(debugStartDelay);
        List<GameObject> playerObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach(GameObject playerObject in playerObjects)
        {
            players.Add(playerObject.GetComponent<PlayerManager>());
        }
        debugInfoCollected = true;
        speedHack.enabled = true;
    }

    List<string> GetDebugInfo()
    {
        List<string> debugStrings = new List<string>();
        foreach(PlayerManager player in players)
        {
            debugStrings.Add(player.GetPlayerDebugInfo());
        }
        if (DDAEngine.isDynamicAdjustmentEnabled)
        {
            debugStrings.Add("DDA(test) condition is active");
            debugStrings.Add("Kill condition: " + KillCountCondition.Instance.ConditionValue);
            debugStrings.Add("Level progression condition: " + LevelProgressionCondition.Instance.ConditionValue);
            debugStrings.Add("Stun condition: " + StunCondition.Instance.ConditionValue);
        }
        else debugStrings.Add("Control condition is active");
        debugStrings.Add("----------------");
        debugStrings.Add("Current level: " + LevelProgressionCondition.Instance.currentLevel);
        debugStrings.Add("Enemies left: " + GameObject.FindGameObjectsWithTag("Enemy").Length);
        if (speedHackEnabled)
            debugStrings.Add("Speedhack enabled - press 'o' again to disable");
        else debugStrings.Add("Press 'o' for speedhack");
        
        debugStrings.Add("Press 'i' to blobify all enemies");

        return debugStrings;
    }

    string GetDebugWindowText(List<string> debugStrings)
    {
        string text = "Debug info:";
        foreach (string debugString in debugStrings)
        {
            text = text + "\n" + debugString;
        }
        return text;
    }
}
