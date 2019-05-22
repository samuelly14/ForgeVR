using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : MonoBehaviour {

    public float temperature;
    public float heatIncrement;
    public float coolIncrement;

    public enum Heat
    {
        cold = 0,
        red = 1,
        orange = 2,
        yellow = 3,
        white = 4
    };

    public Heat currentHeat;

    private bool heating;
    private Renderer rend;
    private Color red = new Color(.65f, 0.0f, 0.0f);
    private Color orange = new Color(1.0f, 0.25f, 0.0f);
    private Color yellow = new Color(1.0f, 0.7f, 0.0f);
    private Color white = new Color(1.0f, 1.0f, 0.33f);

    private Color emissionColor;

    //This  will attach to every type of ingot. It will contain all the information needed for when the metal is tempered

	// Use this for initialization
	void Start () {
        temperature = 70;
        rend = gameObject.GetComponent<MeshRenderer>();
        emissionColor = Color.black;
	}
	
	// Update is called once per frame
	void Update () {

		
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Coals")
        {
            heating = true;
            StartCoroutine("HeatIngot");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Coals" +
            "")
        {
            heating = false;
            StartCoroutine("CoolIngot");
        }
    }




    private void TemperatureChange()
    {
        if (temperature < 600 && currentHeat != Heat.cold)
        {
            StartCoroutine("ColorChange", Color.black);
            currentHeat = Heat.cold;
        }
        else if (temperature >= 600 && temperature < 800 && currentHeat != Heat.red)
        {
            StartCoroutine("ColorChange", red);
            currentHeat = Heat.red;
        }
        else if (temperature >= 800 && temperature < 1000 && currentHeat != Heat.orange)
        {
            StartCoroutine("ColorChange", orange); 
            currentHeat = Heat.orange;
        }
        else if (temperature >= 1000 && temperature < 1200 && currentHeat != Heat.yellow)
        {
            StartCoroutine("ColorChange", yellow);
            currentHeat = Heat.yellow;
        }
        else if (temperature > 1200 && currentHeat != Heat.white)
        {
            StartCoroutine("ColorChange", white);
            currentHeat = Heat.white;
        }
    }

    IEnumerator CoolIngot()
    {
        while (!heating && temperature >= 70)
        {
            temperature = temperature - coolIncrement;
            TemperatureChange();
            yield return null;
        }
    }

    IEnumerator HeatIngot()
    {
        while (heating)
        {
            temperature = temperature + heatIncrement;
            TemperatureChange();
            yield return null;
        }
    }

    IEnumerator ColorChange(Color finish)
    {
        Color start = emissionColor;
        float timeStart = Time.time;
        while ((Time.time - timeStart) < 2.0f)
        {
            Color newColor = Color.Lerp(start, finish, (Time.time - timeStart) / 2.0f);
            rend.material.SetColor("_EmissionColor", newColor);
            yield return null;
        }
        emissionColor = finish;
    }
}
