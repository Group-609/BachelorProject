using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintballClientSide : MonoBehaviour
{
    [SerializeField]
    private float despawnTime = 2.0f;
    [System.NonSerialized]
    public GameObject playerWhoShot;

    void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        //If the object is the player who shot
        if (collision.collider.gameObject == playerWhoShot)
        {
            return;
        }
        Destroy(gameObject);
    }
}
