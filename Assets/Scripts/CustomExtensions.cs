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
        catch(ArgumentOutOfRangeException e)
        {
            spawnPointList.Add(new List<EnemySpawnPoint>() { spawnPointToAdd });
        }
    }
}