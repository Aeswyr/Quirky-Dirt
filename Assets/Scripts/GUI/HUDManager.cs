using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : Singleton<HUDManager>
{
    [SerializeField] private RectTransform hpEmpty;
    [SerializeField] private RectTransform hpBar;
    [SerializeField] private RectTransform arBar;
    [SerializeField] private RectTransform barrierBar;
    [SerializeField] private RectTransform mpEmpty;
    [SerializeField] private RectTransform mpBar;
    [SerializeField] private RectTransform fpBar;
    [SerializeField] private RectTransform staminaBar;
    [SerializeField] private GameObject staminaToken;
    [SerializeField] private Sprite stamEmpty;
    [SerializeField] private Sprite stamFull;
    [SerializeField] private Sprite stamLock;
    private List<GameObject> staminaTokens = new();

    public void UpdateMP(int maxMP, int curMP, int curFP) {
        mpEmpty.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxMP);
        mpBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curMP);
        fpBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curFP);
    }

    public void UpdateHP(int maxHP, int curHP, int curAR, int curBar) {
        hpEmpty.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxHP);
        hpBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curHP);
        arBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curAR);
        barrierBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curBar);
    }

    public void UpdateStam(int maxStam, int currStam, int lockedStam) {
        if (maxStam != staminaTokens.Count) {
            foreach (var token in staminaTokens)
                Destroy(token);
            staminaTokens.Clear();
            for (int i = 0; i <maxStam; i++)
                staminaTokens.Add(Instantiate(staminaToken, staminaBar));
        }

        for (int i = 0; i < maxStam; i++) {
            var token = staminaTokens[i].GetComponent<Image>();
            if (i < currStam)
                token.sprite = stamFull;
            else if (i >= maxStam - lockedStam)
                token.sprite = stamLock;
            else
                token.sprite = stamEmpty;
        }
    }
}