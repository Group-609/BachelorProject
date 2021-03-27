using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable
{
    private List<GameObject> players;
    [System.NonSerialized]
    public Transform player;
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
    public float minDistForMeleeAttack = 2;
    [Tooltip("Stopping distance should be lower than minimum distance for melee")]
    public float stoppingDistance = 2.5f;
    public int minDistForMovement = 110;
    //-------------------------------------
    [System.NonSerialized]
    public float currentHealth = 50f;      //current player health

    [Header("Other variables")]
    [Tooltip("Prefab of projectile to shoot")]
    [SerializeField]
    private GameObject projectilePrefab;

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


    //Used for estimating where player will be when projectile hits
    private Vector3 previousFramePlayerPosition;
    private Vector3 playerVelocity = new Vector3(0,0,0);

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.Play("Walk");     //Walking animation
        agent.stoppingDistance = stoppingDistance;
        players = findPlayers();

        maxHealthColor = new Color(.19f, .1f, .2f); //Dark purple
        lowHealthColor = new Color(.95f, .73f, 1f); //bright pink
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentHealth < 0)
            {
                photonView.RPC("Blobify", RpcTarget.All);
                //  //TODO: Replace with running away logic. Only destroy when the exit point(fountain of color) is reached.
            }
            if(isBlobified && distanceToKeyLocationToDespawn > Vector3.Distance(GetNearestKeyLocation().position, transform.position))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        if (!isBlobified)
        {
            FindNavTarget();
            distanceToPlayer = Vector3.Distance(player.position, transform.position);
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
        agent.destination = GetNearestKeyLocation().position;
        SetSpeed(speed);
        //TODO?: set color to nice pink
    }

    Transform GetNearestKeyLocation()
    {
        GameObject[] keyLocations = GameObject.FindGameObjectsWithTag("KeyLocation");
        GameObject closestKeyLocation = keyLocations[0];
        foreach(GameObject keyLocation in keyLocations)
        {
            if (Vector3.Distance(closestKeyLocation.transform.position, transform.position) < Vector3.Distance(keyLocation.transform.position, transform.position)) 
            {
                closestKeyLocation = keyLocation;
            }
        }
        return closestKeyLocation.transform;
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            playerVelocity = (player.position - previousFramePlayerPosition) / Time.fixedDeltaTime;
            previousFramePlayerPosition = player.position;
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
            Transform closestPlayer = null;
            List<GameObject> alivePlayers = players.FindAll(
               delegate (GameObject player)
               {
                   return player.GetComponent<PlayerManager>().health > 0;
               }
            );
            if (alivePlayers.Count != 0)  //If we found alive players, find the closest player
            {
                closestPlayer = alivePlayers[0].transform;
                foreach (GameObject player in alivePlayers)
                {
                    if (Vector3.Distance(player.transform.position, transform.position) < Vector3.Distance(closestPlayer.position, transform.position)) //We can add in DDA here by multiplying the distances based on the player with a multiplier
                    {
                        closestPlayer = player.transform;
                    }
                }
            }
            player = closestPlayer;
            agent.destination = closestPlayer.position;
            refreshTargetTimer = refreshTargetTimerLimit;
        }
    }

    List<GameObject> findPlayers()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
    }

    public void OnDamageTaken()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.Lerp(lowHealthColor, maxHealthColor, currentHealth / maxHealth);
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
                //player.GetComponent<HurtEffect>().Hit();
                //TODO: play player melee hit sound
                if (PhotonNetwork.IsMasterClient)
                { 
                    HitPlayer(player.gameObject, -meleeDamage);
                }
            }
            else if(distanceToPlayer <= shootingDistance)   //if player too far, shoot instead
            {
                GameObject projectile;
                projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                projectile.GetComponent<EnemyProjectile>().enemyWhoShot = this.gameObject;
                projectile.GetComponent<EnemyProjectile>().damage = this.projectileDamage;
                projectile.GetComponent<EnemyProjectile>().target = player;
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
