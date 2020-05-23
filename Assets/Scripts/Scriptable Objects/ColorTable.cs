using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Color Table", menuName = "Scriptable Objects/Color/Create Color Table")]
public class ColorTable : ScriptableObject
{
    public List<Material> availableColors;

    public Material GetRandomMaterial()
    {
        if(availableColors.Count > 1)
        {
            return availableColors[Random.Range(0, availableColors.Count)];
        }

        throw new UnityException($"[{typeof(ColorTable).Name}] used Color Table not have color variant. At least it must have 2 color.");
    }
}
