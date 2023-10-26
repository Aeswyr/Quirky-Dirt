using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class ItemSlotController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Image image;
    [SerializeField] private ItemType type;
    public int index;
    // Start is called before the first frame update
    void Awake()
    {
        EmptySlot();
    }

    public void SetIndex(int index) {
        this.index = index;
    }

    public void EmptySlot() {
        if (emptySprite == null)
            image.color = Color.clear;
        else {
            image.color = Color.white;
            image.sprite = emptySprite;
        }
    }

    public void FillSlot(Sprite icon) {
        image.color = Color.white;
        image.sprite = icon;
    }

    public void OnPointerDown(PointerEventData data) {
        if (!InventoryManager.Instance.HasItemAtIndex(index))
            return;

        InventoryManager.Instance.ToggleDraggableItem(image.sprite, true);

        Color color = Color.white;
        color.a = 0.5f;
        image.color = color;
    }

    public void OnPointerUp(PointerEventData data) {
        if (!InventoryManager.Instance.HasItemAtIndex(index))
            return;

        InventoryManager.Instance.ToggleDraggableItem(image.sprite, false);
        image.color = Color.white;

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count <= 0)
            return;

        ItemSlotController target = null;
        foreach (var result in results) {
            if (result.gameObject.TryGetComponent(out target))
                break;
        }

        if (target == null) // not over a valid hover target
            return;



        Item item = InventoryManager.Instance.GetItemAtIndex(index);
        if (target.type != ItemType.NONE && !item.types.Contains(target.type))
            return;

        InventoryManager.Instance.RemoveItem(index);
        if (InventoryManager.Instance.HasItemAtIndex(target.index)) {
            Item swap = InventoryManager.Instance.RemoveItem(target.index);
            InventoryManager.Instance.AddItem(swap, index);
        }

        InventoryManager.Instance.AddItem(item, target.index);
    }
}
