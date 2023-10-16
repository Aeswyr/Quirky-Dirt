using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private GameObject[] attacks;
    // Start is called before the first frame update

    Dictionary<uint, GameObject> syncedEntities = new();

    [Command(requiresAuthority = false)] public void CreateAttack(Vector3 position, Quaternion rotation, bool flip, Team team, uint ownerId) {
        GameObject attack = Instantiate(attacks[0], position, rotation);

        attack.GetComponent<SpriteRenderer>().flipY = flip;
        attack.GetComponent<HitboxController>().Init(null, ownerId, team);

        NetworkServer.Spawn(attack);
    }

    [Command(requiresAuthority = false)] public void CreateProjectile() {

    }

    public void RegisterEntity(uint id, GameObject entity) {
        syncedEntities.Add(id, entity);
    }

    public GameObject GetRegisteredEntity(uint id) {
        return syncedEntities[id];
    }
}
