using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTestObject : MonoBehaviour
{
    private int killCount;

    private static readonly int initialTime = 5;
    private int time = initialTime;

    public Button increaseButton;
    public Button decreaseButton;

    public Text timerText;
    public Text killCountText;

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
        ReloadDDAA.Instance.AdjustInGameValue();
        killCount = 0;
        killCountText.text = killCount.ToString();
    }
}
