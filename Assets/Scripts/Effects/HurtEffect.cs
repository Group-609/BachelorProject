using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtEffect : MonoBehaviour
{
    private const float SHAKE_DEFORM_COEF = .2f;

    public bool isTechnicalTesting; //only for testing purposes


    private bool displayEffect = false;

    public float shakeIntensity;
    public float shakeDecay;
    private float currentShakeIntensity;
    private Vector3 originalPos;
    private Quaternion originalRot;

    public Texture hurtTexture;
    private float alpha = 1f;

    private AudioSource audioSource;
    [SerializeField] AudioClip[] hurtSound = new AudioClip[0];

    private bool deformPosition = true;

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
            Shake();
            yield return null;
        }
        ResetEffect();
    }

    private void ResetEffect()
    {
        currentShakeIntensity = shakeIntensity;
        displayEffect = false;
        alpha = 1f;
    }

    private void Shake()
    {
        if (currentShakeIntensity > 0)
        {
            if (deformPosition)
                transform.position = originalPos + Random.insideUnitSphere * shakeIntensity;
            transform.rotation = new Quaternion(
               GetDeformedRotation(originalRot.x),
               GetDeformedRotation(originalRot.y),
               GetDeformedRotation(originalRot.z),
               GetDeformedRotation(originalRot.w)
            );

            currentShakeIntensity -= shakeDecay * Time.deltaTime;
        }
    }

    private float GetDeformedRotation(float axisValue)
    {
        return axisValue + Random.Range(-shakeIntensity, shakeIntensity) * SHAKE_DEFORM_COEF;
    }

    void OnGUI()
    {
        if (displayEffect == true)
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
    public void Hit(bool deformPosition = true)
    {
        if (!displayEffect)
        {
            this.deformPosition = deformPosition;
            displayEffect = true;
            if (deformPosition)
                originalPos = transform.position;
            originalRot = transform.rotation;

            if (audioSource != null)
            {
                audioSource.clip = hurtSound[Random.Range(0, hurtSound.Length)];
                audioSource.Play();
            }

            StartCoroutine(ApplyEffect());
        }
    }
}
