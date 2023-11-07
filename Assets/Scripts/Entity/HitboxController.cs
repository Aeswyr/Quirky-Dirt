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
    [SerializeField] private bool destroyOnHit;
    private GameObject owner;
    private Team team;
    private List<StatController> hitTargets = new();
    private HitData data;

    [SyncVar] private GameManager.AttackBuilder initialData;
    [ClientRpc] public void Init(HitData data, GameManager.AttackBuilder attack) {
        this.data = data;
    }

    void Start() {
        this.team = initialData.team;

        gameObject.GetComponent<SpriteRenderer>().flipY = initialData.flip;

        gameObject.GetComponent<Rigidbody2D>().velocity = initialData.velocity;

        var col = gameObject.GetComponent<BoxCollider2D>();
        col.offset = initialData.hitboxOffset;
        col.size = initialData.hitboxSize;

        Animator animator = gameObject.GetComponent<Animator>();
        AnimatorOverrideController animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animController["attack"] = GameManager.Instance.GetAttackClip(initialData.type);
        animator.runtimeAnimatorController = animController;

        var dad = gameObject.GetComponent<DestroyAfterDelay>();
        dad.Init(GameManager.Instance.GetAttackClip(initialData.type).length - 1/60f);
        if (initialData.lifetime != 0) {
            dad.Init(initialData.lifetime);
        }

        this.destroyOnHit = initialData.destoryOnHit;

        this.owner = GameManager.Instance.GetRegisteredEntity(initialData.ownerID);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // team check
        StartCoroutine(OnHit(other));
    }

    IEnumerator OnHit(Collider2D other) {
        yield return new WaitUntil(() => owner != null);

        if (other.transform.parent.gameObject != owner && other.transform.parent.TryGetComponent(out StatController stats) && stats.GetTeam() != team && !hitTargets.Contains(stats)) {
            VFXManager.Instance.CreateVFX(VFXType.VFX_HIT, 0.5f * (transform.position + other.transform.position), Quaternion.identity);
            hitTargets.Add(stats);
            if (stats.OnHit(data, owner.GetComponent<StatController>()) && destroyOnHit) {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    public void SetDestroyOnHit(bool val) {
        this.destroyOnHit = val;
    }

    public void SetInitialData(GameManager.AttackBuilder initData) {
        this.initialData = initData;
    }
}



public enum Team {
    DEFAULT, PLAYER, ENEMY
}
