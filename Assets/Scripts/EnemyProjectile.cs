using System.Collections;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Tooltip("The time it takes for the projectile to disappear")]
    private float despawnTime = 4.0f;

    [System.NonSerialized]
    public Transform target;

    [System.NonSerialized]
    public float damage; //Damage this specific paintball does

	[System.NonSerialized]
	public GameObject enemyWhoShot;

	[System.NonSerialized]
	public bool isLocal; //true if this is a real bullet that does damage

	public float h = 10;
	public bool debugPath;

	void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    void Update()
    {
        if(debugPath)
        {
            DrawPath();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
		//If the object is the player who shot
		if (collision.collider.gameObject.tag == "Enemy")
		{
			return;
		}
		else if (isLocal)
		{
			if (collision.collider.gameObject.tag == "Player")
			{
				enemyWhoShot.GetComponent<EnemyController>().HitPlayer(collision.collider.gameObject, -damage);
			}
		}
		Destroy(gameObject);
    }

	public void Launch()
	{
		//Physics.gravity = Vector3.up * gravity
		//ball.useGravity = true;
		GetComponent<Rigidbody>().velocity = CalculateLaunchData().initialVelocity;
	}

	LaunchData CalculateLaunchData()
	{
		float displacementY = target.position.y - transform.position.y;
		Vector3 displacementXZ = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
		float time = Mathf.Sqrt(-2 * h / Physics.gravity.y) + Mathf.Sqrt(2 * (displacementY - h) / Physics.gravity.y);
		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * Physics.gravity.y * h);
		Vector3 velocityXZ = displacementXZ / time;

		return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(Physics.gravity.y), time);
	}

	void DrawPath()
	{
		LaunchData launchData = CalculateLaunchData();
		Vector3 previousDrawPoint = transform.position;

		int resolution = 30;
		for (int i = 1; i <= resolution; i++)
		{
			float simulationTime = i / (float)resolution * launchData.timeToTarget;
			Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * Physics.gravity.y * simulationTime * simulationTime / 2f;
			Vector3 drawPoint = transform.position + displacement;
			Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
			previousDrawPoint = drawPoint;
		}
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
