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
    [ClientRpc] public void Init(HitData data, uint ownerId, Team team) {
        this.data = data;
        this.owner = GameManager.Instance.GetRegisteredEntity(ownerId);
        this.team = team;
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
}



public enum Team {
    DEFAULT, PLAYER, ENEMY
}
