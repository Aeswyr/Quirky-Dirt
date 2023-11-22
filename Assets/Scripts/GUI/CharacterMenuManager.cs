using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterMenuManager : Singleton<CharacterMenuManager>
{
    [SerializeField] private GameObject menuParent;
    MenuState state = MenuState.INVENTORY;

    public void OpenMenu() {
        SetState(state);
    }

    public void CloseMenu() {
        menuParent.SetActive(false);
        InventoryManager.Instance.SetActive(false);
        ComboBuilderManager.Instance.SetActive(false);

    }

    public void ToggleMenu(bool toggle) {
        if (toggle)
            OpenMenu();
        else
            CloseMenu();
    }

    private void SetState(MenuState state) {
        this.state = state;

        CloseMenu();
        menuParent.SetActive(true);

        switch (state) {
            case MenuState.INVENTORY:
                InventoryManager.Instance.SetActive(true);
                break;
            case MenuState.COMBO:
                ComboBuilderManager.Instance.SetActive(true);
                break;
        }

    }

    public void SetState(int state)
    {
        SetState((MenuState)state);
    }

    private enum MenuState {
        INVENTORY, COMBO,
    }

}
