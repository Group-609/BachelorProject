using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    public GameObject enemyPrefab;
    public int enemyCount = 0;
    private int maxEnemyCount = 2;
    private bool needToSpawnEnemy = true;
    private Transform enemySpawn;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawn = transform.Find("EnemySpawnPoint");
    }

    // Update is called once per frame
    void Update()
    {
        if (needToSpawnEnemy)
        {
            needToSpawnEnemy = false;
            Debug.Log("Spawning enemy");
            PhotonNetwork.Instantiate(enemyPrefab.name, enemySpawn.transform.position, Quaternion.identity);
        }
        else
        {
            StartCoroutine(CheckEnemyCount());
        }

    }

    IEnumerator CheckEnemyCount()
    {
        yield return new WaitForSeconds(1);
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCount < maxEnemyCount)
        {
            needToSpawnEnemy = true;
        }
        else
        {
            needToSpawnEnemy = false;
        }
    }
}
