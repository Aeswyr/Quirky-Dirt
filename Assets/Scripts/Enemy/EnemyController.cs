using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float baseSpeed;
    [SerializeField] private float searchDistance;
    [SerializeField] private AnimationCurve knockbackCurve;
    [SerializeField] private List<EnemyState> validStates;
    
    public Dictionary<Transform, float> aggroList = new();

    private EnemyState behaviorState;

    private Transform target;
    private Vector3 targetPosition;
    private Vector3 home;

    private float knockbackTime = -1;
    private float knockbackDuration = 0;
    private bool isKnockback;
    private Vector2 knockbackVector;

    private int facing = 1;

    private float actingLockout;

    public bool Acting {
        get;
        private set;
    }

    public enum EnemyState {
        idle, wander, search, chase
    }

    int varianceDelta, nextVariance;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterEntity(netId, gameObject);

        if (!isServer)
            return;

        if (GoToState(EnemyState.wander) || GoToState(EnemyState.idle)) {}

        NewVariance();

        home = transform.position;
        targetPosition = home;
    }

    private void NewVariance() {
        nextVariance = 0;
        varianceDelta = Random.Range(15, 45);
        offset = Quaternion.Euler(0, 0, Random.Range(0, 360)) * new Vector3(Random.Range(0.25f, 1.5f), 0, 0);
    }

    private void NewWander() {
        targetPosition = home + Quaternion.Euler(0, 0, Random.Range(0, 360)) * new Vector3(Random.Range(0, 6f), 0, 0);
        nextVariance = 0;
        varianceDelta = Random.Range(90, 180) + Mathf.CeilToInt(60 * Vector3.Distance(targetPosition, transform.position) / (baseSpeed * 0.75f));
    }

    void FixedUpdate() {
        if (!isServer)
            return;

        if (Time.time < knockbackTime + knockbackDuration) {
            animator.SetBool("hit", true);
            animator.SetBool("move", false);
            rbody.velocity = knockbackCurve.Evaluate(Time.time - knockbackTime) * knockbackVector;
            return;
        } else if (isKnockback) {
            animator.SetBool("hit", false);
            rbody.velocity = Vector2.zero;
            isKnockback = false;
        }

        if (Acting)
            return;
        
        target = FindTarget();

        

        switch (behaviorState) {
            case EnemyState.idle:
                Idle();
                break;
            case EnemyState.wander:
                Wander();
                break;
            case EnemyState.chase:
                Chase();
                break;
        }        
    }

    public void Idle() {
        if (target != null) {
            GoToState(EnemyState.chase);
            return;
        }
    }

    public void Wander() {
        if (target != null) {
            GoToState(EnemyState.chase);
            return;
        }

        nextVariance++;

        if (nextVariance > varianceDelta)
            NewWander();

        GoToPosition(targetPosition, baseSpeed * 0.75f);
    }

    public void Chase() {
        if (target == null) {
            if (    GoToState(EnemyState.search)
                ||  GoToState(EnemyState.wander)
                ||  GoToState(EnemyState.idle))
                return;
        }

        nextVariance++;

        if (nextVariance > varianceDelta)
            NewVariance();


        Vector3 targetPosition = target.position + offset;
        GoToPosition(targetPosition, baseSpeed);
    }
    
    private void GoToPosition(Vector3 pos, float speed) {
        if (Vector3.Distance(transform.position, pos) < 0.1) {

            rbody.velocity = Vector2.zero;
            animator.SetBool("move", false);

            if (target != null) {
                SetFacing((int)Mathf.Sign(target.position.x - transform.position.x));
            }

        } else {

            rbody.velocity = speed * (pos - transform.position).normalized;
            animator.SetBool("move", true);

            SetFacing((int)Mathf.Sign(rbody.velocity.x));

        }
    }
    private bool GoToState(EnemyState state) {
        if (validStates.Contains(state)) {
            this.behaviorState = state;
            return true;
        }
        return false;
    }

    private Transform FindTarget() {
        if (aggroList.Count > 0) {
            Transform target = null;
            float aggro = float.MinValue;
            foreach (var pair in aggroList) {
                if (pair.Value > aggro) {
                    target = pair.Key;
                    aggro = pair.Value;
                }
            }
            return target;
        } else {
            var players = FindObjectsOfType<PlayerController>();
            foreach (var player in players) {
                if (Vector2.Distance(player.transform.position, transform.position) < searchDistance) {
                    AdjustAggro(player, 10);
                    return player.transform;
                }
            }
        }

        return null;
    }

    private void SetFacing(int dir) {
        if (facing != dir) {
            facing = dir;
            SendFacing(facing < 0);
        }

        [ClientRpc] void SendFacing(bool facing) {
            sprite.flipX = facing;
        }
    }

    [Command(requiresAuthority = false)] public void AddAggro(PlayerController source, float aggro) {
        AdjustAggro(source, aggro);
    }

    private void AdjustAggro(PlayerController source, float aggro) {
        var transform = source.transform;

        if (aggroList.ContainsKey(transform)) 
            aggroList[transform] += aggro;
        else
            aggroList.Add(transform, aggro);
    }

    [Command(requiresAuthority = false)] public void DoKnockback(HitData.KnockbackStrength strength, Vector2 dir) {
        EndAction();

        knockbackTime = Time.time;
        isKnockback = true;
        SetFacing((int)Mathf.Sign(-dir.x));
        switch (strength) {
                case HitData.KnockbackStrength.SMALL:
                    knockbackDuration = 0.2f;
                    knockbackVector = 2 * dir;
                    break;
                case HitData.KnockbackStrength.MEDIUM:
                    knockbackDuration = 0.35f;
                    knockbackVector = 4 * dir;
                    break;
                case HitData.KnockbackStrength.LARGE:
                    knockbackDuration = 0.5f;
                    knockbackVector = 8 * dir;
                    break;
                case HitData.KnockbackStrength.MASSIVE:
                    knockbackDuration = 0.5f;
                    knockbackVector = 16 * dir;
                    break;
            }
        
    }

    public Transform GetCurrentTarget() {
        return target;
    }

    public bool GetActionable() {
        return !isKnockback && !Acting;
    }

    public void StartAction() {
        animator.SetBool("move", false);

        Acting = true;
    }

    public void EndAction() {
        Acting = false;
    }
}
