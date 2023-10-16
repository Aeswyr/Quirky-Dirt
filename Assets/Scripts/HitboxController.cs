using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class HitboxController : NetworkBehaviour
{
    private uint ownerId;
    private GameObject owner;
    private Team team;
    private List<StatController> hitTargets = new();
    private HitData data;
    public void Init(HitData data, uint ownerId, Team team) {
        this.data = data;
        this.ownerId = ownerId;
        this.team = team;
    }

    void Start() {
        this.owner = GameManager.Instance.GetRegisteredEntity(ownerId);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // team check
        StartCoroutine(OnHit(other));
    }

    IEnumerator OnHit(Collider2D other) {
        yield return new WaitUntil(() => owner != null);

        if (other.transform.parent.gameObject != owner && other.transform.parent.TryGetComponent(out StatController stats) && stats.GetTeam() != team && !hitTargets.Contains(stats)) {
            stats.OnHit(data, owner.GetComponent<StatController>());
            hitTargets.Add(stats);
        }
    }
}

public enum Team {
    DEFAULT, PLAYER, ENEMY
}
