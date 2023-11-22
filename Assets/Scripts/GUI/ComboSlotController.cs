using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class ComboSlotController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private int attackID = -1;
    [SerializeField] private bool slotOpen;
    [SerializeField] private AttackDictionary attacks;
    [SerializeField] private TextMeshProUGUI text;
    public int GetID() {
        return attackID;
    }

    public void SetAttack(int id) {
        this.attackID = id;
        if (attackID != -1)
            text.text = attacks.GetAttack(attackID).Name;
        else
            text.text = "";
    }

    public void OnPointerDown(PointerEventData data) {
        if (slotOpen) {
            SetAttack(-1);
            ComboBuilderManager.Instance.RefreshCombos();
            return;
        }

        ComboBuilderManager.Instance.ToggleDraggableCombo(true, attacks.GetAttack(attackID).Name);
    }

    public void OnPointerUp(PointerEventData data) {
        if (slotOpen) {
            return;
        }

        ComboBuilderManager.Instance.ToggleDraggableCombo(false);

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count <= 0)
            return;

        ComboSlotController target = null;
        foreach (var result in results) {
            if (result.gameObject.TryGetComponent(out target))
                break;
        }

        if (target == null || !target.slotOpen) // not over a valid hover target
            return;


        target.SetAttack(attackID);

        ComboBuilderManager.Instance.RefreshCombos();
    }

}
