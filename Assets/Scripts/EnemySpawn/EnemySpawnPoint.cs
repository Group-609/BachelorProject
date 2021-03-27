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

    //This is needed, so enemies don't spawn on top of each other
    private float minDistToEnemy = 8;

    public bool IsEnemyTooClose()
    {
        return Vector3.Distance(gameObject.FindClosestObject("Enemy").transform.position, transform.position) < minDistToEnemy;
    }
}
