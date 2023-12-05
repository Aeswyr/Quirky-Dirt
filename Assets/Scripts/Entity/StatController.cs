using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;

    protected int maxHP, curHP;
    protected int matk, atk;
    


    public bool OnHit(HitData data, StatController source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || source.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;
        
        VFXManager.Instance.CreateFloatingText($"{data.GetDamage(0, 0)}", Color.red, transform.position + new Vector3(Random.Range(-0.25f, 0.25f), 1.5f, 0));

        curHP -= data.GetDamage(atk, matk);

        if (curHP <= 0)
            OnDeath();

        return true;
    }

    public abstract void OnDeath();

    public Team GetTeam() {return team;}
}
