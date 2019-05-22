using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HoldTool : MonoBehaviour {

    public GameObject tool;
    public GameObject target;
    public hand Hand = hand.Right;
    public enum hand
    {
        Left = 0,
        Right = 1
    }

    private bool pressed = false;
    private string axis;

    private void Awake()
    {
        axis = Hand == hand.Right ? "GrabRight" : "GrabLeft";
    }

    // Use this for initialization
    void Start () {
        tool = null;
        target = null;
    }

    private void Update()
    {
        if (Input.GetAxis(axis) > 0.5)
        {
            pressed = true;
        }

        if (Input.GetAxis(axis) < 0.5)
        {
            if (pressed)
            {
                pressed = false;
                if (target != null && tool == null)
                {
                    Hold();
                }
                else if (tool != null)
                {
                    Drop();
                }
            }
        }
    }
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Tool")) target = collision.transform.root.gameObject; 
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Tool"))
        {
            target = null;
        }
    }

    private void Hold()
    {
        if (target != null && target.CompareTag("Tool"))
        {        //Make it a child, zero out the local position
            target.transform.SetParent(transform);
            target.transform.localPosition = target.GetComponent<ToolProperties>().holdPosition; 
            target.transform.localEulerAngles = target.GetComponent<ToolProperties>().holdAngle;
            target.GetComponent<Rigidbody>().isKinematic = true;
            tool = target;

            tool.GetComponent<ToolProperties>().isHeld = true;
        }
        else Debug.Log("Can't hold!");
    }
    private void Drop()
    {
        if (tool != null)
        {
            tool.GetComponent<ToolProperties>().isHeld = false;
            tool.transform.parent = null;
            tool.GetComponent<Rigidbody>().isKinematic = false;
            tool = null;
        }
    }
}
