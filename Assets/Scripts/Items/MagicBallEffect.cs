using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBallEffect : MonoBehaviour
{

    private float displacementAmount;
    private MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        displacementAmount = Mathf.Lerp(displacementAmount, 0, Time.deltaTime);
        meshRenderer.material.SetFloat("_Amount", displacementAmount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        displacementAmount += 0.5f;
    }
}
