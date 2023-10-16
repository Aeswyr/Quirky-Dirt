using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{ 

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private StatController stats;
    


    [Header("Stats")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private AnimationCurve rollOverrideWeight;
    [SerializeField] private float redirectGraceWindow;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private AnimationCurve decelerationCurve;


    [SerializeField] private AnimationCurve attackCurve;
    [SerializeField] private AnimationCurve attackOverrideWeight;

    [SyncVar(hook = nameof(SyncFacing))] bool flipX;


    private bool acting = false;
    private Vector2 lastDir;
    private float curveStartTime;
    private AnimationCurve activeCurve = null;
    private AnimationCurve controlOverrideWeight;
    private float redirectTimer;
    private float walkMotionTime;
    private int attackID = -1;
    private Vector2 mouseDir;

    bool cancellable, rollCancellable;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterEntity(netId, gameObject);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isLocalPlayer)
            return;

        mouseDir = (Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos) - transform.position).normalized;

        if (acting && Time.time < redirectTimer && InputHandler.Instance.dir != Vector2.zero) {
            lastDir = InputHandler.Instance.dir;
            UpdateFacing();
        }

        if (!acting && InputHandler.Instance.move.released)
            walkMotionTime = Time.time;
        
        if (!acting && InputHandler.Instance.move.down) {
            animator.SetBool("running", true);
            activeCurve = null;
            if (lastDir == Vector2.zero)
                walkMotionTime = Time.time;
            lastDir = InputHandler.Instance.dir;
            rbody.velocity = accelerationCurve.Evaluate(Time.time - walkMotionTime) * baseSpeed * lastDir;
            UpdateFacing();
        } else if (activeCurve != null) {
            float controlOverride = controlOverrideWeight == null ? 0 : controlOverrideWeight.Evaluate(Time.time - curveStartTime);
            Vector2 avgDir = (1 - controlOverride) * lastDir + controlOverride * InputHandler.Instance.dir;
            rbody.velocity =  activeCurve.Evaluate(Time.time - curveStartTime) * baseSpeed * avgDir;
        } else {
            animator.SetBool("running", false);
            rbody.velocity = decelerationCurve.Evaluate(Time.time - walkMotionTime) * baseSpeed * lastDir;
        }

        if ((!acting || (cancellable && rollCancellable)) && InputHandler.Instance.dodge.pressed) {
            ToAction();
            animator.SetTrigger("dodge");

            activeCurve = rollCurve;
            controlOverrideWeight = rollOverrideWeight;
            curveStartTime = Time.time;
        } else if ((!acting || cancellable) && InputHandler.Instance.primary.pressed) {
            attackID = (attackID + 1) % 2;

            ToAction(false);
            animator.SetInteger("attackID", attackID);
            animator.SetTrigger("attack");

            lastDir = mouseDir;

            activeCurve = attackCurve;
            controlOverrideWeight = attackOverrideWeight;
            curveStartTime = Time.time;

            UpdateFacing(true);
        }

        

        



        if (InputHandler.Instance.pause.pressed)
            Application.Quit();
    }

    public void ToAction(bool canRedirect = true) {
        animator.SetBool("running", false);

        acting = true;
        if (canRedirect)
            redirectTimer = Time.time + redirectGraceWindow;
        else 
            redirectTimer = 0;

        cancellable = false;
    }

    public void FromAction() {
        if (!isLocalPlayer)
            return;
        activeCurve = null;
        controlOverrideWeight = null;
        acting = false;

        cancellable = false;
        rollCancellable = false;
        attackID = -1;
    }

    public void SetCancellable() {
        if (!isLocalPlayer)
            return;

        cancellable = true;
        rollCancellable = true;
    }

     public void SetAttackCancellable() {
        if (!isLocalPlayer)
            return;

        cancellable = true;
        rollCancellable = false;
    }

    public void SpawnHitbox() {
        if (!isLocalPlayer)
            return;
        Quaternion rotation = Quaternion.FromToRotation(Vector2.right, mouseDir);
        bool flip = mouseDir.x >= 0 ? attackID == 0 : attackID == 1;
        GameManager.Instance.CreateAttack(transform.position + 0.5f * Vector3.up + rotation * (1.25f * Vector2.right), rotation, flip, Team.PLAYER, netId);
    }

    public void UpdateFacing(bool mouseOverride = false) {
        bool facing = mouseOverride ? 
            Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos).x < transform.position.x : InputHandler.Instance.dir.x < 0;
        if ((InputHandler.Instance.dir.x != 0 || mouseOverride) && flipX != facing) {
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
