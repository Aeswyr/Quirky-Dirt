using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public abstract class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private InvulnState invuln;
    [SerializeField] private Image hpBar;

    protected enum InvulnState {
        NONE, INTANGIBLE, IMPERVIOUS
    }

    [SyncVar(hook = nameof(SyncHealth))] protected int curHP;
    protected int maxHP;
    protected int matk, atk;
    


    public virtual bool OnHit(HitData data, StatController sourceEntity, Transform source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || sourceEntity.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;

        if (invuln == InvulnState.INTANGIBLE)
            return false;

        int dmg = data.GetDamage(atk, matk);
        VFXManager.Instance.CreateFloatingText($"{dmg}", Color.red, transform.position + new Vector3(Random.Range(-0.25f, 0.25f), 1.5f, 0));
        
        if (invuln != InvulnState.IMPERVIOUS)
            AdjustHealth(-dmg);

        if (gameObject.TryGetComponent(out EnemyController enemy))
            enemy.AddAggro(player, dmg);

        RegisterHit(data, sourceEntity, source);

        return true;
    }

    [Command(requiresAuthority = false)] private void AdjustHealth(int amt) {
        curHP += amt;

        if (curHP <= 0)
            OnDeath();
    }

    private void SyncHealth(int oldHealth, int newHealth) {
        if (hpBar != null)
            hpBar.fillAmount = (float)newHealth / maxHP;
    }

    public abstract void OnDeath();

    public abstract void RegisterHit(HitData data, StatController sourceEntity, Transform source);

    public Team GetTeam() {return team;}
}
