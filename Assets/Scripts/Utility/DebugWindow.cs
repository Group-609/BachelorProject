using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;

public class DebugWindow : MonoBehaviour
{
    Text debugText;
    float debugStartDelay = 1f;     //How long we wait until debugging starts
    bool isDebugInfoCollected = false;
    private bool isDDAInfoShown = true;
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

    private void OnEnable()
    {
        if (speedHack != null)
            speedHack.enabled = true;
    }

    private void OnDisable()
    {
        if (speedHack != null)
            speedHack.enabled = false;
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

            if (Input.GetKeyDown("u"))
            {
                isDDAInfoShown = !isDDAInfoShown;
            }
        }
        if(debugText.enabled && isDebugInfoCollected)
        {
            if (isDDAInfoShown)
                debugText.text = GetDebugWindowText(GetDebugTextForDDA());
            else
                debugText.text = GetDebugWindowText(GetDebugInfo());
        }
            
    }

    IEnumerator GetInfoSources()
    {
        yield return new WaitForSeconds(debugStartDelay);
        List<GameObject> playerObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach(GameObject playerObject in playerObjects)
        {
            players.Add(playerObject.GetComponent<PlayerManager>());
        }
        isDebugInfoCollected = true;
        speedHack.enabled = true;
    }

    List<string> GetDebugInfo()
    {
        List<string> debugStrings = new List<string>();
        debugStrings.Add("Debug info:");
        foreach(PlayerManager player in players)
        {
            debugStrings.Add(player.GetPlayerDebugInfo());
        }
        
        debugStrings.Add("----------------");
        debugStrings.Add("Current level: " + LevelProgressionCondition.Instance.currentLevel);
        debugStrings.Add("Enemies left: " + GameObject.FindGameObjectsWithTag("Enemy").Length);

        debugStrings.AddRange(GetGeneralDebugInfo());

        return debugStrings;
    }

    private List<string> GetDebugTextForDDA()
    {
        List<string> debugStrings = new List<string>();
        if (DDAEngine.isDynamicAdjustmentEnabled)
        {
            debugStrings.Add("DDA condition is active. INFO:");
            debugStrings.Add("----------Conditions-----------");
            debugStrings.Add("Level progression (Smaller value - better team): " + LevelProgressionCondition.Instance.ConditionValue);
            debugStrings.Add("Defeated enemies comparison (BV-BP): " + DefeatedEnemiesCountCondition.Instance.ConditionValue);
            debugStrings.Add("Stun comparison (SV-BP): " + StunCondition.Instance.ConditionValue);
            debugStrings.Add("Damage received comparison (SV-BP): " + DamageReceivedCondition.Instance.ConditionValue);
        }
        else
        {
            debugStrings.Add("Control condition is active. INFO");
            debugStrings.Add("----------Conditions-----------");
            debugStrings.Add("Level progression (Smaller value - better team): " + LevelProgressionCondition.Instance.ConditionValue);
        }

        debugStrings.Add("\n");
        debugStrings.Add("---------------Variables----------------");
        debugStrings.Add("Enemy spawn amount for area (TEAM): " + EnemySpawnDDAA.Instance.spawnAmount);
        debugStrings.Add("Player healing rate (TEAM): " + HealingRateDDAA.Instance.healingRate);
        debugStrings.Add("Player paintball damage: " + PlayerPainballDamageDDAA.Instance.paintballDamage);
        debugStrings.Add("Enemy melee damage: " + EnemyMeleeDamageDDAA.Instance.meleeDamage);
        debugStrings.Add("Enemy bullet damage: " + EnemyBulletDamageDDAA.Instance.bulletDamage);

        debugStrings.Add("\n");
        debugStrings.Add("----------Current values in game-----------");
        debugStrings.Add("Local player defeated enemies: " + DefeatedEnemiesCountCondition.Instance.localPlayerDefeatsCount);
        debugStrings.Add("Local player stun count: " + StunCondition.Instance.localPlayerStuntCount);
        debugStrings.Add("Local player damage received: " + DamageReceivedCondition.Instance.localPlayerTotalDamageReceived);
        

        debugStrings.AddRange(GetGeneralDebugInfo());

        return debugStrings;
    }

    private List<string> GetGeneralDebugInfo()
    {
        List<string> debugStrings = new List<string>();

        debugStrings.Add("\n");
        debugStrings.Add("--------------------------------------");
        debugStrings.Add("\n");

        if (speedHackEnabled)
            debugStrings.Add("Speedhack enabled - press 'o' again to disable");
        else debugStrings.Add("Press 'o' for speedhack");

        if (isDDAInfoShown)
            debugStrings.Add("Press 'u' to show general debug info");
        else debugStrings.Add("Press 'u' to show DDA info");

        debugStrings.Add("Press 'i' to blobify all enemies");
        return debugStrings;
    }

    string GetDebugWindowText(List<string> debugStrings)
    {
        string text = "";
        foreach (string debugString in debugStrings)
        {
            text = text + "\n" + debugString;
        }
        return text;
    }
}
