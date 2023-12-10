using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;

    protected int maxHP, curHP;
    protected int matk, atk;
    


    public virtual bool OnHit(HitData data, StatController sourceEntity, Transform source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || sourceEntity.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;

        int dmg = data.GetDamage(atk, matk);
        VFXManager.Instance.CreateFloatingText($"{dmg}", Color.red, transform.position + new Vector3(Random.Range(-0.25f, 0.25f), 1.5f, 0));

        curHP -= dmg;
        if (gameObject.TryGetComponent(out EnemyController enemy))
            enemy.AddAggro(player, dmg);

        RegisterHit(data, sourceEntity, source);

        if (curHP <= 0)
            OnDeath();

        return true;
    }

    public abstract void OnDeath();

    public abstract void RegisterHit(HitData data, StatController sourceEntity, Transform source);

    public Team GetTeam() {return team;}
}
