using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterEntity(netId, gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
