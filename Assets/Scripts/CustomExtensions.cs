using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class CustomExtensions
{
    public static void AddToEnemySpawnPointList(this List<List<EnemySpawnPoint>> spawnPointList, EnemySpawnPoint spawnPointToAdd)
    {
        try
        {
            spawnPointList[spawnPointToAdd.areaIndex].Add(spawnPointToAdd);
        } 
        catch(ArgumentOutOfRangeException)
        {
            spawnPointList.Add(new List<EnemySpawnPoint>() { spawnPointToAdd });
        }
    }

    public static List<EnemySpawnPoint> ToValidSpawnPoints(this List<EnemySpawnPoint> spawnPointList)
    {
        return spawnPointList.FindAll(
            delegate(EnemySpawnPoint spawnPoint)
            {
                return !spawnPoint.IsEnemyTooClose();
            }
        );
    }

    public static GameObject FindClosestObject(this GameObject targetObject, string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag)
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public static GameObject FindClosestObject(this GameObject targetObject, GameObject[] ojbects)
    {
        return ojbects
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public static GameObject FindClosestObject(this GameObject targetObject, List<GameObject> ojbects)
    {
        return ojbects
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }
}