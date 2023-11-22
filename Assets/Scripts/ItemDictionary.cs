using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDictionary", menuName = "Quirky-Dirt/ItemDictionary", order = 0)]
public class ItemDictionary : ScriptableObject {
    [SerializeField] private List<Item> itemList;

    public Item GetItem(int id) {
        return itemList[id];
    }
}
