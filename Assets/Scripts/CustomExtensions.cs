using System;
using System.Collections.Generic;

public static class CustomExtensions
{
    public static void AddToEnemySpawnPointList(this List<List<EnemySpawnPoint>> currentSpawnPointList, EnemySpawnPoint spawnPointToAdd)
    {
        try
        {
            currentSpawnPointList[spawnPointToAdd.areaIndex].Add(spawnPointToAdd);
        } 
        catch(ArgumentOutOfRangeException e)
        {
            currentSpawnPointList.Add(new List<EnemySpawnPoint>() { spawnPointToAdd });
        }
    }

    public static List<EnemySpawnPoint> ToListWithValidPoints(this List<EnemySpawnPoint> spawnPointList)
    {
        return spawnPointList.FindAll(
            delegate(EnemySpawnPoint spawnPoint)
                {
                    return spawnPoint.IsInValidDistanceToSpawn();
                }
            );
    }
}