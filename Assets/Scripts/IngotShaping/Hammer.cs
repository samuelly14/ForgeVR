using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour {

    private BoxCollider hitbox;
    private bool allowForging;
    private bool hitReset;

	// Use this for initialization
	void Start () {
        hitbox = GetComponent<BoxCollider>();
        allowForging = false;
	}
	
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "AnvilSpace")
        {
            allowForging = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "AnvilSpace")
        {
            allowForging = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<ToolProperties>().isHeld)
        {
            if (collision.gameObject.GetComponent<IngotProperties>() != null && allowForging && hitReset)
            {
                Vector3 norm = collision.contacts[0].normal;
                int axis = GetFace(norm, collision.gameObject.transform);
                if (axis == -1)
                {
                    return;
                }

                collision.gameObject.GetComponent<IngotShape>().Flatten(axis);
                hitReset = false;
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<IngotProperties>() != null) hitReset = true;
    }

    private int GetFace(Vector3 normal, Transform target)
    {

        if (Vector3.Scale(normal, normal) == Vector3.Scale(target.right, target.right))
        {
            return 0;
        }
        else if (Vector3.Scale(normal, normal) == Vector3.Scale(target.up, target.up))
        {
            return 1;
        }
        else if (Vector3.Scale(normal, normal) == Vector3.Scale(target.forward, target.forward))
        {
            return 2;
        }
        else
        {
            return -1;
        }
    }
}
