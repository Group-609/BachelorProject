using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Tooltip("If true - spawn point is assigned to area (control it with areaIndex). If false - spawn point is based on the places in between areas")]
    public bool isAreaBased;

    [Tooltip("Defines, which area this spawn point is assigned to")]
    public int areaIndex;

    public float minDistanceToPlayer = 50;

    public bool IsInValidDistanceToSpawn()
    {
        return Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position) < minDistanceToPlayer;
    }
}
