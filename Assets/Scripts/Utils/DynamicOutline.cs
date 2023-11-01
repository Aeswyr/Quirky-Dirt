using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicOutline : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Material outlineMat;
    private int index = -1;

    public void ShowOutline() {
        HideOutline();

        List<Material> mats = new();

        sprite.GetSharedMaterials(mats);
        mats.Add(outlineMat);
        sprite.materials = mats.ToArray();
        index = mats.Count - 1;
    }

    public void HideOutline() { 
        if (index == -1)
            return;

        List<Material> mats = new();

        sprite.GetSharedMaterials(mats);
        mats.RemoveAt(index);
        sprite.materials = mats.ToArray();

        index = -1;
    }
}
