using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HitboxController : NetworkBehaviour
{
    [SerializeField] private AttackDictionary attackDictionary;
    [SerializeField] private bool destroyOnHit;
    private GameObject owner;
    private List<StatController> hitTargets = new();

    [SyncVar] private GameManager.AttackBuilder data;

    void Start() {
        gameObject.GetComponent<SpriteRenderer>().flipY = data.flip;

        gameObject.GetComponent<Rigidbody2D>().velocity = data.velocity;

        var col = gameObject.GetComponent<BoxCollider2D>();
        col.offset = data.hitboxOffset;
        col.size = data.hitboxSize;

        Animator animator = gameObject.GetComponent<Animator>();
        AnimatorOverrideController animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animController["attack"] = GameManager.Instance.GetAttackClip(data.type);
        animator.runtimeAnimatorController = animController;

        var dad = gameObject.GetComponent<DestroyAfterDelay>();
        dad.Init(GameManager.Instance.GetAttackClip(data.type).length - 1/60f);
        if (data.lifetime != 0) {
            dad.Init(data.lifetime);
        }

        this.destroyOnHit = data.destoryOnHit;

        this.owner = GameManager.Instance.GetRegisteredEntity(data.ownerID);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // team check
        StartCoroutine(OnHit(other));
    }

    IEnumerator OnHit(Collider2D other) {
        yield return new WaitUntil(() => owner != null);

        if (other.transform.parent.gameObject != owner && other.transform.parent.TryGetComponent(out StatController stats) && stats.GetTeam() != data.team && !hitTargets.Contains(stats)) {
            VFXManager.Instance.CreateVFX(VFXType.VFX_HIT, 0.5f * (transform.position + other.transform.position), Quaternion.identity);
            hitTargets.Add(stats);
            if (stats.OnHit(attackDictionary.GetAttack(data.attackID).Data, owner.GetComponent<StatController>(), transform) && destroyOnHit) {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    public void SetDestroyOnHit(bool val) {
        this.destroyOnHit = val;
    }

    public void SetAttackData(GameManager.AttackBuilder initData) {
        this.data = initData;
    }
}



public enum Team {
    DEFAULT, PLAYER, ENEMY
}
