using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    public float Speed;
    public float TurnIntensity;
    public float MoveCooldown;

    private Camera cam;
    private Transform cameraTransform;
    private bool stepReset;
    private bool strafeReset;
    private bool turnReset;
    private bool duckReset;
    private bool ducking;

    // Use this for initialization
    void Start() {
        cam = GetComponentInChildren<Camera>();
        cameraTransform = cam.transform;
        stepReset = true;
        strafeReset = true;
        turnReset = true;
        duckReset = true;

        ducking = false;
    }

    // Update is called once per frame
    void Update() {
       

        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.60 && stepReset)
        {
            SnapForwardBack(Input.GetAxis("Vertical"));
            stepReset = false;
        }
        if (Mathf.Abs(Input.GetAxis("Vertical")) <= 0.2)
        {
            stepReset = true;
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.60 && strafeReset)
        {
            SnapLeftRight(Input.GetAxis("Horizontal"));
            strafeReset = false;
        }
        if (Mathf.Abs(Input.GetAxis("Horizontal")) <= 0.2)
        {
            strafeReset = true;
        }
        if (Mathf.Abs(Input.GetAxis("Turn")) >= 0.60 && turnReset)
        {
            SnapTurn(Input.GetAxis("Turn"));
            turnReset = false;
        }
        if (Mathf.Abs(Input.GetAxis("Turn")) <= 0.2)
        {
            turnReset = true;
        }
        if (Input.GetAxis("Duck") > 0.6 && duckReset)
        {
            Duck();
            duckReset = false;
        }
        if (Input.GetAxis("Duck") <= 0.2)
        {
            duckReset = true;   
        }

    }

    private void Duck()
    {
        if (ducking)
        {
            this.transform.position = new Vector3( this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z);
            ducking = false;
        }
        else
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.5f, this.transform.position.z);
            ducking = true;
        }
    }
      
    private void SnapForwardBack(float dir)
    {
        if (dir > 0)
        {
            //Deprecated, using teleportation instead for forward movement
        }
        else
        {
            Vector3 MoveIncrement = Vector3.Scale(-(cameraTransform.forward), new Vector3(1, 0, 1));
            gameObject.transform.position = gameObject.transform.position + (MoveIncrement * Speed);
        }
    }

    private void SnapLeftRight(float dir)
    {
        if (dir > 0)
        {
            Vector3 MoveIncrement = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
            gameObject.transform.position = gameObject.transform.position + (MoveIncrement * Speed);
        }
        else
        {
            Vector3 MoveIncrement = Vector3.Scale(-cameraTransform.right, new Vector3(1, 0, 1));
            gameObject.transform.position = gameObject.transform.position + (MoveIncrement * Speed);
        }
    }

    private void SnapTurn(float dir)
    {
        if (dir > 0)
        {
            gameObject.transform.RotateAround(cameraTransform.position,Vector3.up, 25);
        }
        else gameObject.transform.RotateAround(cameraTransform.position, Vector3.up, -25);
    }

}
