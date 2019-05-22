using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HoldWeapon : MonoBehaviour
{

    public GameObject weapon;
    public GameObject target;
    private bool pressed = false;

    private string axis;

    public hand Hand = hand.Right;
    public enum hand
    {
        Left = 0,
        Right = 1
    }
    private void Awake()
    {
        axis = Hand == hand.Right ? "GrabRight" : "GrabLeft";
    }
    // Use this for initialization
    void Start()
    {
        weapon = null;
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
                if (target != null && weapon == null)
                {
                    Hold();
                }
                else if (weapon != null)
                {
                    Drop();
                }
            }
        }
    }
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Weapon")) target = collision.transform.root.gameObject;
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Weapon"))
        {
            target = null;
        }
    }

    private void Hold()
    {
        if (target != null && target.CompareTag("Weapon"))
        {        //Make it a child, zero out the local position
            target.transform.SetParent(transform);
            target.transform.localPosition = new Vector3(0, 0, 0);
            target.transform.localEulerAngles = target.GetComponent<WeaponProperties>().holdAngle;
            target.GetComponent<Rigidbody>().isKinematic = true;
            weapon = target;
        }
        else Debug.Log("Can't hold!");
    }
    private void Drop()
    {
        if (weapon != null)
        {
            weapon.transform.parent = null;
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon = null;
        }
    }
}
