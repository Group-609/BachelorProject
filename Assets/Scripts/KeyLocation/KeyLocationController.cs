using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;


public class KeyLocationController : MonoBehaviour
{
    public int areaIndex;
    public float radius;
    public GameObject sphere;
    public GameObject clearSphere;
    public int clearSphereSpawnAmount;
    public List<GameObject> players = new List<GameObject>();
    [System.NonSerialized] public float speedMod;
    private int shrinkValue = 20;

    private GameObject directionalLight;
    public float exposureValue;

    public GameObject fountainWaterObject;
    public Color cleanFountainMain;
    public Color cleanFountainSecondary;

    [System.NonSerialized] public bool isEventToDestroySent;
    [System.NonSerialized] public bool hasEventToDestroyStarted;

    public AudioClip clearedClip;
    public float volume;
    private AudioSource audioSource;

    void Start()
    {
        StartCoroutine(GetPlayers());
        sphere.transform.localScale = new Vector3((radius * 2) + 1, (radius * 2) + 1, (radius * 2) + 1); //+1 to reduce screen clipping with sphere
        audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        directionalLight = GameObject.FindGameObjectWithTag("DirectionalLight");
    }

    void Update()
    {
        if (!hasEventToDestroyStarted)
        {
            //Debug.Log("Current level: " + LevelProgressionCondition.Instance.currentLevel + ". Area index: " + areaIndex);
            if (LevelProgressionCondition.Instance.currentLevel == areaIndex)
            {
                //Debug.Log("Checking for players in zone. Area index: " + areaIndex);
                foreach (GameObject player in players)
                {
                    PlayerManager playerManager = player.GetComponent<PlayerManager>();
                    FirstPersonController firstPersonController = player.GetComponent<FirstPersonController>();
                    if (player.FindClosestObject("KeyLocation") == gameObject) //Only run if this is the closest keyLocation.
                    {
                        float distToPlayer = Vector3.Distance(player.transform.position, transform.position);

                        if (!playerManager.isPlayerInKeyLocZone && distToPlayer <= radius)
                        {
                            playerManager.isPlayerInKeyLocZone = true;
                            if (firstPersonController.isActiveAndEnabled)
                                firstPersonController.isPlayerInKeyLocZone = true;
                        }
                        if (firstPersonController.isActiveAndEnabled)
                        {
                            if (playerManager.isPlayerInKeyLocZone && distToPlayer > radius - 1)
                            {
                                if (player.transform.position.x < transform.position.x) //Look at what side of the key location the player is at, so we only stop movement in the wanted direction.
                                {
                                    firstPersonController.isPlayerKeyLocXPositive = true;
                                }
                                else
                                {
                                    firstPersonController.isPlayerKeyLocXPositive = false;
                                }

                                if (player.transform.position.z < transform.position.z)
                                {
                                    firstPersonController.isPlayerKeyLocZPositive = true;
                                }
                                else
                                {
                                    firstPersonController.isPlayerKeyLocZPositive = false;
                                }

                                float radiusToPlayerDistDiff = radius - distToPlayer;
                                speedMod = Mathf.Lerp(-1f, 1, radiusToPlayerDistDiff); //Use difference in distance to key location, and its radius to determine movement speed modifier. Negative values make it so players can allow to be pushed a bit, and still remain stuck as they will rebound to zone edge.
                                firstPersonController.keyLocationSpeedMod = speedMod;
                            }
                            else
                            {
                                speedMod = 1;
                                firstPersonController.keyLocationSpeedMod = speedMod;
                            }
                        }
                    }
                }
            }
            else if (PhotonNetwork.IsMasterClient && LevelProgressionCondition.Instance.currentLevel > areaIndex && !isEventToDestroySent)
            {
                DestroyKeyLocation();
            }
        }
    }

    private IEnumerator GetPlayers()
    {
        while (players.Count == 0)
        {
            yield return new WaitForSeconds(0.5f);
            players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        }
    }

    private void DestroyKeyLocation()
    {
        Debug.Log("Raised event to destroy key location");
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(GameManager.destroyKeyLocationEvent, areaIndex, raiseEventOptions, SendOptions.SendReliable);
        isEventToDestroySent = true;

    }

    void AreaClearedSound()
    {
        audioSource.clip = clearedClip;
        audioSource.volume = volume;
        audioSource.Play();
        Debug.Log("Area Cleared");
        
    }

    public IEnumerator BeginDestroyingProcess()
    {
        Debug.Log("Begin Destroying process");
        hasEventToDestroyStarted = true;
        foreach (GameObject player in players)
        {
            player.GetComponent<FirstPersonController>().keyLocationSpeedMod = 1; //reset speedmod in case a player should be slowed by the edge when the location is disabled.
            player.GetComponent<PlayerManager>().isPlayerInKeyLocZone = false;
            player.GetComponent<FirstPersonController>().isPlayerInKeyLocZone = false;
        }
        while (sphere.transform.localScale.x > 0)
        {
            sphere.transform.localScale -= new Vector3(shrinkValue, shrinkValue, shrinkValue) * Time.deltaTime;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerManager playMan = players[i].GetComponent<PlayerManager>();
                if (playMan.health < 100)
                {
                    playMan.health += playMan.healthRecoverySpeed * Time.deltaTime;
                    playMan.UpdatePlayerHealthUI();
                }
                else
                {
                    playMan.health = 100;
                    playMan.UpdatePlayerHealthUI();
                }
            }

            yield return null;
        }
        if (sphere.transform.localScale.x <= 0)
        {
            sphere.SetActive(false);
            for (int i = 0; i < clearSphereSpawnAmount; i++)
            {
                Instantiate(clearSphere, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y * 4, gameObject.transform.position.z), Quaternion.identity);
            }
            fountainWaterObject.GetComponent<Renderer>().material.SetColor("_MainColor", cleanFountainMain);
            fountainWaterObject.GetComponent<Renderer>().material.SetColor("_SecondaryColor", cleanFountainSecondary);
            directionalLight.GetComponent<Light>().intensity = exposureValue;
            RenderSettings.skybox.SetFloat("_Exposure", exposureValue);
            AreaClearedSound();
        }
    }

    public static KeyLocationController GetKeyLocationToDestroy(int areaIndex)
    {
        return GameObject.FindGameObjectsWithTag("KeyLocation").ToList().Find(
                delegate (GameObject keyLocation)
                {
                    return keyLocation.GetComponent<KeyLocationController>().areaIndex == areaIndex;
                }
            ).GetComponent<KeyLocationController>();
    }
}
