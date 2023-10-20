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
        NetworkServer.Spawn(attack);
        
        attack.GetComponent<HitboxController>().Init(null, ownerId, team);
    }

    [Command(requiresAuthority = false)] public void CreateProjectile(Vector3 position, Quaternion rotation, float speed, Team team, uint ownerId) {
        GameObject attack = Instantiate(attacks[1], position, rotation);

        attack.GetComponent<Rigidbody2D>().velocity = speed * (rotation * Vector3.right);
        NetworkServer.Spawn(attack);
        
        attack.GetComponent<HitboxController>().Init(null, ownerId, team);
    }

    public void RegisterEntity(uint id, GameObject entity) {
        syncedEntities.Add(id, entity);
    }

    public GameObject GetRegisteredEntity(uint id) {
        return syncedEntities[id];
    }
}
