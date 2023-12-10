using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyStatController : StatController
{
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 10;
        curHP = maxHP;
    }

    public override void OnDeath() {
        Cleanup();
        [Command(requiresAuthority = false)] void Cleanup() {
            NetworkServer.Destroy(gameObject);
        }
    }

    public override void RegisterHit(HitData data, StatController sourceEntity, Transform source) {
        if (data.GetKnockback() != HitData.KnockbackStrength.NONE) {
            switch (data.GetKnockbackRelative()) {
                case HitData.KnockbackRelative.ROTATION:
                    GetComponent<EnemyController>().DoKnockback(data.GetKnockback(), source.rotation * Vector2.right);
                    break;
                case HitData.KnockbackRelative.SOURCE:
                    GetComponent<EnemyController>().DoKnockback(data.GetKnockback(), (transform.position - source.position).normalized);
                    break;
                case HitData.KnockbackRelative.ENTITY:
                    GetComponent<EnemyController>().DoKnockback(data.GetKnockback(), (transform.position - sourceEntity.transform.position).normalized);
                    break;
            }
        }
    }
}
