using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : Singleton<HUDManager>
{
    [SerializeField] private RectTransform hpEmpty;
    [SerializeField] private RectTransform hpBar;
    [SerializeField] private RectTransform arBar;
    [SerializeField] private RectTransform barrierBar;
    [SerializeField] private RectTransform mpEmpty;
    [SerializeField] private RectTransform mpBar;
    [SerializeField] private RectTransform fpBar;

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
}