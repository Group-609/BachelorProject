using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadObject : MonoBehaviour, IValueChangeListener
{

    private float reloadTime; 

    public Text reloadTimeText;

    private void Awake()
    {
        reloadTime = ReloadDDAA.Instance.reloadTime;
        ReloadDDAA.Instance.SetReloadListener(this);
    }

    private void Start()
    {
        reloadTimeText.text = reloadTime.ToString();
    }

    public void OnValueChanged(float value)
    {
        reloadTime = value;
        reloadTimeText.text = reloadTime.ToString();
    }
}
