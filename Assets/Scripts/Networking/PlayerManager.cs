// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.Demo.PunBasics
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Handles fire Input.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields

        //DDA friendly variables
        //---------------------------------------------------------
        [Tooltip("The current Health of our player")]
        public float Health = 100f;

        [Tooltip("The Health of our player when he spawns")]
        public float startingHealth = 100f;

        [Tooltip("Speed of this player's paintballs")]
        public float paintBallSpeed = 50f;

        [Tooltip("Speed of this player's paintballs")]
        public float paintballDamage;
        //---------------------------------------------------------

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The game manager object.")]
        public GameManager gameManager;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;

        [Tooltip("Prefab of paintball to shoot")]
        [SerializeField]
        private GameObject paintballPrefab;

        [Tooltip("Transform the paint balls come from")]
        private Transform paintGun;

        //True, when the user is firing
        bool IsFiring;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                paintGun = gameObject.transform.Find("FirstPersonCharacter").Find("PaintGun");
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Create the UI
            if (this.playerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }


		public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable ();
		}


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            // we only process Inputs and check health if we are the local player
            if (photonView.IsMine)
            {
                this.ProcessInputs();

                if (this.Health <= 0f)
                {
                    transform.position = gameManager.transform.position;        //TODO: Damn first person controller resets this variable breaking the respawn. 
                    this.Health = startingHealth;
                }
            }
        }

        /// <summary>
        /// Change the player's health.
        /// </summary>
        [PunRPC]
        public void ChangeHealth(float value, int targetViewID)
        {
            Debug.LogError("Health changed");
            PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().Health += value;
        }

        [PunRPC]
        public void ShootFakeBullet()
        {
            //TODO: Create fake paintball that does the graphics part
        }

        //Call this function from non networked projectiles to change a player's health. This allows to avoid having a PhotonView on every paintball which is very inefficient.
        //We have to call the RPC from this function because RPCs must be called from gameobjects that have a PhotonView component.
        public void HitPlayer(GameObject player, float healthChange)
        {
            photonView.RPC("ChangeHealth", RpcTarget.All, healthChange, player.GetComponent<PhotonView>().ViewID);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
                // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //	return;
                }

                if (!this.IsFiring)
                {
                    this.IsFiring = true;
                    GameObject paintball = Instantiate(paintballPrefab, paintGun.transform.position, Quaternion.identity);
                    paintball.GetComponent<PaintBall>().playerWhoShot = this.gameObject;
                    paintball.GetComponent<PaintBall>().paintballDamage = this.paintballDamage;
                    paintball.GetComponent<Rigidbody>().velocity = paintGun.TransformDirection(Vector3.forward * paintBallSpeed);
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (this.IsFiring)
                {
                    this.IsFiring = false;
                }
            }
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}