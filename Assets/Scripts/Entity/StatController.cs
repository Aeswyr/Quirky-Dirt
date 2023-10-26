using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;

    private int maxHP, maxAR, maxMP, maxFP;
    private int curHP, curAR, curMP, curFP;
    private int curBarrier;

    private int luk, // luck
                res, // resistance
                att, // attunement
                phy, // phyique
                tec, // technique
                mgt, // might
                prs; // presence

    void Start() {
            
            
        if (team == Team.PLAYER) {
            maxHP = 5 + 2 * res;
            maxMP = 5 + 2 * att;
            maxAR = 0;
            maxFP = 0;


            curAR = maxAR;
            curFP = maxFP;
            curMP = maxMP;
            curBarrier = 0;
        } else {
            maxHP = 10;
        }

        curHP = maxHP;


        if (isLocalPlayer) {
            HUDManager.Instance.UpdateHP(maxHP + maxAR, curHP, curAR, curBarrier);
            HUDManager.Instance.UpdateMP(maxMP + maxFP, curMP, curFP);
        }
    }



    public bool OnHit(HitData data, StatController source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || source.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;
        
        VFXManager.Instance.CreateFloatingText("5", Color.red, transform.position + 1.5f * Vector3.up);

        return true;
    }

    public Team GetTeam() {return team;}
}
