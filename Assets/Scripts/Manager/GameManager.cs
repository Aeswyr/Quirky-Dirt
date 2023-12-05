using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private AnimationClip[] attacks;
    [SerializeField] private GameObject[] enemyPrefabs;

    // Start is called before the first frame update
    void Start() {
        if (!isServer)
            return;

        foreach (var enemy in enemyPrefabs)
            NetworkClient.RegisterPrefab(enemy);
        
    }

    int tick = 0;
    void FixedUpdate() {
        if (!isServer)
            return;

        if (tick == 60)
            SpawnEnemy(EnemyType.RAT, Vector3.zero);

        tick++;
    }

    public void SpawnEnemy(EnemyType type, Vector3 position) {
        NetworkServer.Spawn(Instantiate(enemyPrefabs[(int)type], position, Quaternion.identity));
    }

    public enum EnemyType {
        DEFAULT, RAT
    }


    Dictionary<uint, GameObject> syncedEntities = new();

    public AttackBuilder CreateAttack(int attackID, Team team, uint ownerID) {
        AttackBuilder newAttack = new AttackBuilder() {
            team = team,
            ownerID = ownerID,
            rotation = Quaternion.identity,
            attackID = attackID
        };
        return newAttack;
    }

    [Command(requiresAuthority = false)] public void CreateAttack(AttackBuilder data) {
        GameObject attack = Instantiate(attackPrefab, data.position, data.rotation);

        attack.GetComponent<HitboxController>().SetAttackData(data);

        if (data.enableWallCollisions)
            attack.transform.GetChild(0).gameObject.SetActive(true);

        NetworkServer.Spawn(attack);
    }

    public AnimationClip GetAttackClip(AttackType type) {
        return attacks[(int)type];
    }

    public void RegisterEntity(uint id, GameObject entity) {
        syncedEntities.Add(id, entity);
    }

    public GameObject GetRegisteredEntity(uint id) {
        return syncedEntities[id];
    }

    public struct AttackBuilder {
        public int attackID;
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
    DEFAULT, ARROW, FLASH_CUT, IMPACT
}
