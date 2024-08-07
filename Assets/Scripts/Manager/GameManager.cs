using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private AnimationClip[] attacks;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject dropPrefab;

    // Start is called before the first frame update
    void Awake() {
        foreach (var enemy in enemyPrefabs)
            NetworkClient.RegisterPrefab(enemy);
    }

    int tick = 0;
    void FixedUpdate() {
        if (!isServer)
            return;

        if (tick % 300 == 0)
            SpawnEnemy(EnemyType.RAT, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0));

        tick++;
    }

    public void SpawnEnemy(EnemyType type, Vector3 position) {
        NetworkServer.Spawn(Instantiate(enemyPrefabs[(int)type], position, Quaternion.identity));
    }

    [Server] public void CreateDrop(int[] drops, Vector3 pos) {
        var drop = Instantiate(dropPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(drop);

        drop.GetComponent<PickupController>().SetLoot(drops);
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

    private void CreateAttackLocal(AttackBuilder data) {
        GameObject attack = Instantiate(attackPrefab, data.position, data.rotation);

        attack.GetComponent<HitboxController>().SetAttackData(data);

        if (data.enableWallCollisions)
            attack.transform.GetChild(0).gameObject.SetActive(true);
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
        public bool isPrespawned;
        public bool useEnemyAttacks;

        public AttackBuilder UseEnemyAttacks() {
            useEnemyAttacks = true;
            return this;
        }

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

        public void FinishClientPriority() {
            Instance.CreateAttackLocal(this);
            isPrespawned = true;
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
    DEFAULT, ARROW, FLASH_CUT, IMPACT, THRUST
}
