using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hilt : MonoBehaviour
{
    public Vector3 Bottom; //To place the pommel. (0,0,0) will always be the top of the hilt
    public Vector3 HoldPosition;

    //Some self explanatory bools
    private bool pommelAttached = false;
    private bool guardAttached = false;
    private bool bladeAttached = false;
    
    private void OnCollisionEnter(Collision collision)
    {
        //Check to see what type of object we collided with
        int type = CheckType(collision.gameObject);
        switch(type)
        {
            case 0: //It's a blade
                if (!bladeAttached && collision.transform.root == collision.transform)
                {
                    ProperParent(collision.gameObject, Vector3.zero);
                    bladeAttached = true;
                }
                break;
            case 1: //It's a handguard
                if (!guardAttached && collision.transform.root == collision.transform)
                {
                    ProperParent(collision.gameObject, Vector3.zero);
                    guardAttached = true;
                }
                break;
            case 2: //It's a pommel
                if (!pommelAttached && collision.transform.parent == collision.transform)
                {
                    ProperParent(collision.gameObject, Bottom);
                    pommelAttached = true;
                }
                break;
        }
    }

    private int CheckType(GameObject obj)
    {
        if (obj.GetComponent<Blade>() != null) return 0;
        else if (obj.GetComponent<Handguard>() != null) return 1;
        else if (obj.GetComponent<Pommel>() != null) return 2;
        else return -1;
    }

    private void ProperParent(GameObject child, Vector3 pos)
    {
        child.transform.SetParent(transform);
        child.transform.localEulerAngles = Vector3.zero;
        child.transform.localPosition = pos;
        Rigidbody rb = child.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
    }
}
