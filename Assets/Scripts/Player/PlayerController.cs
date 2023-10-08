using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{ 

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    


    [Header("Stats")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private AnimationCurve controlOverrideWeight;
    [SerializeField] private float redirectGraceWindow;


    [SyncVar(hook = nameof(SyncFacing))] bool flipX;


    private bool acting = false;
    private Vector2 lastDir;
    private float curveStartTime;
    private AnimationCurve activeCurve = null;
    private float redirectTimer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isLocalPlayer)
            return;

        if (acting && Time.time < redirectTimer) {
            lastDir = InputHandler.Instance.dir;
            UpdateFacing();
        }
        
        if (!acting && InputHandler.Instance.move.down) {
            lastDir = InputHandler.Instance.dir;
            rbody.velocity = baseSpeed * lastDir;
            
            UpdateFacing();
        } else if (activeCurve != null) {
            float controlOverride = controlOverrideWeight.Evaluate(Time.time - curveStartTime);
            Vector2 avgDir = (1 - controlOverride) * lastDir + controlOverride * InputHandler.Instance.dir;
            rbody.velocity =  activeCurve.Evaluate(Time.time - curveStartTime) * baseSpeed * avgDir;
        } else
            rbody.velocity = Vector2.zero;
        
        if (!acting && InputHandler.Instance.dodge.pressed) {
            animator.SetTrigger("dodge");

            ToAction();
            activeCurve = rollCurve;
            curveStartTime = Time.time;
        }
        

        



        if (InputHandler.Instance.pause.pressed)
            Application.Quit();
    }

    public void ToAction() {
        acting = true;
        redirectTimer = Time.time + redirectGraceWindow;
    }

    public void FromAction() {
        if (!isLocalPlayer)
            return;
        activeCurve = null;
        acting = false;
    }

    public void UpdateFacing() {
        bool facing = InputHandler.Instance.dir.x < 0;
        if (InputHandler.Instance.dir.x != 0 && flipX != facing) {
            sprite.flipX = facing;
            SendFacing(facing);
        }
    }

    private void SyncFacing(bool oldFacing, bool newFacing) {
        sprite.flipX = newFacing;
    }

    [Command] public void SendFacing(bool facing) {
        flipX = facing;
    }
}
