using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolishingBlock : MonoBehaviour
{
    public GameObject Smoke;
    public WeaponDatabase database;

    private GameObject currentTarget = null;
    private bool polishing = false;
    private bool reset = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<ToolProperties>().isHeld)
        {
            if (collision.gameObject.GetComponent<IngotProperties>() != null)
            {
                if (currentTarget == null)//set the current target to the collider.gameobject, and start the polishing coroutine. 
                {
                    currentTarget = collision.gameObject;
                    polishing = true;
                    StartCoroutine("Polishing");
                }
                else if (currentTarget == collision.gameObject) //Stop the reset sequence
                {
                    reset = false;
                }
                else //it's a diff. ingot, stop current coroutine and reset the target. 
                {
                    polishing = false;
                    currentTarget = null;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == currentTarget)
        {
            reset = true;
        }
    }

    IEnumerator Polishing()
    {
        Debug.Log("Polishing");
        float counter = 0;
        Vector3 lastPos = transform.position;
        Vector3 lastAng = transform.eulerAngles;
        while (counter < 5)
        {
            if(detectKinematicMovement(lastPos, lastAng) && reset == false)
            { 
                counter += Time.deltaTime;
                Smoke.transform.position = transform.position;
                Smoke.GetComponent<ParticleSystem>().Play();

            }

            if (polishing == false) yield break;
            lastPos = transform.position;
            lastAng = transform.eulerAngles;
            yield return null;
        }
        Debug.Log("Instantiated!!");
        GameObject blade = database.Craft(currentTarget.GetComponent<IngotProperties>());
        if (blade != null)
        {
            Instantiate(blade, currentTarget.transform.position, Quaternion.identity);
            Destroy(currentTarget);
        }
        currentTarget = null;
    }

    IEnumerator Reset()
    {
        Debug.Log("Resetting");
        float counter = 0;
        while(counter < 5)
        {
            counter += Time.deltaTime;

            if (reset == false) yield break;

            yield return null;
        }
        polishing = false;
    }

    private bool detectKinematicMovement(Vector3 pos, Vector3 ang)
    {
        Debug.Log((transform.position - pos).magnitude);
        if ((transform.position - pos).magnitude < 0.001) return false;
        return true;
    }
}
