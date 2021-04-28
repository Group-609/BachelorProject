using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;
using System.Collections;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityStandardAssets.Characters.FirstPerson;

using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    [System.NonSerialized]
    public Transform closestPlayer;
    private float distanceToPlayer;
    private bool isAttackReady = true;
    private float attackAnimationDelay = 1.5f;
    

    [Header("DDA friendly variables - they might be changed by the DDAA")]
    public float speed = 3f;
    public float maxHealth = 50f;
    public float shootingDistance = 25f;
    public float minDistForMeleeAttack = 2.5f;
    [Tooltip("Stopping distance should be lower than minimum distance for melee")]
    public float stoppingDistance = 2f;
    public int minDistForMovement = 110;
    //-------------------------------------
    [System.NonSerialized]
    public float currentHealth = 50f;

    [Header("Sounds")]

    [NonSerialized] public float sfxVolume = 0.5f;
    public float soundDistance;
    public AudioClip spawningClip;
    public AudioClip movementClip;
    public AudioClip shrinkingClip;
    public AudioClip shootClip;
    public AudioClip hitClip;
    [SerializeField]
    private AudioClip[] hurtClip = new AudioClip[0];
    public float volumeHurt;
    public float volumeSpawn;
    public float volumeWalk;
    public float volumeHit;
    private AudioSource audioSource;
    private AudioSource audioSourceWalking;
    private AudioSource audioSourceHit;
    private AudioSource audioSourceHurt;
    

    [Header("Other variables")]
    [Tooltip("Prefab of projectile to shoot")]
    public GameObject projectilePrefab;

    private SkinnedMeshRenderer meshRenderer;
    private Color maxHealthColor;
    private Color lowHealthColor;

    private NavMeshAgent agent;
    private Animator animator;
    public int refreshTargetTimeSec = 1;
    [System.NonSerialized]
    public bool isBlobified = false;

    private float spawnSizeScale = 0;
    public float spawnSizeScaleSpeed = 1;

    private bool spawningFinished = false;

    [SerializeField]
    private float distanceToKeyLocationToDespawn = 1f;

    private GameObject assignedKeyLocation;

    [NonSerialized]
    public bool isAreaEnemy;

    //Used for estimating where player will be when projectile hits
    private Vector3 previousFramePlayerPosition;
    private Vector3 playerVelocity = new Vector3(0,0,0);

    //Here, we receive data sent during instantiation, photon networking specific
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        isAreaEnemy = (bool)instantiationData[0];
    }

    void Start()
    {
        UpdateVolumeLevel();
        audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        audioSourceWalking = gameObject.AddComponent<AudioSource>() as AudioSource;
        audioSourceHit = gameObject.AddComponent<AudioSource>() as AudioSource;
        audioSourceHurt = gameObject.AddComponent<AudioSource>() as AudioSource;
        SetInitialAudioClips();
        audioSource.volume = volumeSpawn * sfxVolume;
        audioSource.Play();

        assignedKeyLocation = gameObject.FindClosestObject("KeyLocation");
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.Play("Walk");     //Walking animation
        agent.stoppingDistance = stoppingDistance;

        try
        {
            meshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            maxHealthColor = meshRenderer.material.color; //initial color
            lowHealthColor = new Color(.95f, .73f, 1f); //bright pink
        }
        catch (Exception)
        {
            Debug.LogError("Could not find enemy's mesh renderer");
        }
        
        currentHealth = maxHealth;

        audioSourceWalking.loop = true;
        
        // we want to find nav target not every frame because it's computationally a bit heavy
        InvokeRepeating(nameof(FindNavTarget), 0, refreshTargetTimeSec);

        gameObject.transform.localScale = new Vector3(1, 1, 1) * spawnSizeScale;
    }

    void Update()
    {
        UpdateVolumeLevel();
        PlayWalkingSound();
        if (PhotonNetwork.IsMasterClient && currentHealth <= 0)
        {
            if (!isBlobified)
            {
                photonView.RPC(nameof(Blobify), RpcTarget.All);
            }
            
            if(isAreaEnemy && distanceToKeyLocationToDespawn > Vector3.Distance(assignedKeyLocation.transform.position, transform.position))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        if (!spawningFinished)
        {
            spawnSizeScale += spawnSizeScaleSpeed * Time.deltaTime;

            if (spawnSizeScale >= 1)
            {
                spawnSizeScale = 1;
                spawningFinished = true;
            }

            gameObject.transform.localScale = new Vector3(1, 1, 1) * spawnSizeScale;
        }

        if (!isBlobified)
        {
          
            if(closestPlayer == null)   //if no player is found, chill for a bit
            {
                SetSpeed(0);
            }
            else
            { 
                distanceToPlayer = Vector3.Distance(closestPlayer.position, transform.position);
                if (spawningFinished && distanceToPlayer <= minDistForMovement)
                {
                    SetSpeed(speed);
                    if (isAttackReady)
                    {
                        StartCoroutine(AttackPlayer());
                    }
                }
                else
                {
                    SetSpeed(0);
                }
            } 
        }
    }

    [PunRPC]
    void Blobify()
    {
        spawningFinished = true;
        animator.SetBool("IsDead", true);
        audioSource.PlayOneShot(shrinkingClip);
        isBlobified = true;
        agent.stoppingDistance = 0;
        if (isAreaEnemy)
            agent.destination = assignedKeyLocation.transform.position;
        else
        {
            Vector2 circleEdgePosition = Random.insideUnitCircle.normalized;
            agent.destination = transform.position + new Vector3(circleEdgePosition.x, 0, circleEdgePosition.y) * 30;
            StartCoroutine(DestroyEnemyWithDelay());
        }
        SetSpeed(speed);
        CancelInvoke(nameof(FindNavTarget));
        foreach (Collider collider in GetComponents<Collider>())
            collider.enabled = false;
        //TODO?: set color to nice pink
    }

    public void Die()
    {
        if(spawningFinished)
            currentHealth = 0;
    }

    private IEnumerator DestroyEnemyWithDelay()
    {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (closestPlayer != null)
        {
            playerVelocity = (closestPlayer.position - previousFramePlayerPosition) / Time.fixedDeltaTime;
            previousFramePlayerPosition = closestPlayer.position;
        }
    }

    void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    void UpdateVolumeLevel()
    {
        sfxVolume = PlayerManager.LocalPlayerInstance.GetComponent<FirstPersonController>().volume;
    }

    void PlayWalkingSound()
    {
        audioSourceWalking.volume = volumeWalk * sfxVolume;
        if (agent.velocity != Vector3.zero && !audioSourceWalking.isPlaying)
        {
            audioSourceWalking.Play();
        }
        if (agent.velocity == Vector3.zero)
        {
            audioSourceWalking.Pause();
        }
    }

    void SetInitialAudioClips()
    {
        audioSource.spatialBlend = 1;
        audioSourceWalking.spatialBlend = 1;
        audioSourceHit.spatialBlend = 1;
        audioSourceHurt.spatialBlend = 1;

        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSourceWalking.rolloffMode = AudioRolloffMode.Linear;
        audioSourceHit.rolloffMode = AudioRolloffMode.Linear;
        audioSourceHurt.rolloffMode = AudioRolloffMode.Linear;

        audioSource.maxDistance = soundDistance;
        audioSourceWalking.maxDistance = soundDistance;
        audioSourceHit.maxDistance = soundDistance;
        audioSourceHurt.maxDistance = soundDistance;

        audioSource.clip = spawningClip;
        audioSourceWalking.clip = movementClip;
        audioSourceHit.clip = hitClip;

    }

    void FindNavTarget()
    {
        List<GameObject> alivePlayers = GetPlayersToAttack();

        //If we found alive players, find the closest player, else make it null
        if (alivePlayers.Count != 0)
        {
            closestPlayer = gameObject.FindClosestObject(alivePlayers).transform;
            agent.destination = closestPlayer.position;
        }
        else
        {
            closestPlayer = null;
        }
    }

    private List<GameObject> GetPlayersToAttack()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Player")).FindAll(
            delegate (GameObject player)
            {
                if (isAreaEnemy)
                {
                    //Debug.Log("Player is in key loc zone: " + player.GetComponent<PlayerManager>().isPlayerInKeyLocZone);
                    return player.GetComponent<PlayerManager>().health > 0 && player.GetComponent<PlayerManager>().isPlayerInKeyLocZone;
                }
                else return player.GetComponent<PlayerManager>().health > 0;
            }
        );
    }

    public void OnDamageTaken()
    {
        Debug.Log("Enemy took damage. Current health: " + currentHealth);
        audioSourceHit.volume = volumeHit * sfxVolume;
        audioSourceHit.Play();
        HurtSound();
        if (meshRenderer != null)
            meshRenderer.material.color = Color.Lerp(lowHealthColor, maxHealthColor, currentHealth / maxHealth);
    }
    public void HurtSound()
    {
        int n = Random.Range(1, hurtClip.Length);
        audioSourceHurt.clip = hurtClip[n];
        audioSourceHurt.volume = volumeHurt * sfxVolume;
        audioSourceHurt.PlayOneShot(audioSourceHurt.clip);
        hurtClip[n] = hurtClip[0];
        hurtClip[0] = audioSourceHurt.clip;
    }
    IEnumerator AttackPlayer()
    {
        if (distanceToPlayer <= shootingDistance)
        {

            isAttackReady = false;
            animator.SetBool("IsAttacking", true);

            //Time damage effect delay to when attack happens
            yield return new WaitForSeconds(attackAnimationDelay);
            if (currentHealth > 0) //Check if enemy is still alive since the animation delay started
            {
                if (distanceToPlayer <= minDistForMeleeAttack)
                {
                    closestPlayer.GetComponent<HurtEffect>().Hit();
                    //TODO: play player melee hit sound
                    if (PhotonNetwork.IsMasterClient)
                    {
                        HitPlayer(closestPlayer.gameObject, true);
                    }
                }
                else if (distanceToPlayer <= shootingDistance)   //if player too far, shoot instead
                {
                    GameObject projectile;
                    projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    projectile.GetComponent<EnemyProjectile>().enemyWhoShot = this.gameObject;
                    projectile.GetComponent<EnemyProjectile>().target = closestPlayer;
                    projectile.GetComponent<EnemyProjectile>().isLocal = PhotonNetwork.IsMasterClient;
                    projectile.GetComponent<EnemyProjectile>().Launch(playerVelocity);

                    audioSource.volume = volumeSpawn * sfxVolume;
                    audioSource.PlayOneShot(shootClip);
                }
            }

            //Wait for attack animation to finish
            yield return new WaitForSeconds(attackAnimationDelay);
            animator.SetBool("IsAttacking", false);
            isAttackReady = true;
        }
        yield return null;
    }

    //Function to call when an enemy attacks player. 
    public void HitPlayer(GameObject player, bool isMeleeAttack)
    {
        if(player.GetComponent<PlayerManager>().health > 0)
        {
            player.GetComponent<PlayerManager>().HitPlayer(player, false, isMeleeAttack);
        }
    }

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.currentHealth);
        }
        else
        {
            // Network player, receive data
            this.currentHealth = (float)stream.ReceiveNext();
        }
    }

    #endregion

}
