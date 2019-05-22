using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quenching : MonoBehaviour {

    public float quality; //Quality should mark how well this liquid tempers the metal. 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Temperature data = other.GetComponent<Temperature>();
        if (data != null)
        {
            data.temperature = 250;
            data.StartCoroutine("ColorChange", Color.black);
            //THROW UP SOME STEAM IN THERE
        }
    }



}
