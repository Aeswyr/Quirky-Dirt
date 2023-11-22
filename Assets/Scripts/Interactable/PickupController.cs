using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PickupController : NetworkBehaviour
{
    [SerializeField] private ItemDictionary items;
    public void OnPickup(PlayerController player) {
        InventoryManager.Instance.AddItem(items.GetItem(0));
        InventoryManager.Instance.AddItem(items.GetItem(1));

        CleanupDrop();
    }

    [Command(requiresAuthority = false)] private void CleanupDrop() {
        NetworkServer.Destroy(gameObject);
    }
}
