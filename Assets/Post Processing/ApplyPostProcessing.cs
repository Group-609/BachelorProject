using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;
using UnityEngine.Rendering.PostProcessing;

public class ApplyPostProcessing : MonoBehaviour
{
    PlayerManager playerManager;
    public Vignette vignetteLayer;
    Grayscale grayscaleLayer;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = transform.parent.GetComponent<PlayerManager>();
        PostProcessVolume volume = transform.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out vignetteLayer);
        volume.profile.TryGetSettings(out grayscaleLayer);
    }

    // Update is called once per frame
    void Update()
    {
        grayscaleLayer.blend.value = 1 - playerManager.health / playerManager.startingHealth;
    }
}
