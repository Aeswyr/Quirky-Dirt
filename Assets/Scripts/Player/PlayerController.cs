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
    private int attackType = 0;
    bool charging;
    float chargeStart;
    bool locked = false;

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

        if (InputHandler.Instance.menu.pressed) {
            locked = !locked;
            InventoryManager.Instance.SetActive(locked);
        }

        if (locked)
            return;

        mouseDir = (Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos) - transform.position).normalized;

        // facing override for redirects
        if (acting && Time.time < redirectTimer && InputHandler.Instance.dir != Vector2.zero) {
            lastDir = InputHandler.Instance.dir;
            UpdateFacing();
        }

        //facing override for charges
        if (charging) {
            UpdateFacing(true);
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

        if (!acting && InputHandler.Instance.skill1.pressed) {
            attackType = (attackType + 1) % 2;
            attackID = -1;
        }

        if ((!acting || (cancellable && rollCancellable) || charging) && InputHandler.Instance.dodge.pressed) {
            ToAction();
            animator.SetTrigger("dodge");

            activeCurve = rollCurve;
            controlOverrideWeight = rollOverrideWeight;
            curveStartTime = Time.time;
        } else if (TryAttack()) {}

        if (InputHandler.Instance.pause.pressed)
            Application.Quit();
    }

    private bool TryAttack() {
        if ((!acting || cancellable) && InputHandler.Instance.primary.pressed) {
            if (attackType == 0) {
                attackID = (attackID + 1) % 2;

                ToAction(false);
                animator.SetInteger("attackID", attackID);
                animator.SetTrigger("attack");

                lastDir = mouseDir;

                activeCurve = attackCurve;
                controlOverrideWeight = attackOverrideWeight;
                curveStartTime = Time.time;

                UpdateFacing(true);

                return true;
            } else if (attackType == 1) {
                ToAction(false);

                charging = true;
                chargeStart = Time.time;
                animator.SetInteger("attackID", 2);
                animator.SetTrigger("attack");

                activeCurve = decelerationCurve;
                controlOverrideWeight = null;
                curveStartTime = Time.time;

                UpdateFacing(true);

                return true;
            }
        } else if (charging && Time.time - chargeStart > 0.2 && !InputHandler.Instance.primary.down) {
            ToAction(false);

            animator.SetInteger("attackID", 3);
            animator.SetTrigger("attack");

            lastDir = mouseDir * -1;

            activeCurve = decelerationCurve;
            controlOverrideWeight = null;
            curveStartTime = Time.time;

            UpdateFacing(true);
            return true;
        }

        return false;
    }

    public void ToAction(bool canRedirect = true) {
        animator.SetBool("running", false);

        acting = true;
        if (canRedirect)
            redirectTimer = Time.time + redirectGraceWindow;
        else 
            redirectTimer = 0;

        cancellable = false;
        charging = false;
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

    public void SpawnAttack() {
        if (!isLocalPlayer)
            return;
        Quaternion rotation = Quaternion.FromToRotation(Vector2.right, mouseDir);
        switch (attackType) {
            case 0:
                bool flip = mouseDir.x >= 0 ? attackID == 0 : attackID == 1;
                GameManager.Instance.CreateAttack(transform.position + 0.5f * Vector3.up + rotation * (1.25f * Vector2.right), rotation, flip, Team.PLAYER, netId);
                break;
            case 1:
                VFXManager.Instance.CreateVFX(VFXType.VFX_SHOOT, transform.position + 0.5f * Vector3.up + rotation * (2f * Vector2.right), rotation);
                GameManager.Instance.CreateProjectile(transform.position + 0.5f * Vector3.up + rotation * (1.25f * Vector2.right), rotation, 30, Team.PLAYER, netId);
                break;
        }
    }

    public void UpdateFacing(bool mouseOverride = false) {
        bool facing = mouseOverride ? 
            Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos).x < transform.position.x : InputHandler.Instance.dir.x < 0;
        if ((InputHandler.Instance.dir.x != 0 || mouseOverride) && flipX != facing) {
            sprite.flipX = facing;
            SendFacing(facing);
        }
    }

    private void ResetCurves() {
        activeCurve = null;
        controlOverrideWeight = null;
        curveStartTime = 0;
    }

    private void SyncFacing(bool oldFacing, bool newFacing) {
        sprite.flipX = newFacing;
    }

    [Command] public void SendFacing(bool facing) {
        flipX = facing;
    }
}
