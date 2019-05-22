using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeSpawner : MonoBehaviour
{

    public GameObject obj;
    public Vector3 pos;

    private bool spawn = true;

    private void Start()
    {
        StartCoroutine("Spawn");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<IngotShape>() != null) spawn = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<IngotShape>() != null)
        {
            spawn = true;
            StartCoroutine("Spawn");
        }
    }

    IEnumerator Spawn()
    {
        float counter = 0f;
        while (counter < 10)
        {
            counter += Time.deltaTime;
            if (!spawn) yield break;
            yield return null;
        }
        Instantiate(obj, pos, Quaternion.identity);
    }
}
