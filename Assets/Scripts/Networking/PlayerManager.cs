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
using System.Collections;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;

namespace Photon.Pun.Demo.PunBasics
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Handles fire Input.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable, LevelProgressionCondition.LevelProgressionListener, IOnEventCallback
    {
        #region Public Fields

        [Header("DDA friendly variables")]
        //---------------------------------------------------------
        [Tooltip("The current Health of our player")]
        public float health = 100f;

        [Tooltip("The health of our player when he spawns")]
        public float startingHealth = 100f;

        [Tooltip("Speed of this player's paintballs")]
        public float paintBallSpeed = 15f;

        [Tooltip("Damage of this player's paintballs")]
        public float paintballDamage;

        [Tooltip("Time it takes for the player to get control back after dying")]
        public float respawnTime;

        [Tooltip("Time between 2 shots")]
        public float shootWaitTime = 0.9f;

        [Header("DDA system variables")]
        [NonSerialized]
        public int stunCount;

        [Header("Sounds")]

        public AudioClip shootingClip;

        [Header("Other")]

        [Tooltip("The game manager object.")]
        public GameManager gameManager;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        //where the player will respawn after both players get stunned
        [NonSerialized]
        public Transform respawnTransform;

        [NonSerialized] public bool isPlayerInKeyLocZone = false;   //is this player in a key location zone

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

        private FirstPersonController fpsController;

        private float standUpAnimationTime = 1f;

        //True, when the user is firing
        bool IsFiring;

        //True when the shooting coroutine is running, used for fake bullets of other player
        bool waitingToShoot = false;

        private bool isReturningControl = false;

        private Animator animator;
        private Animator animatorHands;

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
            }
            paintGun = gameObject.transform.Find("FirstPersonCharacter").Find("CharacterHands").Find("Armature").Find("Base").Find("Base.002").Find("Base.003").Find("hand_right").Find("hand_right.001").Find("PaintGun");

            if (PhotonNetwork.IsMasterClient)
            {
                LevelProgressionCondition.Instance.AddLevelProgressionListener(this);
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
            try{animator = GetComponent<Animator>();}
            catch{Debug.LogError("Missing Animator Component on player Prefab.", this);}

            try{animatorHands = gameObject.transform.Find("FirstPersonCharacter").Find("CharacterHands").GetComponent<Animator>();}
            catch {Debug.LogError("Missing Animator Component on player hands Prefab.", this);}

            try{fpsController = GetComponent<FirstPersonController>();}
            catch{Debug.LogError("Missing fpsController.", this);}

            if(gameManager == null)
            {
                gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
            }
            try{respawnTransform = gameManager.transform.Find("PlayerRespawnPoint").transform;}
            catch{Debug.LogError("<Color=Red><b>Missing</b></Color> Respawn location", this);}
        }


		public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable ();
            PhotonNetwork.RemoveCallbackTarget(this);
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            // local player
            if (photonView.IsMine)
            {
                if (health > 0f)
                {
                    if (!isReturningControl)
                    {
                        if (fpsController.isStunned)
                        {
                            animator.SetBool("isDown", false);
                            animatorHands.SetBool("isDown", false);
                            StartCoroutine(ReturnPlayerControl(standUpAnimationTime));
                        }
                        else
                        {
                            AnimateWalking();
                            ProcessInputs();
                        }
                    }
                }
                else if (!fpsController.isStunned)
                {
                    Stun();
                    animator.SetBool("isDown", true);
                    animatorHands.SetBool("isDown", true);
                }   
            }
            if (IsFiring && !waitingToShoot && health > 0f)
            {
                AnimateShoot();
                StartCoroutine(ShootPaintball());
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            
            byte eventCode = photonEvent.Code;

            if (eventCode == GameManager.respawnEvent)
            {
                transform.position = respawnTransform.position;
                isPlayerInKeyLocZone = false;
                StartCoroutine(SetPlayerOutsideKeyLocationZone());
            }
            if (photonView.IsMine) 
            {
                if (eventCode == GameManager.respawnEvent)
                    Respawn();
                if (eventCode == GameManager.destroyKeyLocationEvent)
                {
                    Debug.Log("Destroy key location index " + (int) photonEvent.CustomData);
                    StartCoroutine(KeyLocationController.GetKeyLocationToDestroy((int) photonEvent.CustomData).BeginDestroyingProcess());
                }
            }
        }

        //Called when all players are stunned 
        public void Respawn()
        {
            GetComponentInChildren<ApplyPostProcessing>().vignetteLayer.intensity.value = 0;
            fpsController.enabled = false;   //We disable the script so that we can teleport the player
            GetComponent<FirstPersonController>().isPlayerInKeyLocZone = false;
            this.health = startingHealth;
            animator.SetBool("isDown", false);
            animatorHands.SetBool("isDown", false);
            StartCoroutine(ReturnPlayerControl(respawnTime + standUpAnimationTime)); //we reenable the FirstPersonController script after the respawn time is done
        }

        //Call this function from non networked projectiles to change a player's health. This allows to avoid having a PhotonView on every paintball which is very inefficient.
        //We have to call the RPC from this function because RPCs must be called from gameobjects that have a PhotonView component.
        public void HitPlayer(GameObject player, float healthChange)
        {
            photonView.RPC(nameof(ChangeHealth), RpcTarget.All, healthChange, player.GetComponent<PhotonView>().ViewID);
        }
        
        /// <summary>
        /// Change the player's health.
        /// </summary>
        [PunRPC]
        public void ChangeHealth(float value, int targetViewID)
        {
            PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health += value;
            if (PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health > startingHealth)
                PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health = startingHealth;
            if (PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health < 0)
                PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health = 0;
        }

        [PunRPC]
        public void Stunned(int targetViewID)
        {
            GameObject playerObject = PhotonView.Find(targetViewID).gameObject;
            playerObject.GetComponent<PlayerManager>().stunCount++;
            //Debug.Log("Someone is stunned! Player's stun count is " + stunCount);
            if (photonView.IsMine)
            {
                StunCondition.Instance.localPlayerStuntCount++;
                //Debug.Log("We were stunned! Local player stun count is " + StunCondition.Instance.localPlayerStuntCount);
            }
        }

        //Function to call when an enemy is hit. 
        // enemy - the enemy we hit
        // healthChange - the effect on the enemies health (negative values for hurting)
        public void HitEnemy(GameObject enemy, float healthChange)
        {
            photonView.RPC("ChangeEnemyHealth", RpcTarget.All, healthChange, enemy.GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        public void ChangeEnemyHealth(float value, int targetViewID)
        {
            PhotonView.Find(targetViewID).gameObject.GetComponent<EnemyController>().currentHealth += value;
            PhotonView.Find(targetViewID).gameObject.GetComponent<EnemyController>().OnDamageTaken();
        }

        [PunRPC]
        public void AnimateShoot()
        {
            animator.Play("Shoot");
            animatorHands.Play("Shoot");
          
        }

        private void PlayShootingSound()
        {
            GetComponent<AudioSource>().PlayOneShot(shootingClip);
        }

        public void OnLevelFinished()
        {
            HealingRateDDAA.Instance.AdjustInGameValue();
        }

        #endregion

        #region Private Methods

        //Disables movement
        void Stun()
        {
            IsFiring = false;
            fpsController.isStunned = true;
            GetComponentInChildren<ApplyPostProcessing>().vignetteLayer.intensity.value = 1;
            photonView.RPC(nameof(Stunned), RpcTarget.All, GetComponent<PhotonView>().ViewID);
        }

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
                this.IsFiring = true;
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (this.IsFiring)
                {
                    this.IsFiring = false;
                }
            }
        }

        void AnimateWalking()
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                animator.SetBool("isMovingForward", true);
                animator.SetBool("isMovingBackward", false);
                animatorHands.SetBool("isMovingForward", true);
                animatorHands.SetBool("isMovingBackward", false);
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                animator.SetBool("isMovingForward", false);
                animator.SetBool("isMovingBackward", true);
                animatorHands.SetBool("isMovingForward", false);
                animatorHands.SetBool("isMovingBackward", true);
            }
            else
            {
                animator.SetBool("isMovingForward", false);
                animator.SetBool("isMovingBackward", false);
                animatorHands.SetBool("isMovingForward", false);
                animatorHands.SetBool("isMovingBackward", false);
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                animator.SetBool("isMovingRight", true);
                animator.SetBool("isMovingLeft", false);
                animatorHands.SetBool("isMovingRight", true);
                animatorHands.SetBool("isMovingLeft", false);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", true);
                animatorHands.SetBool("isMovingRight", false);
                animatorHands.SetBool("isMovingLeft", true);
            }
            else
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
                animatorHands.SetBool("isMovingRight", false);
                animatorHands.SetBool("isMovingLeft", false);
            }
        }

        private IEnumerator SetPlayerOutsideKeyLocationZone()
        {
            yield return new WaitForSeconds(respawnTime + standUpAnimationTime);
            isPlayerInKeyLocZone = false;
        }

        private IEnumerator ReturnPlayerControl(float waitTime)
        {
            isReturningControl = true;
            fpsController.enabled = true;
            yield return new WaitForSeconds(waitTime);
            GetComponentInChildren<ApplyPostProcessing>().vignetteLayer.intensity.value = 0;
            fpsController.isStunned = false;
            isReturningControl = false;
        }
        
        private IEnumerator ShootPaintball()
        {
            waitingToShoot = true;
            
            GameObject paintball;
            paintball = Instantiate(paintballPrefab, paintGun.transform.position, Quaternion.identity);
            paintball.GetComponent<PaintBall>().playerWhoShot = this.gameObject;
            paintball.GetComponent<PaintBall>().paintballDamage = this.paintballDamage;
            paintball.GetComponent<PaintBall>().isLocal = photonView.IsMine;
            paintball.GetComponent<Rigidbody>().velocity = paintGun.TransformDirection(Vector3.forward * paintBallSpeed);
            PlayShootingSound();
            yield return new WaitForSeconds(shootWaitTime);
            waitingToShoot = false;
        }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(this.health);
                stream.SendNext(this.IsFiring);
            }
            else
            {
                // Network player, receive data
                this.health = (float)stream.ReceiveNext();
                this.IsFiring = (bool)stream.ReceiveNext();
            }
        }

        #endregion
    }
}