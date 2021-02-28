using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class PaintBall : MonoBehaviour, IPunInstantiateMagicCallback
{
    private int playerWhoShotViewID;
    private float bulletDamage; //Damage this specific bullet does

    //Here, we receive data sent during instantiation, photon networking specific
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        Vector3 velocity = (Vector3)instantiationData[0];
        playerWhoShotViewID = (int)instantiationData[1];
        bulletDamage = (float)instantiationData[2];
        GetComponent<Rigidbody>().velocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        //
        //If hit a networked object
        if (collision.collider.gameObject.GetComponent<PhotonView>() != null)
        {
            //If the object is the player who shot
            if (collision.collider.gameObject.GetComponent<PhotonView>().ViewID == playerWhoShotViewID)
            {
                return;
            }
            //Is it a different player
            else if(collision.collider.gameObject.GetComponent<PlayerManager>())
            {
                collision.collider.gameObject.GetComponent<PlayerManager>().ChangeHealth(-bulletDamage);      //We damage friend :( for now for testing reasons, later change to heal friend :)
            }
            /*  //Code for when we create an enemy
            else if (collision.collider.gameObject.GetComponent<Enemy>())
            {
                collision.collider.gameObject.GetComponent<Enemy>().ChangeHealth(-bulletDamage);     //We damage enemy
            }
            */

        }
        PhotonNetwork.Destroy(gameObject);
    }
}
