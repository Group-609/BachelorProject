using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof (FirstPersonController))]
public class HurtEffect : MonoBehaviour
{
    private bool isDisplayingEffect = false;

    public float minMovementMultiplier = 0.5f;
    public float movementMultiplierFadeOutSpeed = 0.5f;

    public Texture hurtTexture;
    public float textureFadeOutSpeed = 1f;
    private float opacity = 1f;

    [SerializeField] 
    private AudioClip[] hurtSound = new AudioClip[0];
    private AudioSource audioSource;

    private FirstPersonController controller;

    private void Start()
    {
        controller = gameObject.GetComponent<FirstPersonController>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private IEnumerator ApplyEffect()
    {
        isDisplayingEffect = true;
        controller.speedMultiplier = minMovementMultiplier;

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
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hurtTexture, ScaleMode.StretchToFill);
        }
    }

    public void Hit()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = (AudioClip) hurtSound.GetRandomItem();
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
