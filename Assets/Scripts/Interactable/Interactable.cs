using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent<PlayerController> onInteract;
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;
    private void OnTriggerEnter2D(Collider2D other) {
        if (onEnter != null && other.transform.parent.GetComponent<PlayerController>().isLocalPlayer)
            onEnter.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (onExit != null && other.transform.parent.GetComponent<PlayerController>().isLocalPlayer)
            onExit.Invoke();
    }

    public void Trigger(PlayerController source) {
        onInteract.Invoke(source);
    }
}
