using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtEffect : MonoBehaviour
{

    public bool isTechnicalTesting; //only for testing purposes

    private bool isDisplayingEffect = false;

    public float movementSpeedChange = 0.8f;

    public Texture hurtTexture;
    private float alpha = 1f;

    private AudioSource audioSource;
    [SerializeField] AudioClip[] hurtSound = new AudioClip[0];

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isTechnicalTesting)
            Hit();
    }

    IEnumerator ApplyEffect()
    {
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            yield return null;
        }
        ResetEffect();
        isDisplayingEffect = false;
    }

    private void ResetEffect()
    {
        alpha = 1f;
    }

    void OnGUI()
    {
        if (isDisplayingEffect == true)
        {
            // apply alpha for fade out
            Vector4 tempColor = GUI.color;
            tempColor.w = alpha;
            GUI.color = tempColor;

            //draw texture to GUI
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hurtTexture, ScaleMode.StretchToFill);
        }
    }

    // call this function from the follower's (bee swarm) script, when the distance is close enough
    public void Hit()
    {
        isDisplayingEffect = true;

        if (audioSource != null)
        {
            audioSource.clip = hurtSound[Random.Range(0, hurtSound.Length)];
            audioSource.Play();
        }
        if (isDisplayingEffect)
        {
            ResetEffect();
        }
        else
        {
            StartCoroutine(ApplyEffect());
        }
    }
}
