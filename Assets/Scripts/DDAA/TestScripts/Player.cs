using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int killCount;

    private static readonly int initialTime = 5;
    private int time = initialTime;

    [SerializeField] private TextMesh timerText;
    [SerializeField] private TextMesh killCountText;

    private void Start()
    {
        InvokeRepeating("ControlTimer", 1, 0);
    }

    private void ControlTimer()
    {
        time--;
        timerText.text = time.ToString();
        if (time == 0)
        {
            AdjustDifficulty();
            time = initialTime;
            timerText.text = time.ToString();
        }
    }

    public void OnInreaseClicked()
    {
        killCount++;
        killCountText.text = killCount.ToString();
    }

    public void OnDicreaseClicked()
    {
        if (killCount < 0)
        {
            killCount--;
            killCountText.text = killCount.ToString();
        }
    }

    private void AdjustDifficulty()
    {
        KillCountCondition.Instance.ConditionValue = killCount;
        ReloadDDAA.Instance.AdjustReloadTime();
    }
}
