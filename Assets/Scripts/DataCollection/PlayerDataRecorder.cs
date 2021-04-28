using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;


public class PlayerDataRecorder : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Save(string str);

    private List<FrameData> timeBasedData = new List<FrameData>();
    private List<FrameData> teamData = new List<FrameData>();
    private StreamWriter writer;
    private int counter = 0;
    [SerializeField] private int framesBetweenRecordTakes = 500; // How many frames between recording of DDA data
    [System.NonSerialized] public bool testEnded = false;       //Set to true when the player finishes the game
    private JsonDateTime sessionStartTime;
    
    void Start()
    {
        sessionStartTime = (JsonDateTime)System.DateTime.Now;

        //Initial data added
        AddTimeBasedData();
        AddTeamData();
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

                Time.fixedTime
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

                EnemySpawnDDAA.Instance.spawnMultiplier,
                HealingRateDDAA.Instance.healingMultiplier,

                Time.fixedTime
            )
        );
        Debug.Log("Added team data frame. Count: " + teamData.Count);
    }

    //Forms the JSON string to be send to the database
    public string GetJsonToSend()
    {
        DataContainer data = new DataContainer();
        data.playerID = GameObject.Find("ConditionSetter").GetComponent<PlayerIdentifier>().playerIdentifier;
        data.timeBasedData = timeBasedData.ToArray();
        data.teamData = teamData.ToArray();
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
}

//This data container is needed as we should send the data as a single object for MongoDB to accept it
class DataContainer{
    public string sessionStartTime;
    public FrameData[] timeBasedData;
    public FrameData[] teamData;
    public string condition;
    public string playerID;
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
