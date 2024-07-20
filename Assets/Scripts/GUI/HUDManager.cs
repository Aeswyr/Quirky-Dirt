using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : Singleton<HUDManager>
{
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private GameObject armorToken;
    [SerializeField] private TextMeshProUGUI arText;
    [SerializeField] private Image mpBar;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private GameObject focusToken;
    [SerializeField] private TextMeshProUGUI fpText;
    
    
    [SerializeField] private RectTransform staminaBar;
    [SerializeField] private GameObject staminaToken;
    [SerializeField] private Sprite stamEmpty;
    [SerializeField] private Sprite stamFull;
    [SerializeField] private Sprite stamLock;
    private List<GameObject> staminaTokens = new();

    public void UpdateMP(int maxMP, int curMP, int curFP) {
        mpBar.fillAmount = (float)curMP / maxMP;
        mpText.text = $"{curMP}/{maxMP}";
        if (curFP > 0) {
            focusToken.SetActive(true);
            fpText.text = $"x{curFP}";
        } else {
            focusToken.SetActive(false);
        }
    }

    public void UpdateHP(int maxHP, int curHP, int curAR) {
        hpBar.fillAmount = (float)curHP / maxHP;
        hpText.text = $"{curHP}/{maxHP}";
        if (curAR > 0) {
            armorToken.SetActive(true);
            arText.text = $"x{curAR}";
        } else {
            armorToken.SetActive(false);
        }
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