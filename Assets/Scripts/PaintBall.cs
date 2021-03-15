using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class PaintBall : MonoBehaviour
{

    [Tooltip("The time it takes for the bullet to disappear")]
    private float despawnTime = 2.0f;

    [System.NonSerialized]
    public GameObject playerWhoShot;

    [System.NonSerialized]
    public float paintballDamage; //Damage this specific paintball does

    [System.NonSerialized]
    public bool isLocal; //true if this is a real bullet that does damage

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
        else if (isLocal)
        {
            if (collision.collider.gameObject.tag == "Player")
            {
                playerWhoShot.GetComponent<PlayerManager>().HitPlayer(collision.collider.gameObject, paintballDamage);      //We heal friend :)
            }
            else if (collision.collider.gameObject.tag == "Enemy")
            {
                playerWhoShot.GetComponent<PlayerManager>().HitEnemy(collision.collider.gameObject, -paintballDamage);     //We damage enemy
            }
        }
        Destroy(gameObject);
    }
}
