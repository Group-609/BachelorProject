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

    private bool isEnemySpawningInProgress = false;

    // enemy count meant to be spawned in progress locations
    private static readonly int[] enemyCountForProgressSpawnPoints = new int[] { 2, 4, 6 };

    // adds to the base enemy count when the level changes. First is always 0. 
    private static readonly int[] baseEnemyCountAddition = new int[] { 0, 5, 10 };

    private static readonly int initialEnemyAmountToSpawn = EnemySpawnDDAA.Instance.spawnAmount;
    private int enemiesLeftToSpawnForArea = initialEnemyAmountToSpawn;

    private int activeSpawnPointIndex;
    private readonly List<List<EnemySpawnPoint>> enemyAreaSpawnPoints = new List<List<EnemySpawnPoint>>();
    private readonly List<List<EnemySpawnPoint>> enemyProgressSpawnPoints = new List<List<EnemySpawnPoint>>(); // not sure about a correct name here

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(enemiesLeftToSpawnForArea + " enemies to spawn");
        SetSpawnPoints(GameObject.FindGameObjectsWithTag("EnemySpawnPoint"));
        EnemySpawnDDAA.Instance.SetSpawnListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !LevelProgressionCondition.Instance.isGameFinished)
        {
            LevelProgressionCondition.Instance.AddDeltaTime(Time.deltaTime);
            if (!isEnemySpawningInProgress && enemiesLeftToSpawnForArea > 0)
            {
                if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                {
                    StartCoroutine(InitialEnemySpawn());
                }
                else
                {
                    StartCoroutine(SpawnEnemy());
                }
            }
            else if (enemiesLeftToSpawnForArea == 0 && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                LevelProgressionCondition.Instance.LevelFinished();

                try
                {
                    ChangeEnemyCount(baseEnemyCountAddition[LevelProgressionCondition.Instance.currentLevel]);
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.Log("Game is finished");
                }
                activeSpawnPointIndex++;
            }
        }
    }

    IEnumerator InitialEnemySpawn()
    {
        isEnemySpawningInProgress = true;
        if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
        {
            foreach (EnemySpawnPoint spawnPoint in enemyProgressSpawnPoints[activeSpawnPointIndex])
            {
                PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.transform.position, Quaternion.identity);
                enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
            }
            yield return new WaitForSeconds(spawnIntervalForProgress);
        }
        else
        {
            foreach (EnemySpawnPoint spawnPoint in enemyAreaSpawnPoints[activeSpawnPointIndex])
            {
                PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.transform.position, Quaternion.identity);
                enemiesLeftToSpawnForArea--;
            }
            yield return new WaitForSeconds(spawnIntervalForArea);
        }
        isEnemySpawningInProgress = false;
    }

    IEnumerator SpawnEnemy()
    {
        isEnemySpawningInProgress = true;
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount)
        {
            if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
            { 
                // for now we just take the last progress point for spawning in process (should probably be changed later, but fits the current design)
                EnemySpawnPoint spawnPoint = enemyProgressSpawnPoints[activeSpawnPointIndex].FindLast(delegate(EnemySpawnPoint point) { return true; });
                PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.transform.position, Quaternion.identity);
                enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                yield return new WaitForSeconds(spawnIntervalForProgress);
            }
            else
            {
                int locationIndex = UnityEngine.Random.Range(0, enemyAreaSpawnPoints[activeSpawnPointIndex].Count - 1);
                Debug.Log("Enemy " + enemiesLeftToSpawnForArea + ". Location index: " + locationIndex);
                PhotonNetwork.Instantiate(enemyPrefab.name, enemyAreaSpawnPoints[activeSpawnPointIndex][locationIndex].transform.position, Quaternion.identity);
                enemiesLeftToSpawnForArea--;
                yield return new WaitForSeconds(spawnIntervalForArea);
            }
        }
        isEnemySpawningInProgress = false;
    }
    private void ChangeEnemyCount(int addToEnemyCount)
    {
        if (DDAEngine.Instance.isDynamicAdjustmentEnabled)
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
        Debug.Log("Active point: " + activeSpawnPointIndex + ". " + value + " enemies to spawn");
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
