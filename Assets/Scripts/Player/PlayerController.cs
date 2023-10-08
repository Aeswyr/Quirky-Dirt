using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{ 

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float baseSpeed;



    [SyncVar(hook = nameof(UpdateFacing))] bool flipX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isLocalPlayer)
            return;
            
        rbody.velocity = baseSpeed * InputHandler.Instance.dir;

        bool facing = InputHandler.Instance.dir.x < 0;
        if (InputHandler.Instance.dir.x != 0 && flipX != facing) {
            sprite.flipX = facing;
            SyncFacing(facing);
        }

        



        if (InputHandler.Instance.pause.down)
            Application.Quit();
    }

    private void UpdateFacing(bool oldFacing, bool newFacing) {
        sprite.flipX = newFacing;
    }

    [Command] public void SyncFacing(bool facing) {
        flipX = facing;
    }
}
