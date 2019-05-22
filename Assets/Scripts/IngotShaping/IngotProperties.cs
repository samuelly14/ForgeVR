using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotProperties : MonoBehaviour {

    public int hardness;
    public int toughness;

    public WeaponType type;

    public float min;
    public float mid;
    public float max;

    private int minAxis;
    private int midAxis;
    private int maxAxis;


    public int blades = 0;
    public bool pointed = false;
    public int threshold = 9;
    public Metal material;

    public Dictionary<Vector2Int, int> Sharpness = new Dictionary<Vector2Int, int>();
    public Dictionary<int, int> Pointedness = new Dictionary<int, int>();

    public enum WeaponType
    {
        broadsword,
        longsword, // 0 - 0.05, 0.1 - 0.3, 1.3 - 2  
        rapier,
        smallsword,
        
        shortsword, // 0 - 0.05, 0.1 - 0.3, 0.5 - 1.3
        gladius,
        dirk,

        greatsword, // 0 - 0.05, 0.3 - 0.7, 2+
        bastardsword,
        claymore,
        zweihander,

        dagger, // 0 - 0.05, 0.1 - 0.3, 0.3 - 0.5
        cinquedea,
        undefinedsword,

        handaxe, // 0 - 0.05, |y - z| < 0.3, z < 0.75
        greataxe, //0 - 0.05, |y - z| < 0.3, 0.75 < z < 2
        undefinedaxe,


        hammer, // |x - y| < 0.1, 1.5 < z < 2.5 * min(x, y), 0.5 < min(x, y), z < 0.7
        warhammer, // |x - y| < 0.1, 1.5 < z < 2.5 * min(x, y), 0.5 < min(x, y), 0.7 < z

        ingot,
    }
    public enum Metal
    {
        copper,
        bronze,
        iron,
        lowGradeSteel,
        highGradeSteel,
        Graphene,
        silver,
        gold, 
    }

    private IngotShape shapeInfo;

	// Use this for initialization
	void Start () {
        shapeInfo = GetComponent<IngotShape>();
        type = WeaponType.ingot;
	}

    private void OnCollisionEnter(Collision collision)
    {
        UpdateWeaponType();
    }
    private void OnCollisionExit(Collision collision)
    {
        UpdateWeaponType();
    }

    private void OnTriggerEnter(Collider other)
    {
        UpdateWeaponType();
    }

    private void OnTriggerExit(Collider other)
    {
        UpdateWeaponType();
    }

    public void UpdateDimensions()
    {
        minAxis = shapeInfo.GetMin(shapeInfo.dimensions);
        midAxis = shapeInfo.GetMid(shapeInfo.dimensions);
        if (minAxis + midAxis == 1) maxAxis = 2;
        else if (minAxis + midAxis == 2) maxAxis = 1;
        else maxAxis = 0;

        min = shapeInfo.dimensions[minAxis];
        mid = shapeInfo.dimensions[midAxis];
        max = shapeInfo.dimensions[maxAxis];
    }

    public void UpdateSharpness(int face, int side)
    {
        int val;
        Vector2Int key = new Vector2Int(face, side);
        if (Sharpness.TryGetValue(key, out val))
        {
            Sharpness[key] += 1;
        }
        else
        {
            Sharpness.Add(key, 1);
        }
        UpdateProperties();
    }
    public void UpdatePointedness(int face)
    {
        int val;
        if (Pointedness.TryGetValue(face, out val))
        {
            Pointedness[face] += 1;
        }
        else
        {
            Pointedness.Add(face, 1);
        }

        UpdateProperties();
    }

    public void UpdateWeaponType()
    {
        UpdateDimensions();
        if (min <= 0.075) //Bladed
        {
            if ( ((max - mid)/max) < 0.25) //Axe/Greataxe
            {
                if (max > 0.75 && max < 2)
                {
                    type = WeaponType.greataxe;
                }
                else if (max < 0.75 && max > 0.1)
                {
                    type = WeaponType.handaxe;
                }
                else
                {
                    type = WeaponType.undefinedaxe;
                }
            }

            else
            {
                type = ClassifySword();
            } //Classify Sword

        }
        else //Hammer or Spearhead
        {
            // hammer |x - y| < 0.1, 1.5 < z < 2.5 * min(x, y), 0.5 < min(x, y), z < 0.7
            // warhammer |x - y| < 0.1, 1.5 < z < 2.5 * min(x, y), 0.5 < min(x, y), 0.7 < z
            if (mid - min < 0.1 && max < (2.5 * mid) && max > (1.5 * min) && min > 0.2 && max < 0.7)
            {
                type = WeaponType.hammer;
            }
            else if (mid - min < 0.2 && max < (2.5 * mid) && max > (1.5 * min) && min > 0.2 && max < 1.3)
            {
                type = WeaponType.warhammer;
            }
            else type = WeaponType.ingot;
        }
    }

    private WeaponType ClassifySword()
    {
        //Switch on length
        int length;
        if (max > 2.35) length = 4;
        else if (max > 1.45) length = 3;
        else if (max > 0.5) length = 2;
        else if (max > 0.2) length = 1;
        else length = 0;

        int width;
        if (mid > 0.35 && mid < 0.65) width = 4;
        else if (mid > 0.21) width = 3;
        else if (mid > 0.12) width = 2;
        else if (mid > 0.08) width = 1;
        else width = 0;

        switch (length)
        {
            case 1: 
                switch (width)
                {
                    case 1: return WeaponType.dagger;
                    case 2: return WeaponType.cinquedea;
                }
                break;
            case 2:
                switch (width)
                {
                    case 1: return WeaponType.dirk;
                    case 2: return WeaponType.shortsword;
                    case 3: return WeaponType.gladius;
                }
                break;
            case 3:
                switch (width)
                {
                    case 1: return WeaponType.smallsword;
                    case 2: return WeaponType.rapier;
                    case 3: return WeaponType.longsword;
                    case 4: return WeaponType.broadsword;

                }
                break;
            case 4:
                switch (width)
                {
                    case 2: return WeaponType.claymore;
                    case 3: return WeaponType.bastardsword;
                    case 4: return WeaponType.greatsword;
                }
                break;

        }
        return WeaponType.undefinedsword;
    }

    private void UpdateProperties()
    {
        blades = 0;
        foreach (KeyValuePair<Vector2Int, int> kvp in Sharpness)
        {
            if  (kvp.Key.y == 0)
            {
                int temp;
                if (kvp.Value > threshold && Sharpness.TryGetValue(new Vector2Int(kvp.Key.x, 1), out temp))
                {
                    if (temp > threshold)
                    {
                        blades += 1;
                    }
                }
            }
        }
        foreach (int val in Pointedness.Values)
        {
            if (val > threshold)
            {
                pointed = true;
            }
        }
    }


}
