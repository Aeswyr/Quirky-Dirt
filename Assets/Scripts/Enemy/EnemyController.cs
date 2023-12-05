using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterEntity(netId, gameObject);
    }

    void FixedUpdate() {
        
    }
}
