using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DestroyOnHit : NetworkBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private VFXType destructionVFX;
    private void OnTriggerEnter2D(Collider2D other) {
        TriggerDestruction();
    }

    private void OnTriggerStay2D(Collider2D other) {
        TriggerDestruction();
    }

    private void TriggerDestruction() {
        NetworkServer.Destroy(parent.gameObject);
        if (destructionVFX != VFXType.NONE)
            VFXManager.Instance.CreateVFX(destructionVFX, parent.position, parent.rotation, 10, "Entity");
    }
}
