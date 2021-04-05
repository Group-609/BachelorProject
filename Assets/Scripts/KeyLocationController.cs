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
    public float speedMod;
    private int shrinkValue = 20;

    [System.NonSerialized]
    public bool hasEventToDestroyStarted;
    [System.NonSerialized]
    public bool isDestroyed;

    public AudioClip clearedClip;
    private AudioSource audioSource;

    void Start()
    {
        StartCoroutine(GetPlayers());
        sphere.transform.localScale = new Vector3((radius * 2) + 1, (radius * 2) + 1, (radius * 2) + 1); //+1 to reduce screen clipping with sphere
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDestroyed || !hasEventToDestroyStarted)
        {
            if (LevelProgressionCondition.Instance.currentLevel == areaIndex)
            {
                foreach (GameObject player in players)
                {
                    if (player.FindClosestObject("KeyLocation") == gameObject) //Only run if this is the closest keyLocation.
                    {
                        float distToPlayer = Vector3.Distance(player.transform.position, transform.position);

                        if (!player.GetComponent<FirstPersonController>().isPlayerInKeyLocZone && distToPlayer <= radius)
                        {
                            player.GetComponent<FirstPersonController>().isPlayerInKeyLocZone = true;
                        }

                        if (player.GetComponent<FirstPersonController>().isPlayerInKeyLocZone && distToPlayer > radius - 1)
                        {
                            if (player.transform.position.x < transform.position.x) //Look at what side of the key location the player is at, so we only stop movement in the wanted direction.
                            {
                                player.GetComponent<FirstPersonController>().isPlayerKeyLocXPositive = true;
                            }
                            else
                            {
                                player.GetComponent<FirstPersonController>().isPlayerKeyLocXPositive = false;
                            }

                            if (player.transform.position.z < transform.position.z)
                            {
                                player.GetComponent<FirstPersonController>().isPlayerKeyLocZPositive = true;
                            }
                            else
                            {
                                player.GetComponent<FirstPersonController>().isPlayerKeyLocZPositive = false;
                            }

                            float radiusToPlayerDistDiff = radius - distToPlayer;
                            speedMod = Mathf.Lerp(-1f, 1, radiusToPlayerDistDiff); //Use difference in distance to key location, and its radius to determine movement speed modifier. Negative values make it so players can allow to be pushed a bit, and still remain stuck as they will rebound to zone edge.
                            player.GetComponent<FirstPersonController>().keyLocationSpeedMod = speedMod;
                        }
                        else
                        {
                            speedMod = 1;
                            player.GetComponent<FirstPersonController>().keyLocationSpeedMod = speedMod;
                        }
                    }
                }
            }
            else if (LevelProgressionCondition.Instance.currentLevel > areaIndex && !isDestroyed && !hasEventToDestroyStarted)
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
        hasEventToDestroyStarted = true;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(GameManager.destroyKeyLocationEvent, 0, raiseEventOptions, SendOptions.SendReliable);
    }

    public IEnumerator BeginDestroyingProcess()
    {
        Debug.Log("Begin Destroying process");
        foreach (GameObject player in players)
        {
            player.GetComponent<FirstPersonController>().keyLocationSpeedMod = 1; //reset speedmod in case a player should be slowed by the edge when the location is disabled.
            player.GetComponent<FirstPersonController>().isPlayerInKeyLocZone = false;
        }
        while (sphere.transform.localScale.x > 0)
        {
            sphere.transform.localScale -= new Vector3(shrinkValue, shrinkValue, shrinkValue) * Time.deltaTime;
            yield return null;
        }
        if (sphere.transform.localScale.x <= 0)
        {
            sphere.SetActive(false);
            isDestroyed = true;
            for (int i = 0; i < clearSphereSpawnAmount; i++)
            {
                Instantiate(clearSphere, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y * 4, gameObject.transform.position.z), Quaternion.identity);
            }
        }
    }

    public static KeyLocationController GetKeyLocationToDestroy()
    {
        return GameObject.FindGameObjectsWithTag("KeyLocation").ToList().Find(
                delegate (GameObject keyLocation)
                {
                    KeyLocationController controller = keyLocation.GetComponent<KeyLocationController>();
                    return controller.hasEventToDestroyStarted && !controller.isDestroyed;
                }
            ).GetComponent<KeyLocationController>();
    }
}
