using UnityEngine;

class ProfilerSettingUpdater
{
    [RuntimeInitializeOnLoadMethod]
    static void InitProfiler()
    {
        UnityEngine.Profiling.Profiler.maxUsedMemory = 40000000;
    }
}
