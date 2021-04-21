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
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;

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

        [Tooltip("Speed at which health temporarily recovers at zone clear")]
        public float healthRecoverySpeed = 30f;

        [Tooltip("Speed of this player's paintballs")]
        public float paintBallSpeed = 15f;

        [Tooltip("Damage of this player's paintballs")]
        private float paintballDamage = PlayerPainballDamageDDAA.Instance.paintballDamage;

        [Tooltip("Time it takes for the player to get control back after dying")]
        public float respawnTime;

        [Tooltip("Time between 2 shots")]
        public float shootWaitTime = 0.9f;

        [Header("DDA system variables")]
        [NonSerialized]
        public int stunCount;

        [NonSerialized]
        public float totalDamageReceived;

        [NonSerialized]
        public int defeatedEnemiesCount;

        [Header("Sounds")]

        public AudioClip shootingClip;
        public AudioClip musicBase;
        public AudioClip musicLow;
        public float musicVolumeBase;
        public float musicVolumeLow;

        [Header("Other")]

        [Tooltip("The game manager object.")]
        public GameManager gameManager;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        public static PlayerManager LocalPlayerManager;

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


        public GameObject healthUI;

        private AudioSource audioSourceMusicBase;
        private AudioSource audioSourceMusicLow;

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
                LocalPlayerManager = this;
                gameObject.transform.Find("Character").gameObject.transform.Find("hat").gameObject.SetActive(false);
                gameObject.transform.Find("Character").gameObject.transform.Find("head").gameObject.SetActive(false);
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
            try {healthUI = GameObject.FindWithTag("HealthUI");}
            catch { Debug.LogError("Health UI not found", this); }

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

            PlayerPainballDamageDDAA.Instance.SetPainballDamageListener(
                new OnValueChangeListener(
                    (newValue) =>
                    {
                        paintballDamage = newValue;
                    }    
                )
            );

            if (photonView.IsMine)
            {
                audioSourceMusicBase = gameObject.AddComponent<AudioSource>() as AudioSource;
                audioSourceMusicLow = gameObject.AddComponent<AudioSource>() as AudioSource;
                SetBackgroundMusic();
                audioSourceMusicBase.volume = musicVolumeBase;
                audioSourceMusicLow.volume = 0;
                audioSourceMusicBase.loop = true;
                audioSourceMusicLow.loop = true;
                audioSourceMusicBase.Play();
                audioSourceMusicLow.Play();
            }
            

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
            UpdatePlayerHealthUI();
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
         
        #endregion

        #region Photon events
        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            
            if (eventCode == GameManager.respawnEvent || eventCode == GameManager.levelStartReset)
            {
                if (eventCode == GameManager.levelStartReset)
                {
                    transform.position = (Vector3)photonEvent.CustomData;
                    SetMouseLock(true);
                }
                if (eventCode == GameManager.respawnEvent)
                    transform.position = respawnTransform.position;

                isPlayerInKeyLocZone = false;
                StartCoroutine(SetPlayerOutsideKeyLocationZone());
                if (photonView.IsMine)
                    Respawn();
            }
                
            if (photonView.IsMine) 
            {
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
            Debug.Log("Respawning local player. fpsController enabled = " + fpsController.enabled);
            GetComponentInChildren<ApplyPostProcessing>().vignetteLayer.intensity.value = 0;
            fpsController.enabled = false;   //We disable the script so that we can teleport the player
            GetComponent<FirstPersonController>().isPlayerInKeyLocZone = false;
            ChangeBackgroundMusic();
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
            PhotonView receivedPhotonView = PhotonView.Find(targetViewID);
            PlayerManager player = receivedPhotonView.gameObject.GetComponent<PlayerManager>();
            player.health = Mathf.Clamp(player.health + value, 0f, startingHealth);
            if (value < 0)
            {
                player.totalDamageReceived += value;
                if (receivedPhotonView.IsMine)
                {
                    DamageReceivedCondition.Instance.localPlayerTotalDamageReceived += value;
                    //Debug.Log("We were damaged! Local player total damage received: " + DamageReceivedCondition.Instance.localPlayerTotalDamageReceived);
                }
                //else Debug.Log("Someone was damaged! Player's total damage received: " + player.totalDamageReceived);
            }
        }

        public void UpdatePlayerHealthUI()
        {
            if (photonView.IsMine && SceneManager.GetActiveScene().name == "Master")
            {
                if (healthUI == null)
                    healthUI = GameObject.FindWithTag("HealthUI");
                healthUI.GetComponent<Text>().text = (int)health + "%";
            }
        }

        [PunRPC]
        public void Stunned(int targetViewID)
        {
            PhotonView receivedPhotonView = PhotonView.Find(targetViewID);
            PlayerManager player = receivedPhotonView.gameObject.GetComponent<PlayerManager>();
            player.stunCount++;
            if (receivedPhotonView.IsMine)
            {
                StunCondition.Instance.localPlayerStuntCount++;
                //Debug.Log("We were stunned! Local player stun count is " + StunCondition.Instance.localPlayerStuntCount);
            }
            //else Debug.Log("Someone is stunned! Player's stun count is " + player.stunCount);
        }

        //Function to call when an enemy is hit. 
        // enemy - the enemy we hit
        // healthChange - the effect on the enemies health (negative values for hurting)
        public void HitEnemy(GameObject enemy, float healthChange)
        {
            photonView.RPC(nameof(ChangeEnemyHealth), RpcTarget.All, healthChange, GetComponent<PhotonView>().ViewID, enemy.GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        public void ChangeEnemyHealth(float value, int playerViewID, int targetViewID)
        {
            PhotonView enemyPhotonView = PhotonView.Find(targetViewID);
            EnemyController enemy = enemyPhotonView.gameObject.GetComponent<EnemyController>();
            enemy.currentHealth += value;
            enemy.OnDamageTaken();
            if (enemy.currentHealth <= 0)
            {
                PhotonView playerPhotonView = PhotonView.Find(playerViewID);
                PlayerManager player = playerPhotonView.gameObject.GetComponent<PlayerManager>();
                player.defeatedEnemiesCount++;
                if (playerPhotonView.IsMine)
                {
                    DefeatedEnemiesCountCondition.Instance.localPlayerDefeatsCount++;
                    Debug.Log("We defeated enemy! Local player defeated enemy count is " + DefeatedEnemiesCountCondition.Instance.localPlayerDefeatsCount);
                }
                else Debug.Log("Someone defeated enemy! Player's defeated enemy count is " + player.defeatedEnemiesCount);
            }
        }

        [PunRPC]
        public void AnimateShoot()
        {
            animator.Play("Shoot");
            animatorHands.Play("Shoot");
        }

        public bool IsPlayerLocal()
        {
            return photonView.IsMine;
        }

        private void PlayShootingSound()
        {
            GetComponent<AudioSource>().PlayOneShot(shootingClip);
        }

        private void SetBackgroundMusic()
        {
            audioSourceMusicLow.clip = musicLow;
            audioSourceMusicBase.clip = musicBase;
        }

        public void ChangeBackgroundMusic()
        {
            if (photonView.IsMine)
            {
                if (isPlayerInKeyLocZone)
                {
                    audioSourceMusicBase.volume = 0;
                    audioSourceMusicLow.volume = musicVolumeLow;
                }
                else
                {
                    audioSourceMusicBase.volume = musicVolumeBase;
                    audioSourceMusicLow.volume = 0;
                }
            }
                
        }

        public void OnLevelFinished()
        {
            HealingRateDDAA.Instance.AdjustInGameValue();
        }

        public string GetPlayerDebugInfo()
        {
            string debugPrintContent = "----Player info----\n";
            if(photonView.IsMine){ debugPrintContent = debugPrintContent + "Your local player\n";  }
            else { debugPrintContent += "Other player\n"; }
            debugPrintContent += "Player's stun count: " + stunCount + "\n";
            debugPrintContent += "Player's received damage: " + totalDamageReceived + "\n";
            debugPrintContent += "In key location: " + isPlayerInKeyLocZone + "\n";
            debugPrintContent += "----------------";
            return debugPrintContent;
        }

        public void SetMouseLock(bool shouldLock)
        {
            photonView.RPC(nameof(LockMouse), RpcTarget.All, shouldLock, GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        private void LockMouse(bool shouldLock, int targetViewID)
        {
            PhotonView.Find(targetViewID).gameObject.GetComponent<FirstPersonController>().SetMouseLock(shouldLock);
        }

        public IEnumerator ResetPlayerLocation(Vector3 position, float delaySec)
        {
            yield return new WaitForSeconds(delaySec);
            photonView.RPC(nameof(ResetPlayerLocationAtStart), RpcTarget.All, position, GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        private void ResetPlayerLocationAtStart(Vector3 position, int targetViewID)
        {
            PhotonView.Find(targetViewID).gameObject.transform.position = position;
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
                animatorHands.SetBool("isMovingForward", true);
                animatorHands.SetBool("isMovingBackward", false);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", true);
                animatorHands.SetBool("isMovingForward", true);
                animatorHands.SetBool("isMovingBackward", false);
            }
            else
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
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
            Debug.Log("Return local player's control. fpsController enabled = " + fpsController.enabled);
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