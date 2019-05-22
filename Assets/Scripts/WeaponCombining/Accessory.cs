using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accessory : MonoBehaviour
{
    public enum AccessoryType
    {
        Guard = 0,
        Pommel = 1,
    }

    public AccessoryType type;
}
