using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatController : StatController
{   
    private int maxAR, maxMP, maxFP;
    private int curAR, curMP, curFP;
    private int curStam, maxStam, lockStam;

    private float nextStamina, staminaDelay;
    private float nextArmor, armorDelay;
    private float nextFocus, focusDelay;


    private int luk, // luck
                res, // resistance
                att, // attunement
                phy, // phyique
                tec, // technique
                mgt, // might
                prs; // presence
    
    
    void Start() {
        RecalculateStats();

        curAR = 0;
        curFP = 0;
        curMP = maxMP;
        curStam = maxStam;
        curHP = maxHP;


        if (isLocalPlayer) {
            HUDManager.Instance.UpdateHP(maxHP, curHP, curAR);
            HUDManager.Instance.UpdateMP(maxMP, curMP, curFP);
            HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
        }
    }

    public void RecalculateStats() {
            maxHP = 5 + 2 * res;
            maxMP = 5 + 2 * att;
            maxStam = 3 + phy / 4;
        
            maxAR = 3;
            maxFP = 3;

            staminaDelay = 1.5f - 0.1f * phy; // INCREDIBLY TEMPORARY, DO REAL MATH

            armorDelay = 3f;
            focusDelay = 5f;

            nextArmor = Time.time + armorDelay;
            nextFocus = Time.time + focusDelay;
    }

    public bool CanSpendStam(int amt) {
        return curStam >= amt;
    }

    public void SpendStam(int amt) {
        curStam -= amt;
        if (curStam < 0)
            curStam = 0;
        nextStamina = Time.time + staminaDelay;
        HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
    }

    void FixedUpdate() {
        if (curStam < maxStam - lockStam && Time.time > nextStamina) {
            curStam++;
            nextStamina = Time.time + staminaDelay;
            HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
        }

        if (curAR < maxAR && Time.time > nextArmor) {
            curAR++;
            nextArmor = Time.time + armorDelay;
            HUDManager.Instance.UpdateHP(maxHP, curHP, curAR);
        }

        if (curFP < maxFP && Time.time > nextFocus) {
            curFP++;
            nextFocus = Time.time + focusDelay;
            HUDManager.Instance.UpdateMP(maxMP, curMP, curFP);
        }
    }

    public override bool OnHit(HitData data, StatController sourceEntity, Transform source) {
        bool result = true;
        if (curAR > 0) {
            curAR = Mathf.Max(0, curAR - data.GetDamage(atk, matk));
        } else {
            result = base.OnHit(data, sourceEntity, source);
        }
        HUDManager.Instance.UpdateHP(maxHP, curHP, curAR);
        return result;
    }

    public override void OnDeath() {

    }

    public override void RegisterHit(HitData data, StatController sourceEntity, Transform source) {

    }
}
