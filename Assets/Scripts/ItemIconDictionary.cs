using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemIconDictionary", menuName = "Quirky-Dirt/ItemIconDictionary", order = 0)]
public class ItemIconDictionary : ScriptableObject {
    [SerializeField] private List<Sprite> icons;

    public Sprite GetIcon(int id) {
        return icons[id];
    }
}
