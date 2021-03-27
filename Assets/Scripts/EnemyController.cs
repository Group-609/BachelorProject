using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;
using System.Collections;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable
{
    private List<GameObject> players;
    [System.NonSerialized]
    public Transform closestPlayer;
    private float distanceToPlayer;
    private bool isAttackReady = true;
    private float attackAnimationDelay = 1.5f;

    [Header("DDA friendly variables - they might be changed by the DDAA")]
    //the default values here should be used if DDA is not applied
    public float meleeDamage = 90f;
    public float projectileDamage = 30f;
    public float speed = 3f;
    public float maxHealth = 50f;
    public float shootingDistance = 25f;
    public float minDistForMeleeAttack = 2.5f;
    [Tooltip("Stopping distance should be lower than minimum distance for melee")]
    public float stoppingDistance = 2f;
    public int minDistForMovement = 110;
    //-------------------------------------
    [System.NonSerialized]
    public float currentHealth = 50f;      //current player health

    [Header("Other variables")]
    [Tooltip("Prefab of projectile to shoot")]
    [SerializeField]
    private GameObject projectilePrefab;

    private SkinnedMeshRenderer meshRenderer;
    private Color maxHealthColor;
    private Color lowHealthColor;

    private NavMeshAgent agent;
    private Animator animator;
    private int refreshTargetTimer = 0;
    public int refreshTargetTimerLimit = 50;
    [System.NonSerialized]
    public bool isBlobified = false;

    [SerializeField]
    private float distanceToKeyLocationToDespawn = 1f;

    private List<GameObject> keyLocations;


    //Used for estimating where player will be when projectile hits
    private Vector3 previousFramePlayerPosition;
    private Vector3 playerVelocity = new Vector3(0,0,0);

    void Start()
    {
        keyLocations = new List<GameObject>(GameObject.FindGameObjectsWithTag("KeyLocation"));
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.Play("Walk");     //Walking animation
        agent.stoppingDistance = stoppingDistance;
        players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

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
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && currentHealth < 0)
        {
            if (!isBlobified)
            {
                photonView.RPC("Blobify", RpcTarget.All);
            }
            
            if(distanceToKeyLocationToDespawn > Vector3.Distance(gameObject.FindClosestObject(keyLocations).transform.position, transform.position))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        if (!isBlobified)
        {
            FindNavTarget();
            distanceToPlayer = Vector3.Distance(closestPlayer.position, transform.position);
            if (distanceToPlayer <= minDistForMovement)
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

    [PunRPC]
    void Blobify()
    {
        animator.SetBool("IsDead", true);
        isBlobified = true;
        agent.stoppingDistance = 0;
        agent.destination = gameObject.FindClosestObject(keyLocations).transform.position;
        SetSpeed(speed);
        //TODO?: set color to nice pink
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

    void FindNavTarget()
    {
        refreshTargetTimer -= 1;

        //we set destination for target to run less than every frame, cause it's computationally heavy over longer distances
        if (refreshTargetTimer <= 0)
        {
            List<GameObject> alivePlayers = GetAlivePlayers();

            //If we found alive players, find the closest player, else make it null
            if (alivePlayers.Count != 0)  
            {
                closestPlayer = gameObject.FindClosestObject(alivePlayers).transform;
                agent.destination = closestPlayer.position;
            }
            else closestPlayer = null;
            
            refreshTargetTimer = refreshTargetTimerLimit;
        }
    }

    private List<GameObject> GetAlivePlayers()
    {
        return players.FindAll(
                   delegate (GameObject player)
                   {
                       return player.GetComponent<PlayerManager>().health > 0;
                   }
                );
    }

    public void OnDamageTaken()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = Color.Lerp(lowHealthColor, maxHealthColor, currentHealth / maxHealth);
    }

    IEnumerator AttackPlayer()
    {

        //TODO: attack sound
        if (distanceToPlayer <= shootingDistance)
        {

            isAttackReady = false;
            animator.SetBool("IsAttacking", true);

            //Time damage effect delay to when attack happens
            yield return new WaitForSeconds(attackAnimationDelay);
            if (distanceToPlayer <= minDistForMeleeAttack)
            {
                closestPlayer.GetComponent<HurtEffect>().Hit();
                //TODO: play player melee hit sound
                if (PhotonNetwork.IsMasterClient)
                { 
                    HitPlayer(closestPlayer.gameObject, -meleeDamage);
                }
            }
            else if(distanceToPlayer <= shootingDistance)   //if player too far, shoot instead
            {
                GameObject projectile;
                projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                projectile.GetComponent<EnemyProjectile>().enemyWhoShot = this.gameObject;
                projectile.GetComponent<EnemyProjectile>().damage = this.projectileDamage;
                projectile.GetComponent<EnemyProjectile>().target = closestPlayer;
                projectile.GetComponent<EnemyProjectile>().isLocal = PhotonNetwork.IsMasterClient;
                projectile.GetComponent<EnemyProjectile>().Launch(playerVelocity);
            }

            //Wait for attack animation to finish
            yield return new WaitForSeconds(attackAnimationDelay);
            animator.SetBool("IsAttacking", false);
            isAttackReady = true;
        }
        yield return null;
    }

    //Function to call when an enemy attacks player. 
    // enemy - the enemy we hit
    // healthChange - the effect on the enemies health (negative values for hurting)
    public void HitPlayer(GameObject player, float healthChange)
    {
        if(player.GetComponent<PlayerManager>().health > 0)
        {
            photonView.RPC("ChangePlayerHealth", RpcTarget.All, healthChange, player.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    public void ChangePlayerHealth(float value, int targetViewID)
    {
        PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health += value;
        //PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().OnDamageTaken();   //TODO: Player hurt effect
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
