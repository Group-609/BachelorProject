using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    public GameObject enemyPrefab;


    [SerializeField]
    private int maxEnemyCount = 2;

    [SerializeField]
    private int spawnInterval = 3;

    private bool coroutineRunning = false;
    private Transform enemySpawn;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawn = transform.Find("EnemySpawnPoint");
    }

    // Update is called once per frame
    void Update()
    {
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
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount)
        {
            PhotonNetwork.Instantiate(enemyPrefab.name, enemySpawn.transform.position, Quaternion.identity);
            Debug.Log("Spawning enemy");
        }
        coroutineRunning = false;
    }
}
