using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class KeyLocationController : MonoBehaviour
{
    private float radius;
    public GameObject sphere;
    public List<GameObject> players;
    private float speedMod;

    void Start()
    {
        StartCoroutine(GetPlayers());
        radius = (sphere.transform.localScale.x/2)-1; //-1 to reduce screen clipping with sphere
    }

    void Update()
    {
        //players = GameObject.FindGameObjectsWithTag("Player"); //Locate in start/singlerun instead. Idk with Photon stuff

        foreach (GameObject player in players)
        {
            if (player.GetComponent<FirstPersonController>().FindClosestKeyLocation() == gameObject) //Only run if this is the closest keyLocation.
            {
                float dist = Vector3.Distance(player.transform.position, transform.position);
                if (dist <= radius && dist > radius - 1)
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

                    float t = radius - dist; 
                    speedMod = Mathf.Lerp(-0.2f, 1, t); //Use difference in distance to key location, and its radius to determine movement speed modifier. Negative values make it so players can allow to be pushed a bit, and still remain stuck.
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

    IEnumerator GetPlayers()
    {
        yield return new WaitForSeconds(0.5f);
        players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
    }
}
