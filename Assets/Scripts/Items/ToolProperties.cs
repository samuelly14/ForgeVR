using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolProperties : MonoBehaviour {
    public Vector3 holdPosition;
    public Vector3 holdAngle;
    public Vector3 defaultLocation;
    public bool isHeld;

    private bool replacing = false;

    private void Update()
    {
        if (transform.root == transform && !replacing && (transform.position - defaultLocation).sqrMagnitude > 2)
        {
            StartCoroutine("Replace");
        }
    }

    IEnumerator Replace()
    {
        replacing = true;
        float startTime = Time.time;
        bool stop = false;
        while (Time.time - startTime < 5)
        {
            if (transform.root != transform)
            {
                stop = true;
            }
            yield return null;
        }
        if (!stop)
        {
            transform.position = defaultLocation;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        replacing = false;
    }

}
