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
    /*
    private readonly List<List<EnemySpawnPoint>> enemyAreaSpawnPoints = new List<List<EnemySpawnPoint>>();
    private readonly List<List<EnemySpawnPoint>> enemyProgressSpawnPoints = new List<List<EnemySpawnPoint>>(); // not sure about a correct name here
    */

    private readonly List<EnemySpawnPoint> area0SpawnPoints = new List<EnemySpawnPoint>();
    private readonly List<EnemySpawnPoint> area1SpawnPoints = new List<EnemySpawnPoint>();
    private readonly List<EnemySpawnPoint> area2SpawnPoints = new List<EnemySpawnPoint>();
    private readonly List<EnemySpawnPoint> progress0SpawnPoints = new List<EnemySpawnPoint>();
    private readonly List<EnemySpawnPoint> progress1SpawnPoints = new List<EnemySpawnPoint>();
    private readonly List<EnemySpawnPoint> progress2SpawnPoints = new List<EnemySpawnPoint>();



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
                catch { Debug.Log("Game is finished"); }

                activeSpawnPointIndex++;
                isInitialSpawnMade = false;
            }
        }
    }

    [PunRPC]
    private void LevelFinished()
    {
        //Debug.LogError("Level Finished called in EnemySpawner");
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
            switch (activeSpawnPointIndex)
            {
                case 0:
                    foreach(EnemySpawnPoint spawnPoint in area0SpawnPoints)
                    {
                        if (enemiesLeftToSpawnForArea > 0)
                        {
                            InstantiateEnemy(spawnPoint, true);
                            enemiesLeftToSpawnForArea--;
                        }
                    }
                    break;
                case 1:
                    foreach (EnemySpawnPoint spawnPoint in area1SpawnPoints)
                    {
                        if (enemiesLeftToSpawnForArea > 0)
                        {
                            InstantiateEnemy(spawnPoint, true);
                            enemiesLeftToSpawnForArea--;
                        }
                    }
                    break;
                case 2:
                    foreach (EnemySpawnPoint spawnPoint in area2SpawnPoints)
                    {
                        if (enemiesLeftToSpawnForArea > 0)
                        {
                            InstantiateEnemy(spawnPoint, true);
                            enemiesLeftToSpawnForArea--;
                        }
                    }
                    break;
            }
            yield return new WaitForSeconds(spawnIntervalForArea);
        }
        else
        {
            switch (activeSpawnPointIndex)
            {
                case 0:
                    foreach (EnemySpawnPoint spawnPoint in progress0SpawnPoints)
                    {
                        if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
                        {
                            InstantiateEnemy(spawnPoint, false);
                            enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                        }
                    }
                    break;
                case 1:
                    foreach (EnemySpawnPoint spawnPoint in progress1SpawnPoints)
                    {
                        if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
                        {
                            InstantiateEnemy(spawnPoint, false);
                            enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                        }
                    }
                    break;
                case 2:
                    foreach (EnemySpawnPoint spawnPoint in progress2SpawnPoints)
                    {
                        if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
                        {
                            InstantiateEnemy(spawnPoint, false);
                            enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                        }
                    }
                    break;
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
            List<EnemySpawnPoint> validSpawnPoints = new List<EnemySpawnPoint>();
            switch (activeSpawnPointIndex)
            {
                case 0:
                    validSpawnPoints = area0SpawnPoints.ToValidSpawnPoints();
                    break;
                case 1:
                    validSpawnPoints = area1SpawnPoints.ToValidSpawnPoints();
                    break;
                case 2:
                    validSpawnPoints = area2SpawnPoints.ToValidSpawnPoints();
                    break;
            }
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
            EnemySpawnPoint spawnPoint = null;
            switch (activeSpawnPointIndex)
            {
                case 0:
                    spawnPoint = progress0SpawnPoints.FindLast(delegate (EnemySpawnPoint point) { return true; });
                    break;
                case 1:
                    spawnPoint = progress1SpawnPoints.FindLast(delegate (EnemySpawnPoint point) { return true; });
                    break;
                case 2:
                    spawnPoint = progress2SpawnPoints.FindLast(delegate (EnemySpawnPoint point) { return true; });
                    break;
            }
            
            if (spawnPoint != null && !spawnPoint.IsEnemyOrPlayerTooClose())
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
            //Debug.Log("Raw spawn point index: " + spawnPoint.areaIndex + ". Is area: " + spawnPoint.isAreaBased);

            if (spawnPoint.isAreaBased)
            {
                switch (spawnPoint.areaIndex)
                {
                    case 0:
                        area0SpawnPoints.Add(spawnPoint);
                        break;
                    case 1:
                        area1SpawnPoints.Add(spawnPoint);
                        break;
                    case 2:
                        area2SpawnPoints.Add(spawnPoint);
                        break;
                }
            } 
            else
            {

                switch (spawnPoint.areaIndex)
                {
                    case 0:
                        progress0SpawnPoints.Add(spawnPoint);
                        break;
                    case 1:
                        progress1SpawnPoints.Add(spawnPoint);
                        break;
                    case 2:
                        progress2SpawnPoints.Add(spawnPoint);
                        break;
                }
            }
        }

        /*Debug.Log("area0SpawnPoints: " + area0SpawnPoints.Count + " spawn points");
        Debug.Log("area1SpawnPoints: " + area1SpawnPoints.Count + " spawn points");
        Debug.Log("area2SpawnPoints: " + area2SpawnPoints.Count + " spawn points");
        Debug.Log("progress0SpawnPoints: " + progress0SpawnPoints.Count + " spawn points");
        Debug.Log("progress1SpawnPoints: " + progress1SpawnPoints.Count + " spawn points");
        Debug.Log("progress2SpawnPoints: " + progress2SpawnPoints.Count + " spawn points");*/
    }
}
