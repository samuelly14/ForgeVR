using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {
    private bool scanning = false;
    private TeleportIndicator tp;

	// Use this for initialization
	void Start () {
        tp = GetComponent<TeleportIndicator>();
	}

    // Update is called once per frame
    void Update () {
        if (Input.GetAxis("Vertical") > 0.5)
        {
            tp.ToggleDisplay(true);
            scanning = true;
        }
        if(Input.GetAxis("Vertical") < 0.5 && scanning)
        {
            tp.Teleport();
            tp.ToggleDisplay(false);
            scanning = false;
        }


    }
}
