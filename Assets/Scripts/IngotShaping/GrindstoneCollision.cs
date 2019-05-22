using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrindstoneCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<IngotShape>() != null) gameObject.GetComponent<Rigidbody>().AddTorque(-transform.forward * 10);
    }
}
