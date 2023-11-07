using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private AnimationClip[] attacks;
    // Start is called before the first frame update

    Dictionary<uint, GameObject> syncedEntities = new();

    public AttackBuilder CreateAttack(Team team, uint ownerID) {
        AttackBuilder newAttack = new AttackBuilder() {
            team = team,
            ownerID = ownerID,
            rotation = Quaternion.identity
        };
        return newAttack;
    }

    [Command(requiresAuthority = false)] public void CreateAttack(AttackBuilder data) {
        GameObject attack = Instantiate(attackPrefab, data.position, data.rotation);

        attack.GetComponent<SpriteRenderer>().flipY = data.flip;

        attack.GetComponent<Rigidbody2D>().velocity = data.velocity;

        var col = attack.GetComponent<BoxCollider2D>();
        col.offset = data.hitboxOffset;
        col.size = data.hitboxSize;


        Animator animator = attack.GetComponent<Animator>();
        AnimatorOverrideController animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animController["attack"] = attacks[(int)data.type];
        animator.runtimeAnimatorController = animController;

        var dad = attack.GetComponent<DestroyAfterDelay>();
        dad.Init(attacks[(int)data.type].length - 1/60f);
        if (data.lifetime != 0) {
            dad.Init(data.lifetime);
        }

        var hitController = attack.GetComponent<HitboxController>();
        hitController.SetDestroyOnHit(data.destoryOnHit);

        if (data.enableWallCollisions)
            attack.transform.GetChild(0).gameObject.SetActive(true);

        NetworkServer.Spawn(attack);
        
        hitController.Init(null, data.ownerID, data.team);
    }

    public void RegisterEntity(uint id, GameObject entity) {
        syncedEntities.Add(id, entity);
    }

    public GameObject GetRegisteredEntity(uint id) {
        return syncedEntities[id];
    }

    public struct AttackBuilder {
        public AttackType type;
        public Vector2 hitboxSize;
        public Vector2 hitboxOffset;
        public Vector2 position;
        public Quaternion rotation;
        public Vector2 velocity;
        public Team team;
        public uint ownerID;
        public bool flip;
        public float lifetime;
        public bool destoryOnHit;
        public bool enableWallCollisions;

        public AttackBuilder SetVelocity(Vector2 dir, float mag) {
            this.velocity = mag * dir;
            return this;
        }

        public AttackBuilder SetType(AttackType type) {
            this.type = type;
            return this;
        }

        public AttackBuilder SetPosition(Vector2 pos) {
            this.position = pos;
            return this;
        }

        public AttackBuilder SetRotation(Quaternion rot) {
            rotation = rot;
            return this;
        }

        public AttackBuilder SetHitboxSize(Vector2 size) {
            hitboxSize = size;
            return this;
        }

        public AttackBuilder SetHitboxOffset(Vector2 off) {
            hitboxOffset = off;
            return this;
        }

        public AttackBuilder SetFlip(bool flip) {
            this.flip = flip;
            return this;
        }

        public AttackBuilder SetLifetime(float time) {
            this.lifetime = time;
            return this;
        }
        public void Finish() {
            Instance.CreateAttack(this);
        }

        public AttackBuilder EnableDestroyOnHit() {
            destoryOnHit = true;
            return this;
        }

        public AttackBuilder EnableDestroyOnWall() {
            enableWallCollisions = true;
            return this;
        }
    }
}

public enum AttackType {
    DEFAULT, ARROW, FLASH_CUT
}
