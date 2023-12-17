using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboBuilderManager : Singleton<ComboBuilderManager>
{
    [SerializeField] private GameObject comboDraggable;
    [SerializeField] private GameObject comboMenuParent;
    [SerializeField] private GameObject comboPrefab;
    [SerializeField] private Transform comboListParent;
    [SerializeField] private AttackDictionary attacks;

    [SerializeField] private List<ComboSlotController> leftSlots;
    [SerializeField] private List<ComboSlotController> rightSlots;
    private int[] leftCombo;
    private int[] rightCombo;
    private List<int> validCombos = new List<int>();

    void Start() {
        
       RefreshCombos();

        ToggleDraggableCombo(false);

        foreach (var slot in leftSlots)
            slot.SetAttack(-1);
        foreach (var slot in rightSlots)
            slot.SetAttack(-1);
    }

    public void SetActive(bool toggle) {
        comboMenuParent.SetActive(toggle);
        if (toggle) {
            GenerateComboList();
            
            for (int i = 0; i < comboListParent.childCount; i++)
                Destroy(comboListParent.GetChild(i).gameObject);

            foreach (var id in validCombos) {
                Instantiate(comboPrefab, comboListParent).GetComponent<ComboSlotController>().SetAttack(id);
            }

        }
    }

    public void GenerateComboList() {
        validCombos.Clear();

        // add basic unarmed
        validCombos.Add(4);
        validCombos.Add(5);

        // add equipment combos
        foreach (var item in InventoryManager.Instance.GetEquipment())
            if (item.attackIDs != null)
                foreach (var id in item.attackIDs)
                    if (!validCombos.Contains(id))
                        validCombos.Add(id);
    }

    public void RefreshCombos() {
        List<int> combo = new();
        foreach(var slot in leftSlots)
            if (slot.GetID() != -1)
                combo.Add(slot.GetID());
        if (combo.Count > 0)
            leftCombo = combo.ToArray();
        else
            leftCombo = new int[] {4};

        combo.Clear();
        foreach(var slot in rightSlots)
            if (slot.GetID() != -1 && validCombos.Contains(slot.GetID()))
                combo.Add(slot.GetID());
        if (combo.Count > 0)
            rightCombo = combo.ToArray();
        else
            rightCombo = new int[] {5};
    }

    public void EquipmentChanged() {
        GenerateComboList();

        foreach (var slot in leftSlots)
            if (slot.GetID() != -1 && !validCombos.Contains(slot.GetID()))
                slot.SetAttack(-1);

        foreach (var slot in rightSlots)
            if (slot.GetID() != -1 && !validCombos.Contains(slot.GetID()))
                slot.SetAttack(-1);

        RefreshCombos();
    }

    public int[] GetLeftCombo() {
        return leftCombo;
    }

    public int[] GetRightCombo() {
        return rightCombo;
    }

    void FixedUpdate() {
        if (!comboMenuParent.activeInHierarchy)
            return;

        comboDraggable.transform.position = InputHandler.Instance.mousePos;
    }

    public void ToggleDraggableCombo(bool toggle, string name = "") {
        comboDraggable.SetActive(toggle);

        if (toggle) {
            comboDraggable.GetComponentInChildren<TextMeshProUGUI>().text = name;
        }
    }
}
