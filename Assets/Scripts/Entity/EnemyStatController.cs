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
        CreateDrops();
        Cleanup();
        [Server] void Cleanup() {
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server] private void CreateDrops() {
        int[] pickups = null;

        switch (Random.Range(0, 3))
        {
            case 0:
                pickups = new[]{0};
                break;
            case 1:
                pickups = new[]{0, 1};
                break;
            case 2:
                pickups = new[]{0, 1, 2};
                break;
        }

        GameManager.Instance.CreateDrop(pickups, transform.position);
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
