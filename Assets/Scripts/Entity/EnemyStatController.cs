using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyStatController : StatController
{
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 10;
        curHP = maxHP;
    }

    public override void OnDeath() {
        NetworkServer.Destroy(gameObject);
    }
}
