using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class KeyLocationController : MonoBehaviour
{
    private float radius;
    public GameObject sphere;
    public GameObject[] players = new GameObject[2];
    private float speedMod;

    void Start()
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        radius = (sphere.transform.localScale.x/2)-1; //-1 to reduce screen clipping with sphere
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player"); //Locate in start/singlerun instead. Idk with Photon stuff

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<FirstPersonController>().FindClosestKeyLocation() == gameObject) //Only run if this is the closest keyLocation.
            {
                float dist = Vector3.Distance(players[i].transform.position, transform.position);
                if (dist <= radius && dist > radius - 1)
                {
                    if (players[i].transform.position.x < transform.position.x) //Look at what side of the key location the player is at, so we only stop movement in the wanted direction.
                    {
                        players[i].GetComponent<FirstPersonController>().isPlayerKeyLocXPositive = true;
                    } 
                    else
                    {
                        players[i].GetComponent<FirstPersonController>().isPlayerKeyLocXPositive = false;
                    }

                    if (players[i].transform.position.z < transform.position.z)
                    {
                        players[i].GetComponent<FirstPersonController>().isPlayerKeyLocZPositive = true;
                    }
                    else
                    {
                        players[i].GetComponent<FirstPersonController>().isPlayerKeyLocZPositive = false;
                    }

                    float t = radius - dist; 
                    speedMod = Mathf.Lerp(-0.2f, 1, t); //Use difference in distance to key location, and its radius to determine movement speed modifier. Negative values make it so players can allow to be pushed a bit, and still remain stuck.
                    players[i].GetComponent<FirstPersonController>().keyLocationSpeedMod = speedMod;
                }
                else
                {
                    speedMod = 1;
                    players[i].GetComponent<FirstPersonController>().keyLocationSpeedMod = speedMod;
                }
            }
        }
    }
}
