using UnityEngine;
using Photon.Pun;

public class PaintBall : MonoBehaviour, IPunInstantiateMagicCallback
{
    private int playerWhoShotViewID;

    //Here, we receive data sent during instantiation, photon networking specific
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        Vector3 velocity = (Vector3)instantiationData[0];
        playerWhoShotViewID = (int)instantiationData[1];
        GetComponent<Rigidbody>().velocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.collider);
        if (collision.collider.gameObject.GetComponent<PhotonView>() != null)
        {
            if (collision.collider.gameObject.GetComponent<PhotonView>().ViewID == playerWhoShotViewID)
            {
                return;
            }
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
