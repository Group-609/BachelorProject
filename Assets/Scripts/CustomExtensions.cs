using System;
using System.Collections.Generic;

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
}