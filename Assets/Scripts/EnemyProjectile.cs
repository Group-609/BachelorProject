using System.Collections;
using UnityEngine;
using System;

public class EnemyProjectile : MonoBehaviour
{
    [Tooltip("The time it takes for the projectile to disappear")]
    private float despawnTime = 4.0f;

    [NonSerialized]
    public Transform target;

	[NonSerialized]
	public float damage; //Damage this specific paintball does

	[NonSerialized]
	public GameObject enemyWhoShot;

	[NonSerialized]
	public bool isLocal; //true if this is a real bullet that does damage

	public float h = 10;
	public bool debugPath;

	void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    void OnCollisionEnter(Collision collision)
    {
		GameObject hitObject = collision.collider.gameObject;

		//If the object is the enemy, who shot
		if (hitObject.CompareTag("Enemy"))
		{
			return;
		}
		else if (isLocal && hitObject.CompareTag("Player"))
		{
			enemyWhoShot.GetComponent<EnemyController>().HitPlayer(hitObject, -damage);
		}
		//TODO: projectile hit sound
		Destroy(gameObject);
    }

	public void Launch(Vector3 playerVelocity)
	{
		GetComponent<Rigidbody>().velocity = CalculateLaunchData(playerVelocity).initialVelocity;
	}

	LaunchData CalculateLaunchData(Vector3 playerVelocity)
	{
		float displacementY = target.position.y - transform.position.y;
		float time = Mathf.Sqrt(-2 * h / Physics.gravity.y) + Mathf.Sqrt(2 * (displacementY - h) / Physics.gravity.y);
		Vector3 displacementXZ = new Vector3(target.position.x + playerVelocity.x * time - transform.position.x, 0, target.position.z + playerVelocity.z * time - transform.position.z);
		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * Physics.gravity.y * h);
		Vector3 velocityXZ = displacementXZ / time;

		return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(Physics.gravity.y), time);
	}

	struct LaunchData
	{
		public readonly Vector3 initialVelocity;
		public readonly float timeToTarget;

		public LaunchData(Vector3 initialVelocity, float timeToTarget)
		{
			this.initialVelocity = initialVelocity;
			this.timeToTarget = timeToTarget;
		}

	}
}
