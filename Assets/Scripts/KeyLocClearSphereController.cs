using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLocClearSphereController : MonoBehaviour
{
    private float gravity = 9.8f;
    private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, .33f)));
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 3f), Random.Range(-1f, 1f)) * Random.Range(1f, 10f);

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
        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }
}
