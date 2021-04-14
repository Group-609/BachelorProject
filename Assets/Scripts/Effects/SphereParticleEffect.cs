using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereParticleEffect : MonoBehaviour
{
    private float gravity = 9.8f;
    private Vector3 direction;

    public bool keyLocationClearSphere = false;

    public float upMinValue = 0f;
    public float upMaxValue = 3f;
    public float maxMultiplier = 10f;

    public float lifetime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        if (keyLocationClearSphere)
        {
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, .33f)));
        } 

        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(upMinValue, upMaxValue), Random.Range(-1f, 1f)) * Random.Range(1f, maxMultiplier);

        StartCoroutine(DestroyThis());
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += direction * Time.deltaTime;

        direction.y -= gravity * Time.deltaTime;
    }

    private IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(lifetime);

        Destroy(gameObject);
    }
}
