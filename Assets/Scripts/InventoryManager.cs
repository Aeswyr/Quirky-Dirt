using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] private GameObject inventoryParent;
    public void SetActive(bool toggle) {
        inventoryParent.SetActive(toggle);
    }
}
