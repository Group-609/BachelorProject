using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject[] players;
    public Transform player;
    private float distanceToPlayer;
    public int minDistForMeleeAttack = 2;
    private float minDistForMovement = 110;

    private bool isAttackReady = true;
    private float attackAnimationDelay = 1.5f;

    //DDA friendly variables--------------
    public float attackDamage = 5f;
    public int speed = 2;
    public float maxHealth = 50f;
    public float health = 50f;
    //-------------------------------------

    public Color maxHealthCol;
    public Color lowHealthCol;

    private NavMeshAgent agent;
    public Animator animator;
    private int refreshTargetTimer = 0;
    public int refreshTargetTimerLimit = 50;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.Play("Walk_body");     //Walking animation
        agent.stoppingDistance = minDistForMeleeAttack;
        players = findPlayers();

        maxHealthCol = new Color(.19f, .1f, .2f); //Dark purple
        lowHealthCol = new Color(.95f, .73f, 1f); //bright pink
        health = maxHealth;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health < 0)
            {
                animator.SetBool("IsDead", true);
                PhotonNetwork.Destroy(gameObject);  //TODO: Replace with running away logic. Only destroy when the exit point(fountain of color) is reached.
            }
            FindNavTarget();
            distanceToPlayer = Vector3.Distance(player.position, transform.position);
            if (distanceToPlayer <= minDistForMovement)
            {
                SetSpeed(speed);
                Attack();
            } 
            else
            {
                SetSpeed(0);
            }
        }
    }

    void SetSpeed(int speed)
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
        if (isAttackReady && distanceToPlayer <= minDistForMeleeAttack)
        {
            isAttackReady = false; 
            animator.SetBool("IsAttacking", true);
            StartCoroutine(TriggerDamageEffect());
        }
    }

    public void OnDamageTaken()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.Lerp(lowHealthCol, maxHealthCol, health / maxHealth);
    }

    IEnumerator TriggerDamageEffect()
    {
        //Time damage effect delay to when attack happens
        yield return new WaitForSeconds(attackAnimationDelay);
        if (distanceToPlayer <= minDistForMeleeAttack)
        {
            //player.GetComponent<HurtEffect>().Hit();
            HitPlayer(player.gameObject, -attackDamage);
            Debug.LogError("Player is attacked");
        }

        //Wait for attack animation to finish
        yield return new WaitForSeconds(attackAnimationDelay);
        animator.SetBool("IsAttacking", false);
        isAttackReady = true;

        yield return null;
    }

    //Function to call when an enemy attacks player. 
    // enemy - the enemy we hit
    // healthChange - the effect on the enemies health (negative values for hurting)
    public void HitPlayer(GameObject player, float healthChange)
    {
        photonView.RPC("ChangePlayerHealth", RpcTarget.All, healthChange, player.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    public void ChangePlayerHealth(float value, int targetViewID)
    {
        PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().health += value;
        //PhotonView.Find(targetViewID).gameObject.GetComponent<PlayerManager>().OnDamageTaken();   //Player hurt effect
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
