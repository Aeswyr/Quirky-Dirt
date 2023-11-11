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
    private int curBarrier;

    private float nextStamina;
    private float staminaDelay;

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

            curAR = maxAR;
            curFP = maxFP;
            curMP = maxMP;
            curStam = maxStam;
            curBarrier = 0;
        } else {
            maxHP = 10;
        }

        curHP = maxHP;


        if (isLocalPlayer) {
            HUDManager.Instance.UpdateHP(maxHP + maxAR, curHP, curAR, curBarrier);
            HUDManager.Instance.UpdateMP(maxMP + maxFP, curMP, curFP);
            HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
        }
    }

    public void RecalculateStats() {
            maxHP = 5 + 2 * res;
            maxMP = 5 + 2 * att;
            maxStam = 3 + phy / 4;
        
            maxAR = 0;
            maxFP = 0;

            staminaDelay = 1.5f - 0.1f * phy; // INCREDIBLY TEMPORARY, DO REAL MATH
    }


    public bool OnHit(HitData data, StatController source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || source.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;
        
        VFXManager.Instance.CreateFloatingText($"{data.GetDamage(0, 0)}", Color.red, transform.position + 1.5f * Vector3.up);

        return true;
    }

    public Team GetTeam() {return team;}

    public bool CanSpendStam(int amt) {
        return curStam >= amt;
    }

    public void SpendStam(int amt) {
        curStam -= amt;
        nextStamina = Time.time + staminaDelay;
        HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
    }

    void FixedUpdate() {
        if (curStam < maxStam - lockStam && Time.time > nextStamina) {
            curStam++;
            nextStamina = Time.time + staminaDelay;
            HUDManager.Instance.UpdateStam(maxStam, curStam, lockStam);
        }
    }
}
