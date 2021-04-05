using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviourPunCallbacks, IValueChangeListener
{
    public GameObject enemyPrefab;

    [SerializeField]
    private int maxEnemyCount = 2;

    [SerializeField]
    private int spawnIntervalForArea = 2;

    [SerializeField]
    private int spawnIntervalForProgress = 5;

    private bool isEnemySpawning = false;

    // enemy count meant to be spawned in progress locations
    private static readonly int[] enemyCountForProgressSpawnPoints = new int[] { 2, 4, 6 };

    // adds to the base enemy count when the level changes. First is always 0. 
    private static readonly int[] baseEnemyCountAddition = new int[] { 0, 5, 10 };

    private static readonly int initialEnemyAmountToSpawn = EnemySpawnDDAA.Instance.spawnAmount;
    private int enemiesLeftToSpawnForArea = initialEnemyAmountToSpawn;

    private int activeSpawnPointIndex;
    private readonly List<List<EnemySpawnPoint>> enemyAreaSpawnPoints = new List<List<EnemySpawnPoint>>();
    private readonly List<List<EnemySpawnPoint>> enemyProgressSpawnPoints = new List<List<EnemySpawnPoint>>(); // not sure about a correct name here

    private bool isInitialSpawnMade;

    private bool IsAreaCleared
    {
        get => enemiesLeftToSpawnForArea == 0;
    }
    private bool IsProgressCleared
    {
        get 
        {
            if (enemyCountForProgressSpawnPoints.Length > activeSpawnPointIndex)
                return enemyCountForProgressSpawnPoints[activeSpawnPointIndex] == 0;
            else return false;
        } 
    }
    private bool CanSpawnEnemy
    {
        get => !isEnemySpawning && GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount && (!IsProgressCleared || !IsAreaCleared);
    }
    public bool IsLevelFinished
    {
        get => IsProgressCleared && IsAreaCleared && GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log(enemiesLeftToSpawnForArea + " enemies to spawn");
            SetSpawnPoints(GameObject.FindGameObjectsWithTag("EnemySpawnPoint"));
            EnemySpawnDDAA.Instance.SetSpawnListener(this);
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !LevelProgressionCondition.Instance.isGameFinished)
        {
            LevelProgressionCondition.Instance.AddDeltaTime(Time.deltaTime);
            
            if (CanSpawnEnemy)
            {
                SpawnEnemy();
            }
            else if (IsLevelFinished)
            {
                photonView.RPC(nameof(LevelFinished), RpcTarget.All);
                
                try
                {
                    ChangeEnemyCount(baseEnemyCountAddition[LevelProgressionCondition.Instance.currentLevel]);
                }
                catch (IndexOutOfRangeException) { Debug.Log("Game is finished"); }

                activeSpawnPointIndex++;
                isInitialSpawnMade = false;
            }
        }
    }

    [PunRPC]
    private void LevelFinished()
    {
        LevelProgressionCondition.Instance.LevelFinished();
    }

    private void SpawnEnemy()
    {
        if (!LevelProgressionCondition.Instance.isGameFinished)
        {
            if (!isInitialSpawnMade)
            {
                StartCoroutine(InitialEnemySpawn());
                isInitialSpawnMade = true;
            }
            else
            {
                StartCoroutine(SingleEnemySpawn());
            }
        }
    }

    private IEnumerator InitialEnemySpawn()
    {
        isEnemySpawning = true;
        if (IsProgressCleared)
        {
            foreach (EnemySpawnPoint spawnPoint in enemyAreaSpawnPoints[activeSpawnPointIndex])
            {
                InstantiateEnemy(spawnPoint, true);
                enemiesLeftToSpawnForArea--;
            }
            yield return new WaitForSeconds(spawnIntervalForArea);
        }
        else
        {
            foreach (EnemySpawnPoint spawnPoint in enemyProgressSpawnPoints[activeSpawnPointIndex])
            {
                InstantiateEnemy(spawnPoint, false);
                enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
            }
            yield return new WaitForSeconds(spawnIntervalForProgress);
        }
        isEnemySpawning = false;
    }

    private IEnumerator SingleEnemySpawn()
    {
        isEnemySpawning = true;
        if (IsProgressCleared)
        {
            List<EnemySpawnPoint> validSpawnPoints = enemyAreaSpawnPoints[activeSpawnPointIndex].ToValidSpawnPoints();
            if (validSpawnPoints.Count > 0)
            {
                EnemySpawnPoint spawnPoint = validSpawnPoints[UnityEngine.Random.Range(0, validSpawnPoints.Count)];
                InstantiateEnemy(spawnPoint, true);
                enemiesLeftToSpawnForArea--;
                yield return new WaitForSeconds(spawnIntervalForArea);
            }
        }
        else
        {
            // for now we just take the last progress point for spawning in process (should probably be changed later, but it fits the current design)
            EnemySpawnPoint spawnPoint = enemyProgressSpawnPoints[activeSpawnPointIndex].FindLast(delegate (EnemySpawnPoint point) { return true; });
            if (!spawnPoint.IsEnemyOrPlayerTooClose())
            {
                InstantiateEnemy(spawnPoint, false);
                enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                isInitialSpawnMade = !IsProgressCleared;
                yield return new WaitForSeconds(spawnIntervalForProgress);
            }
        }
        isEnemySpawning = false;
    }

    private void InstantiateEnemy(EnemySpawnPoint spawnPoint, bool isAreaEnemy)
    {
        object[] instantiationData = { isAreaEnemy };   //we need to pass instantiation data like this for the client players to receive this data
        PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.transform.position, Quaternion.identity, 0, instantiationData);
    }

    private void ChangeEnemyCount(int addToEnemyCount)
    {
        if (DDAEngine.isDynamicAdjustmentEnabled)
        {
            EnemySpawnDDAA.Instance.AdjustInGameValue(addToEnemyCount);
        }
        else
        {
            OnValueChanged(initialEnemyAmountToSpawn + addToEnemyCount);
        }
    }

    public void OnValueChanged(float value)
    {
        //Debug.Log("Active point: " + activeSpawnPointIndex + ". " + value + " enemies to spawn");
        enemiesLeftToSpawnForArea = (int) value;
    }

    private void SetSpawnPoints(GameObject[] allSpawnPointObjects)
    {
        foreach (GameObject spawnPointObject in allSpawnPointObjects)
        {
            EnemySpawnPoint spawnPoint = spawnPointObject.GetComponent<EnemySpawnPoint>();

            if (spawnPoint.isAreaBased)
            {
                enemyAreaSpawnPoints.AddToEnemySpawnPointList(spawnPoint);
            } 
            else
            {
                enemyProgressSpawnPoints.AddToEnemySpawnPointList(spawnPoint);
            }
        }
    }
}
