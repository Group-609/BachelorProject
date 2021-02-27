using UnityEngine;
using Photon.Pun;

public class PaintBall : MonoBehaviour, IPunInstantiateMagicCallback
{
    //Here, we receive data sent during instantiation, photon networking specific
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        Vector3 velocity = (Vector3)instantiationData[0];
        GetComponent<Rigidbody>().velocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.collider);
        
        PhotonNetwork.Destroy(gameObject);  //TODO: stop hitting the player who shot the bullet
    }
}
