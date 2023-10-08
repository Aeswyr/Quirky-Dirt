using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{ 

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float baseSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isLocalPlayer)
            return;
            
        rbody.velocity = baseSpeed * InputHandler.Instance.dir;

        if (InputHandler.Instance.dir.x != 0)
            sprite.flipX = InputHandler.Instance.dir.x < 0;

        



        if (InputHandler.Instance.pause.pressed)
            Application.Quit();
    }
}
