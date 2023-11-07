using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent<PlayerController> onInteract;
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;
    PlayerController collidingPlayer;
    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController player = other.transform.parent.GetComponent<PlayerController>();
        if (onEnter != null && player.isLocalPlayer)
            collidingPlayer = player;
    }

    private void OnTriggerExit2D(Collider2D other) {
        PlayerController player = other.transform.parent.GetComponent<PlayerController>();
        if (onExit != null && player.isLocalPlayer) {
            collidingPlayer = null;
            if (player.GetNearestInteractable() == this)  {
                player.SetNearestInteractable(null);
                if (onExit != null)
                    onExit.Invoke();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (collidingPlayer == null)
            return;

        Interactable nearest = collidingPlayer.GetNearestInteractable();
        if (nearest == this)
            return;
        if (nearest == null ||
            Dist2(collidingPlayer.transform.position, nearest.transform.position) > Dist2(collidingPlayer.transform.position, transform.position)) {
                if (nearest != null && nearest.onExit != null)
                    nearest.onExit.Invoke();
                if (onEnter != null)
                    onEnter.Invoke();

                collidingPlayer.SetNearestInteractable(this);
            }
    }

    private float Dist2(Vector3 a, Vector3 b) {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }

    public void Trigger(PlayerController source) {
        onInteract.Invoke(source);
    }
}
