using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class StatController : NetworkBehaviour
{
    [SerializeField] private Team team;

    public bool OnHit(HitData data, StatController source) {
        PlayerController player;

        if ((gameObject.TryGetComponent(out player) || source.TryGetComponent(out player)) && !player.isLocalPlayer)
            return false;
        
        VFXManager.Instance.CreateFloatingText("5", Color.red, transform.position + 1.5f * Vector3.up);

        return true;
    }

    public Team GetTeam() {return team;}
}
