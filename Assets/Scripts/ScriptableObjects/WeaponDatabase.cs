using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "Inventory/WeaponDatabase", order = 1)]
public class WeaponDatabase : ScriptableObject
{
    public List<GameObject> Swords;

    public List<GameObject> Axes;

    public List<GameObject> Hammers;

    public GameObject Craft(IngotProperties properties)
    {
        Blade.Constraint temp = new Blade.Constraint(properties.blades, properties.pointed, properties.type, properties.material);
        if ((int)properties.type <= (int)IngotProperties.WeaponType.undefinedsword) //it's a sword
        {
            //if it's a sword, then we check to see which sword it shoudl be. 
            return FindPrefab(Swords, temp);
        }
        else if ((int)properties.type <= (int)IngotProperties.WeaponType.undefinedaxe) // it's an axe
        {
            return FindPrefab(Axes, temp);
        }
        else if ((int)properties.type <= (int)IngotProperties.WeaponType.warhammer) //it's a hammer
        {
            return FindPrefab(Hammers, temp);
        }
        else
        {
            return null;//do nothing cause it is nothing
        }
    }

    private GameObject FindPrefab(List<GameObject> database, Blade.Constraint values)
    {
        for (int i = 0; i < database.Count; i++)
        {
            if (database[i].GetComponent<Blade>().constraint == values) return database[i];
        }
        return null;
    }
}
