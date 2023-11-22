using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;

    private int maxHP, maxAR, maxMP, maxFP;
    private int curHP, curAR, curMP, curFP;
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
        if (team == Team.PLAYER) {
            RecalculateStats();

            curAR = 0;
            curFP = 0;
            curMP = maxMP;
            curStam = maxStam;
        } else {
            maxHP = 10;
        }

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


    public bool OnHit(HitData data, StatController source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || source.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;
        
        VFXManager.Instance.CreateFloatingText($"{data.GetDamage(0, 0)}", Color.red, transform.position + new Vector3(Random.Range(-0.25f, 0.25f), 1.5f, 0));

        return true;
    }

    public Team GetTeam() {return team;}

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
}
