using System.Collections.Generic;

public static class CustomExtensions
{
    public static void AddToEnemySpawnPointList(this List<List<EnemySpawnPoint>> currentSpawnPointList, EnemySpawnPoint spawnPointToAdd)
    {
        if (currentSpawnPointList[spawnPointToAdd.areaIndex] == null)
        {
            currentSpawnPointList.Insert(spawnPointToAdd.areaIndex, new List<EnemySpawnPoint>() { spawnPointToAdd });
        }
        else
        {
            currentSpawnPointList[spawnPointToAdd.areaIndex].Add(spawnPointToAdd);
        }
    }
}