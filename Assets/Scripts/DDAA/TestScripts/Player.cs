using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private int killCount;

    private static readonly int initialTime = 5;
    private int time = initialTime;

    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;

    [SerializeField] private Text timerText;
    [SerializeField] private Text killCountText;

    private void Start()
    {
        increaseButton.onClick.AddListener(OnInreaseClicked);
        decreaseButton.onClick.AddListener(OnDecreaseClicked);
        InvokeRepeating("ControlTimer", 0, 1);
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

    private void OnInreaseClicked()
    {
        killCount++;
        killCountText.text = killCount.ToString();
    }

    public void OnDecreaseClicked()
    {
        if (killCount > 0)
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
