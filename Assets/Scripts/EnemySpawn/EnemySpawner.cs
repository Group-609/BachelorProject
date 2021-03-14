namespace EnemySpawner
{
    using EnemySpawnExtensions;
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
        private int spawnInterval = 2;

        private bool isEnemySpawningInProgress = false;

        // enemy count meant to be spawned in progress locations
        private static readonly int[] enemyCountForProgressSpawnPoints = new int[] { 2, 4, 6 };

        // adds to the base enemy count when the level changes. First is always 0. 
        private static readonly int[] baseEnemyCountAddition = new int[] { 0, 5, 10 };

        private static readonly int initialEnemyAmountToSpawn = EnemySpawnDDAA.Instance.spawnAmount;
        private int enemiesLeftToSpawn = initialEnemyAmountToSpawn;

        private int activeSpawnPointIndex;
        private List<List<EnemySpawnPoint>> enemyAreaSpawnPoints = new List<List<EnemySpawnPoint>>();
        private List<List<EnemySpawnPoint>> enemyProgressSpawnPoints = new List<List<EnemySpawnPoint>>(); // not sure about a correct name here

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(enemiesLeftToSpawn + " enemies to spawn");
            SetSpawnPoints(GameObject.FindGameObjectsWithTag("EnemySpawnPoint"));
            EnemySpawnDDAA.Instance.SetSpawnListener(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (PhotonNetwork.IsMasterClient && !LevelProgressionCondition.Instance.isGameFinished)
            {
                LevelProgressionCondition.Instance.AddDeltaTime(Time.deltaTime);
                if (!isEnemySpawningInProgress && enemiesLeftToSpawn != 0)
                {
                    StartCoroutine(SpawnEnemy());
                }
                else if (enemiesLeftToSpawn == 0 && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
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

        IEnumerator SpawnEnemy()
        {
            isEnemySpawningInProgress = true;
            yield return new WaitForSeconds(spawnInterval);
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount)
            {
                if (enemyCountForProgressSpawnPoints[activeSpawnPointIndex] > 0)
                {
                    int locationIndex = new System.Random().Next(0, enemyProgressSpawnPoints[activeSpawnPointIndex].Count - 1);
                    Debug.Log("Enemy " + enemyCountForProgressSpawnPoints[activeSpawnPointIndex] + ". Location index: " + locationIndex);
                    PhotonNetwork.Instantiate(enemyPrefab.name, enemyProgressSpawnPoints[activeSpawnPointIndex][locationIndex].transform.position, Quaternion.identity);
                    enemyCountForProgressSpawnPoints[activeSpawnPointIndex]--;
                }
                else
                {
                    int locationIndex = new System.Random().Next(0, enemyAreaSpawnPoints[activeSpawnPointIndex].Count - 1);
                    Debug.Log("Enemy " + enemiesLeftToSpawn + ". Location index: " + locationIndex);
                    PhotonNetwork.Instantiate(enemyPrefab.name, enemyAreaSpawnPoints[activeSpawnPointIndex][locationIndex].transform.position, Quaternion.identity);
                    enemiesLeftToSpawn--;
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
            // change spawn location here, when level is created
            Debug.Log(value + " enemies to spawn");
            enemiesLeftToSpawn = (int) value;
        }

        private void SetSpawnPoints(GameObject[] allSpawnPointObjects)
        {
            foreach(GameObject spawnPointObject in allSpawnPointObjects)
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

}
namespace EnemySpawnExtensions
{
    using System.Collections.Generic;
    public static class EnemySpawnListExtensions
    {
        public static void AddToEnemySpawnPointList(this List<List<EnemySpawnPoint>> currentSpawnPointList, EnemySpawnPoint spawnPointToAdd)
        {
            if (currentSpawnPointList[spawnPointToAdd.areaIndex] == null)
            {
                currentSpawnPointList.Insert(spawnPointToAdd.areaIndex, new List<EnemySpawnPoint>() { spawnPointToAdd });
            }
            else
            {
                currentSpawnPointList[spawnPointToAdd.areaIndex].Add(spawnPointToAdd);
            }
        }
    }
}
