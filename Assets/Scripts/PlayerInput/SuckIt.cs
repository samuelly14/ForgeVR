using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckIt : MonoBehaviour
{
    //Take out your suck it and you-
    public enum hand
    {
        Left = 0,
        Right = 1
    }

    private string axis;
    private bool release = true ;
    private Collider col;

    public float distance;
    public float speed = 1;
    public hand Hand = hand.Left;
    public GameObject indicator; //This GameObject should have a mesh as well as a linerenderer component
    private void Awake()
    {
        axis = Hand == hand.Left ? "TriggerLeft" : "TriggerRight";
        col = GetComponent<Collider>();
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        if (Input.GetAxis(axis) > 0.3)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && release)
            {
                indicator.SetActive(true);
                indicator.transform.position = hit.point;
                indicator.transform.LookAt(hit.normal);

                if (hit.transform.gameObject.CompareTag("Movable") || hit.transform.gameObject.CompareTag("Tool") || hit.transform.gameObject.CompareTag("Weapon"))
                {
                    //Set indicator to green
                    //start coroutine
                    release = false;
                    StartCoroutine("PullTowards", hit.rigidbody);
                }
            }
            else
            {
                //Display indicator as red
                indicator.transform.position = transform.forward * distance;
            }
        }
        if (Input.GetAxis(axis) < 0.2 && indicator.activeSelf)
        {
            indicator.SetActive(false);
            release = true;
        }
    }

    IEnumerator PullTowards(Rigidbody target)
    {
        target.isKinematic = true;
        float start = Time.time;
        while(!release)
        {
            if (target != null)
            {
                Vector3 dest = col.ClosestPoint(transform.position + transform.forward);
                target.MovePosition(Vector3.MoveTowards(target.position, dest, speed));
            }
            else release = true;
            yield return null;
        }
        if (target == null)
        {
            //Do nothing
        }
        else if (target.gameObject.CompareTag("Tool") || target.gameObject.CompareTag("Weapon") && target.transform != target.transform.root)
        { 
            target.isKinematic = true;
        }
        else
        {
            target.isKinematic = false;
        }
    }
}
