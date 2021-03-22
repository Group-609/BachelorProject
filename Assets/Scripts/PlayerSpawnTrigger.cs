using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnTrigger : MonoBehaviour
{
	private Transform playerRespawnTransform;
	// Start is called before the first frame update
	void Start()
	{
		playerRespawnTransform = GameObject.Find("Game Manager").transform.Find("PlayerRespawnPoint").transform;
	}

	void OnTriggerEnter(Collider collider)
	{
		//If the object is the player who shot
		if (collider.gameObject.tag == "Player")
		{
			playerRespawnTransform.transform.position = transform.parent.position;
			playerRespawnTransform.transform.rotation = transform.parent.rotation;
		}
	}
}
