using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PickupController : NetworkBehaviour
{
    public void OnPickup(PlayerController player) {
        InventoryManager.Instance.AddItem(new Item {
            iconID = 0,
            types = new []{ItemType.MAINHAND, ItemType.OFFHAND}
        });
        InventoryManager.Instance.AddItem(new Item {
            iconID = 1,
            types = new []{ItemType.MAINHAND}
        });

        CleanupDrop();
    }

    [Command(requiresAuthority = false)] private void CleanupDrop() {
        NetworkServer.Destroy(gameObject);
    }
}
