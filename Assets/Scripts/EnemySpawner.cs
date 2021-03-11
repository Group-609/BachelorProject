using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPunCallbacks, IValueChangeListener
{
    public GameObject enemyPrefab;

    [SerializeField]
    private int maxEnemyCount = 2;

    [SerializeField]
    private int spawnInterval = 2;

    private bool coroutineRunning = false;
    private Transform enemySpawn;

    private int enemiesLeftToSpawn = EnemySpawnDDAA.Instance.spawnAmount;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(enemiesLeftToSpawn + " enemies to spawn");
        enemySpawn = transform.Find("EnemySpawnPoint");
        EnemySpawnDDAA.Instance.SetSpawnListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !LevelProgressionCondition.Instance.isGameFinished)
        {
            LevelProgressionCondition.Instance.AddDeltaTime(Time.deltaTime);
            if (!coroutineRunning && enemiesLeftToSpawn != 0)
            {
                StartCoroutine(SpawnEnemy());
            }
            else if (enemiesLeftToSpawn == 0 && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                LevelProgressionCondition.Instance.LevelFinished();
                EnemySpawnDDAA.Instance.AdjustInGameValue();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(spawnInterval);
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount)
        {
            PhotonNetwork.Instantiate(enemyPrefab.name, enemySpawn.transform.position, Quaternion.identity);
            enemiesLeftToSpawn--;
        }
        coroutineRunning = false;
    }

    public void OnValueChanged(float value)
    {
        // change spawn location here, when level is created
        Debug.Log(value + " enemies to spawn");
        enemiesLeftToSpawn = (int) value;
    }
}
