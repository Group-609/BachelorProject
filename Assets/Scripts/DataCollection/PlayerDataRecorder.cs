using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;


public class PlayerDataRecorder : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Save(string str);

    private List<FrameData> timeBasedData = new List<FrameData>();
    private List<FrameData> teamData = new List<FrameData>();
    private List<ValidationData> validationData = new List<ValidationData>();
    [SerializeField] private int framesBetweenRecordTakes = 500; // How many frames between recording of DDA data
    [System.NonSerialized] public bool testEnded = false;       //Set to true when the player finishes the game
    private JsonDateTime sessionStartTime;
    private float conditionStartTime;
    private bool isMasterClient;
    
    void Start()
    {
        if(!GetComponent<PlayerManager>().IsPlayerLocal())
        {
            this.enabled = false;
        }
        sessionStartTime = (JsonDateTime)System.DateTime.Now;
        conditionStartTime = Time.fixedTime;
        //Initial data added
        AddTimeBasedData();
        AddTeamData();
        AddValidationData();
    }

    public void ResetForCondition()
    {
        timeBasedData = new List<FrameData>();
        teamData = new List<FrameData>();
        validationData = new List<ValidationData>();
        testEnded = false;
        conditionStartTime = Time.fixedTime;
        AddTimeBasedData();
        AddTeamData();
        AddValidationData();
    }

    public void AddValidationData()
    {
        validationData.Add(
            new ValidationData(
                GetComponent<PlayerManager>().completeDamageReceived,
                GetComponent<PlayerManager>().completeDamageDone,
                Time.fixedTime - conditionStartTime
            )
        );
    }

    public void AddTimeBasedData()
    {
        timeBasedData.Add(
            new FrameData(
                DDAEngine.difficultiesPointGlobal,

                DefeatedEnemiesCountCondition.Instance.ConditionValue,
                StunCondition.Instance.ConditionValue,
                DamageReceivedCondition.Instance.ConditionValue,

                PlayerPainballDamageDDAA.Instance.paintballDamage,
                EnemyMeleeDamageDDAA.Instance.meleeDamage,
                EnemyBulletDamageDDAA.Instance.bulletDamage,

                PlayerPainballDamageDDAA.Instance.painballDamageMultiplier,
                EnemyMeleeDamageDDAA.Instance.meleeDamageMultiplier,
                EnemyBulletDamageDDAA.Instance.bulletDamageMultiplier,

                DefeatedEnemiesCountCondition.Instance.localPlayerDefeatsCount,
                StunCondition.Instance.localPlayerStuntCount,
                DamageReceivedCondition.Instance.localPlayerTotalDamageReceived,

                Time.fixedTime - conditionStartTime
            )
        );
        Debug.Log("Added time based data frame. Count: " + timeBasedData.Count);
    }

    public void AddTeamData()
    {
        teamData.Add(
            new FrameData(
                DDAEngine.difficultiesPointGlobal,

                LevelProgressionCondition.Instance.ConditionValue,

                EnemySpawnDDAA.Instance.spawnAmount,
                HealingRateDDAA.Instance.healingRate,

                EnemySpawnDDAA.Instance.spawnFloatingPoint,

                EnemySpawnDDAA.Instance.spawnMultiplier,
                HealingRateDDAA.Instance.healingMultiplier,

                Time.fixedTime - conditionStartTime
            )
        );
        Debug.Log("Added team data frame. Count: " + teamData.Count);
    }

    //Forms the JSON string to be send to the database
    public string GetJsonToSend()
    {
        DataContainer data = new DataContainer();
        data.playerIDs = CollectPlayerIDs();
        data.isMaster = PhotonNetwork.IsMasterClient;
        data.timeBasedData = timeBasedData.ToArray();
        data.teamData = teamData.ToArray();
        data.validationData = validationData.ToArray();
        data.sessionStartTime = sessionStartTime.dateTime.ToString();
        if (DDAEngine.isDynamicAdjustmentEnabled)
        {
            data.condition = "DDA";
        }
        else
        {
            data.condition = "Control";
        }
        return JsonUtility.ToJson(data);
    }
    private string[] CollectPlayerIDs()
    {
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        List<string> playerIdentifiers = new List<string>();
        foreach(GameObject player in players)
        {
            playerIdentifiers.Add(player.GetComponent<PlayerManager>().playerIdentifier);
        }
        return playerIdentifiers.ToArray();
    }
}

//This data container is needed as we should send the data as a single object for MongoDB to accept it
class DataContainer{
    public string sessionStartTime;
    public string condition;
    public bool isMaster;
    public string[] playerIDs;
    public FrameData[] timeBasedData;
    public FrameData[] teamData;
    public ValidationData[] validationData;
}

struct JsonDateTime
{
    public long dateTime;

    public static implicit operator JsonDateTime(System.DateTime dt)
    {
        JsonDateTime jdt = new JsonDateTime();
        jdt.dateTime = dt.ToFileTimeUtc();
        return jdt;
    }
}
