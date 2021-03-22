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
    private float paintballHealingRate = HealingRateDDAA.Instance.healingRate; //Healing power that this specific paintball does

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
            Debug.Log("Healing rate: " + paintballHealingRate);
            if (collision.collider.gameObject.CompareTag("Player"))
            {
                playerWhoShot.GetComponent<PlayerManager>().HitPlayer(collision.collider.gameObject, paintballHealingRate);   //We heal friend :)
            }
            else if (collision.collider.gameObject.CompareTag("Enemy"))
            {
                playerWhoShot.GetComponent<PlayerManager>().HitEnemy(collision.collider.gameObject, -paintballDamage);     //We damage enemy
            }
        }
        //TODO: paintball hit sound
        Destroy(gameObject);
    }
}
