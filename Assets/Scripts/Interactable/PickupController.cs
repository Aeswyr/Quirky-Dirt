using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PickupController : NetworkBehaviour
{

    [SerializeField] private List<int> pickups;
    [SerializeField] private ItemDictionary items;
    [SerializeField] private ItemIconDictionary itemIcons;
    [SerializeField] private Transform itemHolder;
    [SerializeField] private GameObject itemDisplayPrefab;
    [SerializeField] private Animator fadeAnimator;

    [ClientRpc] public void SetLoot(int[] drops) {
        pickups = new(drops);

        foreach (var pickup in pickups) {
            var display = Instantiate(itemDisplayPrefab, itemHolder);
            var image = display.GetComponent<Image>();
            image.sprite = itemIcons.GetIcon(items.GetItem(pickup).iconID);
            image.SetNativeSize();
        }
    }

    public void FadeInItems() {
        fadeAnimator.SetTrigger("fadeIn");
    }

    public void FadeOutItems() {
        fadeAnimator.SetTrigger("fadeOut");
    }

    public void OnPickup(PlayerController player) {
        if (pickups.Count == 0)
            return;

        foreach (var pickup in pickups)
            InventoryManager.Instance.AddItem(items.GetItem(pickup));

        CleanupDrop();
    }

    [Command(requiresAuthority = false)] private void CleanupDrop() {
        NetworkServer.Destroy(gameObject);
    }
}
