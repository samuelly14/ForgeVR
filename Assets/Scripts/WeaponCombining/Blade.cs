using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
    [System.Serializable]
    public struct Constraint
    {
        public int blades;
        public bool isPointed;
        public IngotProperties.WeaponType type;
        public IngotProperties.Metal material;

        public Constraint(int b, bool p, IngotProperties.WeaponType t, IngotProperties.Metal m)
        {
            blades = b;
            isPointed = p;
            type = t;
            material = m;
        }
        public static bool operator ==(Constraint a, Constraint b)
        {
            return a.blades == b.blades && a.isPointed == b.isPointed && a.type == b.type && a.material == b.material;
        }
        public static bool operator !=(Constraint a, Constraint b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    };

    public Constraint constraint;

    public int power;
    public int toughness;
    public int hardness;

}
