using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;

[RequireComponent(typeof (FirstPersonController))]
public class HurtEffect : MonoBehaviourPun
{
    private bool isDisplayingEffect = false;

    public float minMovementMultiplier = 0.5f;
    public float movementMultiplierFadeOutSpeed = 0.5f;

    public Texture hurtTexture1;
    public Texture hurtTexture2;
    public Texture hurtTexture3;
    public float textureFadeOutSpeed = 1f;
    private float opacity = 1f;

    [SerializeField] 
    private AudioClip[] hurtSound = new AudioClip[0];
    private AudioSource audioSource;

    private FirstPersonController controller;
    private PlayerManager player;

    private void Start()
    {
        controller = gameObject.GetComponent<FirstPersonController>();
        audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        audioSource.spatialBlend = 1;
        player = gameObject.GetComponent<PlayerManager>();
    }

    private IEnumerator ApplyEffect()
    {
        isDisplayingEffect = true;
        controller.speedMultiplier = minMovementMultiplier;
        opacity = 1;

        while (opacity > 0 || controller.speedMultiplier < 1f)
        {
            if (opacity > 0)
                opacity -= textureFadeOutSpeed * Time.deltaTime;
            if (controller.speedMultiplier < 1f)
                controller.speedMultiplier = Mathf.Max(minMovementMultiplier, controller.speedMultiplier + movementMultiplierFadeOutSpeed * Time.deltaTime);
            yield return null;
        }
        isDisplayingEffect = false;
    }

    void OnGUI()
    {
        if (isDisplayingEffect == true)
        {
            // apply alpha for fade out
            Vector4 tempColor = GUI.color;
            tempColor.w = opacity;
            GUI.color = tempColor;

            //draw texture to GUI
            if (player.health <= 33)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hurtTexture3, ScaleMode.StretchToFill);
            }
            if (player.health <= 66)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hurtTexture2, ScaleMode.StretchToFill);
            }
            if (player.health <= 100)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hurtTexture1, ScaleMode.StretchToFill);
            }

        }
    }

    public void Hit()
    {
        if (photonView.IsMine)
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.clip = (AudioClip)hurtSound.GetRandomItem();
                audioSource.Play();
            }
            if (isDisplayingEffect)
            {
                opacity = Mathf.Min(1f, opacity + 0.5f);
            }
            else
            {
                StartCoroutine(ApplyEffect());
            }
        }
    }
}
