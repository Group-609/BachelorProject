using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadObject : MonoBehaviour, ReloadDDAA.IReloadChangeListener
{

    private float reloadTime = ReloadDDAA.Instance.reloadTime;

    [SerializeField] private Text reloadTimeText;

    private void Awake()
    {
        ReloadDDAA.Instance.SetReloadListener(this);
    }

    private void Start()
    {
        reloadTimeText.text = reloadTime.ToString();
    }

    public void OnReloadTimeChanged(float reloadTime)
    {
        this.reloadTime = reloadTime;
        Debug.Log("Reload time changed. New reload time is " + reloadTime.ToString());
        reloadTimeText.text = reloadTime.ToString();
    }   
}
