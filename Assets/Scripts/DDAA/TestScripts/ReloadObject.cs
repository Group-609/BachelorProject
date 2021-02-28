using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadObject : MonoBehaviour, ReloadDDAA.IReloadChangeListener
{

    private float reloadTime = ReloadDDAA.Instance.reloadTime;

    [SerializeField] private TextMesh reloadTimeText;

    private void Awake()
    {
        ReloadDDAA.Instance.SetReloadListener(this);
    }

    private void Start()
    {
        reloadTimeText.text = reloadTime.ToString();
    }

    public void OnReloadTimeChanged()
    {
        Debug.Log("Reload time changed. New reload time is " + reloadTime.ToString());
        reloadTimeText.text = reloadTime.ToString();
    }   
}
