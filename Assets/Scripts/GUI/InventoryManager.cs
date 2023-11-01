using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.UI;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemSlotParent;
    [SerializeField] private GameObject inventoryParent;
    [SerializeField] private int inventorySize;
    [SerializeField] private Image itemDisplay;

    public ItemIconDictionary iconDictionary;

    private Item?[] items;
    private List<ItemSlotController> slots = new();
    [SerializeField] private List<ItemSlotController> equipmentSlots;

    void Start() {
        items = new Item?[inventorySize + equipmentSlots.Count];

        for (int i = 0; i < inventorySize; i++) {
            slots.Add(Instantiate(itemSlotPrefab, itemSlotParent).GetComponent<ItemSlotController>());
            slots[i].SetIndex(i);
        }

        for (int i = 0; i < equipmentSlots.Count; i++) {
            equipmentSlots[i].SetIndex(inventorySize + i);
            slots.Add(equipmentSlots[i]);
        }
    }

    public void AddItem(Item item) {
        for (int i = 0; i < inventorySize; i++) {
            if (!items[i].HasValue) {
                items[i] = item;
                slots[i].FillSlot(iconDictionary.GetIcon(item.iconID));
                break;
            }
        }
    }

    public void AddItem(Item item, int index) {
        if (!items[index].HasValue) {
            items[index] = item;
            slots[index].FillSlot(iconDictionary.GetIcon(item.iconID));
        }
    }

    public Item RemoveItem(int index) {
        Item item = items[index].GetValueOrDefault();

        items[index] = null;
        slots[index].EmptySlot();

        return item;
    }

    public void SetActive(bool toggle) {
        inventoryParent.SetActive(toggle);
    }

    public bool HasItemAtIndex(int index) {
        return items[index].HasValue;
    }

    public Item GetItemAtIndex(int index) {
        return items[index].GetValueOrDefault();
    }

    void FixedUpdate() {
        if (!inventoryParent.activeInHierarchy)
            return;

        itemDisplay.transform.position = InputHandler.Instance.mousePos;
    }

    public void ToggleDraggableItem(Sprite sprite, bool toggle) {
        itemDisplay.sprite = sprite;
        if (toggle) {
            Color color = Color.white;
            color.a = 0.5f;
            itemDisplay.color = color;
        } else {
            itemDisplay.color = Color.clear;
        }
    }
}
