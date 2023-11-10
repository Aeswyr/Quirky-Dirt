using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{ 

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private StatController stats;
    [SerializeField] private BoxCollider2D pickupBox;
    


    [Header("Stats")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private AnimationCurve rollOverrideWeight;
    [SerializeField] private float redirectGraceWindow;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private AnimationCurve decelerationCurve;


    [SerializeField] private AnimationCurve attackCurve;
    [SerializeField] private AnimationCurve attackOverrideWeight;

    [SerializeField] private int[] leftCombo;
    [SerializeField] private int[] rightCombo;

    private enum ComboList {
        DEFAULT, LEFT, RIGHT
    }
    private ComboList currentCombo;
    private int comboIndex;
    [SyncVar(hook = nameof(SyncFacing))] bool flipX;


    private bool acting = false;
    private Vector2 lastDir;
    private float curveStartTime;
    private AnimationCurve activeCurve = null;
    private AnimationCurve controlOverrideWeight;
    private float redirectTimer;
    private float walkMotionTime;
    private int attackID = -1;
    bool charging;
    float chargeStart;
    bool locked = false;
    bool inventoryOpen, pauseOpen;

    private Vector2 mouseDir;
    Interactable nearestInteractable;

    bool cancellable, rollCancellable;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterEntity(netId, gameObject);

        if (!isLocalPlayer)
            return;

        var cam = FindObjectOfType<CinemachineVirtualCamera>();
        cam.Follow = transform;

        InventoryManager.Instance.SetActive(inventoryOpen);
        PauseManager.Instance.SetActive(pauseOpen);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isLocalPlayer)
            return;

        if (!pauseOpen && InputHandler.Instance.menu.pressed) {
            inventoryOpen = !inventoryOpen;
            locked = inventoryOpen;
            InventoryManager.Instance.SetActive(inventoryOpen);
        }

        if (InputHandler.Instance.pause.pressed) {
            if (inventoryOpen) {
                inventoryOpen = false;
                locked = inventoryOpen;
                InventoryManager.Instance.SetActive(inventoryOpen);
            } else {
                pauseOpen = !pauseOpen;
                locked = pauseOpen;
                PauseManager.Instance.SetActive(pauseOpen);
            }
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

        if ((!acting || (cancellable && rollCancellable) || charging) && InputHandler.Instance.dodge.pressed) {
            ToAction();
            animator.SetTrigger("dodge");

            activeCurve = rollCurve;
            controlOverrideWeight = rollOverrideWeight;
            curveStartTime = Time.time;
        } else if (TryAttack()) {}

        if (nearestInteractable != null && InputHandler.Instance.interact.pressed) {
            nearestInteractable.Trigger(this);
        }
            
    }
    private bool GetComboRelease(ComboList combo) {
        return (combo == ComboList.LEFT && !InputHandler.Instance.primary.down)
            || (combo == ComboList.RIGHT && !InputHandler.Instance.secondary.down);
    }

    private bool GetComboPressed(out ComboList combo) {

        combo = ComboList.DEFAULT;
        if (InputHandler.Instance.primary.pressed) {
            combo = ComboList.LEFT;
            return true;
        }

        if (InputHandler.Instance.secondary.pressed) {
            combo = ComboList.RIGHT;
            return true;
        }

        return false;
    }

    private bool IsNotChargeAttack(int id) {
        return id != 2;
    }
    private bool TryAttack() {
        if ((!acting || cancellable) && GetComboPressed(out ComboList nextCombo)) {
            if (currentCombo != nextCombo) {
                comboIndex = 0;
            } else {
                comboIndex++;
            }

            currentCombo = nextCombo;

            int[] comboList = leftCombo;
            if (currentCombo == ComboList.RIGHT)
                comboList = rightCombo;

            attackID = comboList[comboIndex];

            if (IsNotChargeAttack(attackID)) {
                ToAction(false);
                animator.SetInteger("attackID", attackID);
                animator.SetTrigger("attack");

                lastDir = mouseDir;

                activeCurve = attackCurve;
                controlOverrideWeight = attackOverrideWeight;
                curveStartTime = Time.time;

                UpdateFacing(true);

                return true;
            } else {
                ToAction(false);

                charging = true;
                chargeStart = Time.time;
                animator.SetInteger("followup", 0);
                animator.SetInteger("attackID", attackID);
                animator.SetTrigger("attack");

                activeCurve = decelerationCurve;
                controlOverrideWeight = null;
                curveStartTime = Time.time;

                UpdateFacing(true);

                return true;
            }
        } else if (charging && Time.time - chargeStart > 0.2 && GetComboRelease(currentCombo)) {
            ToAction(false);

            animator.SetInteger("followup", 1);
            animator.SetInteger("attackID", attackID);
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
        currentCombo = ComboList.DEFAULT;
    }

    public void SetCancellable() {
        if (!isLocalPlayer)
            return;


        if ((currentCombo == ComboList.LEFT && comboIndex + 1 < leftCombo.Length) 
            || (currentCombo == ComboList.RIGHT && comboIndex + 1 < rightCombo.Length)) {
            cancellable = true;
            rollCancellable = true;
        }
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
        var attack = GameManager.Instance.CreateAttack(Team.PLAYER, netId);
        switch (attackID) {
            case 0:
            case 1:
                bool flip = mouseDir.x >= 0 ? attackID == 0 : attackID == 1;
                attack
                    .SetType(AttackType.DEFAULT)
                    .SetHitboxSize(new Vector2(1.5f, 1))
                    .SetHitboxOffset(new Vector2(0, 0.0625f))
                    .SetPosition(transform.position + 0.5f * Vector3.up + rotation * (1.25f * Vector2.right))
                    .SetRotation(rotation)
                    .SetFlip(flip)
                    .Finish();
                break;
            case 2:
                VFXManager.Instance.CreateVFX(VFXType.VFX_SHOOT, transform.position + 0.5f * Vector3.up + rotation * (2f * Vector2.right), rotation);
                attack
                    .SetType(AttackType.ARROW)
                    .SetHitboxSize(new Vector2(0.5f, 0.25f))
                    .SetPosition(transform.position + 0.5f * Vector3.up + rotation * (1.25f * Vector2.right))
                    .SetRotation(rotation)
                    .SetVelocity(rotation * Vector2.right, 30)
                    .SetLifetime(10)
                    .EnableDestroyOnHit()
                    .EnableDestroyOnWall()
                    .Finish();
                break;
            case 3:
                attack
                    .SetType(AttackType.FLASH_CUT)
                    .SetHitboxSize(new Vector2(3.5f, 1))
                    .SetHitboxOffset(new Vector2(0, 0.0625f))
                    .SetPosition(transform.position + 0.5f * Vector3.up + rotation * (2.75f * Vector2.right))
                    .SetRotation(rotation)
                    .Finish();
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

    public void SetNearestInteractable(Interactable nearest) {
        nearestInteractable = nearest;
    }

    public Interactable GetNearestInteractable() {
        return nearestInteractable;
    }
}
