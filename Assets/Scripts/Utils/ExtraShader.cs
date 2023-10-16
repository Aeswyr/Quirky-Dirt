using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraShader : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private List<Material> shaders;

    void Start()
    {
        if (shaders.Count == 0)
            return;

        List<Material> mats = new();
        sprite.GetSharedMaterials(mats);
        mats.AddRange(shaders);
        sprite.materials = mats.ToArray();
    }
}
