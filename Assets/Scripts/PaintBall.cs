using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class PaintBall : MonoBehaviour
{

    [Tooltip("The time it takes for the bullet to disappear")]
    private float despawnTime = 5.0f;

    [System.NonSerialized]
    public GameObject playerWhoShot;

    [System.NonSerialized]
    public bool isLocal; //true if this is a real bullet that does damage

    public GameObject impactSphere;
    public int impactSphereSpawnAmount;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, .33f));

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
            if (collision.collider.gameObject.CompareTag("Player"))
            {
                playerWhoShot.GetComponent<PlayerManager>().HitPlayer(collision.collider.gameObject, true);   //We heal friend :)
            }
            else if (collision.collider.gameObject.CompareTag("Enemy") && !collision.collider.gameObject.GetComponent<EnemyController>().isBlobified)
            {
                playerWhoShot.GetComponent<PlayerManager>().HitEnemy(collision.collider.gameObject);     //We damage enemy
            }
        }

        for (int i = 0; i < impactSphereSpawnAmount; i++)
        {
            GameObject currentImpactSphere = Instantiate(impactSphere, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity);
            currentImpactSphere.GetComponent<Renderer>().material.color = gameObject.GetComponent<Renderer>().material.color;
        }

        //TODO: paintball hit sound
        Destroy(gameObject);
    }
}
