using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjects : MonoBehaviour
{
    public GameObject holding = null;
    private bool moving = false;
    public GameObject moveTarget = null;

    
    void Update()
    {
        
        if (((Input.GetAxis("GrabLeft") > 0.5 && gameObject.name == "LeftHand") || (Input.GetAxis("GrabRight") > 0.5 && gameObject.name == "RightHand")) && moveTarget != null && moveTarget.CompareTag("Movable") && !moving)
        {
            moving = true;
            StartCoroutine("MoveTarget", moveTarget);
        }
        if ((Input.GetAxis("GrabLeft") < 0.5 && gameObject.name == "LeftHand") || (Input.GetAxis("GrabRight") < 0.5 && gameObject.name == "RightHand"))
        {
            moving = false;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Movable")) moveTarget = FindParentWithTag(col.transform.root, "Movable", new Queue<Transform>());
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject == moveTarget) moveTarget = null;
    }

    IEnumerator MoveTarget(GameObject tg)
    {
        holding = tg;
        Rigidbody rb = tg.GetComponent<Rigidbody>();
        rb.useGravity = false;
        tg.transform.SetParent(gameObject.transform);

        while (moving)
        {
            if (tg == null || tg.transform.parent.gameObject != gameObject || moveTarget != tg) moving = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            yield return null;
        }
        if (tg != null && tg.transform.parent == transform)
        {
            tg.transform.parent = null;
            tg.GetComponent<Rigidbody>().useGravity = true;
        }
        holding = null;
    }

    private GameObject FindParentWithTag(Transform root, string tag, Queue<Transform> q)
    {
        //Breadth first search of an unbalanced n-tree
        if (root.CompareTag(tag))
        {
            return root.gameObject; 
        }
        else
        {
            foreach(Transform child in root)
            {
                q.Enqueue(child);
            }
        }
        if (q.Count == 0) return null;
        return FindParentWithTag(q.Dequeue(), tag, q);
    }
}
