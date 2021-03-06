﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CustomExtensions
{
    public static object GetRandomItem(this List<object> objects)
    {
        return objects[Random.Range(0, objects.Count)];
    }

    public static object GetRandomItem(this object[] objects)
    {
        return objects[Random.Range(0, objects.Length)];
    }

    public static List<EnemySpawnPoint> ToValidSpawnPoints(this List<EnemySpawnPoint> spawnPointList)
    {
        return spawnPointList.FindAll(
            delegate(EnemySpawnPoint spawnPoint)
            {
                return !spawnPoint.IsEnemyOrPlayerTooClose();
            }
        );
    }

    public static GameObject FindClosestObject(this GameObject targetObject, string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag)
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public static GameObject FindClosestObject(this GameObject targetObject, GameObject[] objects)
    {
        return objects
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public static GameObject FindClosestObject(this GameObject targetObject, List<GameObject> objects)
    {
        return objects
            .OrderBy(gameObject => (gameObject.transform.position - targetObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public static bool Between(this float num, float? lower, float? upper)
    {
        if (lower != null || upper != null)
        {
            if (lower == null)
                return num <= upper;
            if (upper == null)
                return lower <= num;
            return lower <= num && num <= upper;
        }
        else
        {
            Debug.LogError("Both numbers in comparison cannot be null");
            return false;
        }
    }
}