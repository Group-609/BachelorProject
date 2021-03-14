using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject[] players;
    public Transform player;
    private float distanceToPlayer;
    public int minDist = 2;
    public int speed = 2;
    private bool isAttackReady = true;
    private float attackAnimationDelay = .5f;

    public float health = 50f;

    private NavMeshAgent agent;
    //public Animator animator;
    private int refreshTargetTimer = 0;
    public int refreshTargetTimerLimit = 50;

    void Start()
    {
        //animator = GetComponentInChildren<Animator>();    //Uncomment when adding animator
        agent = GetComponent<NavMeshAgent>();
        //animator.Play("AngryFlight");     //Walking animation
        agent.stoppingDistance = 2;
        players = findPlayers();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health < 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            FindNavTarget();
            distanceToPlayer = Vector3.Distance(player.position, transform.position);
            SetSpeed();
            Attack();
        }
    }

    void SetSpeed()
    {
        agent.speed = speed;
    }

    void FindNavTarget()
    {
        refreshTargetTimer -= 1;

        //we set destination for target to run less than every frame, cause it's computationally heavy over longer distances
        if (refreshTargetTimer <= 0)
        {
            Transform closestPlayer = players[0].transform;
            //find player closest to enemy
            for(int i = 0; i < players.Length; i++)
            {
                if(Vector3.Distance(players[i].transform.position, transform.position) < Vector3.Distance(closestPlayer.position, transform.position)) //We can add in DDA here by multiplying the distances based on the player with a multiplier
                {
                    closestPlayer = players[i].transform;
                }
            }
            player = closestPlayer;
            agent.destination = closestPlayer.position;
            refreshTargetTimer = refreshTargetTimerLimit;
        }
    }

    GameObject[] findPlayers()
    {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    void Attack()
    {
        if (isAttackReady && distanceToPlayer <= minDist)
        {
            isAttackReady = false; 

            //Set run attack animation here
            //animator.Play("Attack");

            StartCoroutine(TriggerDamageEffect());
        }
    }

    IEnumerator TriggerDamageEffect()
    {
        //Time damage effect delay to when attack happens
        yield return new WaitForSeconds(attackAnimationDelay);
        if (distanceToPlayer <= minDist)
        {
            //player.GetComponent<HurtEffect>().Hit();
            Debug.LogError("Player is attacked");
        }

        //Wait for attack animation to finish
        yield return new WaitForSeconds(attackAnimationDelay);
        isAttackReady = true;

        yield return null;
    }

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.health);
        }
        else
        {
            // Network player, receive data
            this.health = (float)stream.ReceiveNext();
        }
    }

    #endregion



}
