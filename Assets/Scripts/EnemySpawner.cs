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
    private int spawnInterval = 3;

    private bool coroutineRunning = false;
    private Transform enemySpawn;

    private int enemiesLeftToSpawn = EnemySpawnDDAA.Instance.spawnAmount;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawn = transform.Find("EnemySpawnPoint");
    }

    // Update is called once per frame
    void Update()
    {
        LevelProgressionCondition.Instance.AddDeltaTime(Time.deltaTime);

        if (PhotonNetwork.IsMasterClient)
        {
            if (!coroutineRunning)
            {
                StartCoroutine(CheckEnemyCount());
            }
        }
    }

    IEnumerator CheckEnemyCount()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(spawnInterval);
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount && enemiesLeftToSpawn > 0)
        {
            PhotonNetwork.Instantiate(enemyPrefab.name, enemySpawn.transform.position, Quaternion.identity);
            Debug.Log("Spawning enemy");
            enemiesLeftToSpawn--;
        } 
        else if (enemiesLeftToSpawn == 0)
        {
            LevelProgressionCondition.Instance.LevelFinished();
            EnemySpawnDDAA.Instance.AdjustInGameValue();
        }
        coroutineRunning = false;
    }

    public void OnValueChanged(float value)
    {
        // change spawn location here
        enemiesLeftToSpawn = (int) value;
    }
}
