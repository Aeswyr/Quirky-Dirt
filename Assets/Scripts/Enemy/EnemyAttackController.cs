using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;

public class EnemyAttackController : NetworkBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private List<EnemyAttack> attackList;
    private float[] attackCooldowns;
    private int currentAttackIndex;
    private float attackStartTime;
    private float attackDuration;

    private float nextAttack;
    private Vector3 targetDirection;

    // Start is called before the first frame update
    void Start()
    {
        attackCooldowns = new float[attackList.Count];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isServer)
            return;

        Transform target = controller.GetCurrentTarget();
        if (controller.GetActionable() && target != null && Time.time > nextAttack && TryGetNextAttack(out int index)) {
            controller.StartAction();
            animator.SetTrigger(attackList[index].animId);
            

            targetDirection = (target.position - transform.position).normalized;

            attackCooldowns[index] = Time.time + attackList[index].attackCooldown;
            nextAttack = Time.time + attackList[index].globalCooldown;
                
            currentAttackIndex = index;
            attackStartTime = Time.time;


            

        }

        if (controller.Acting) {
            rbody.velocity = 15 * attackList[currentAttackIndex].motionCurve.Evaluate(Time.time - attackStartTime) * targetDirection;
        }
    }

    private bool TryGetNextAttack(out int index) {
        index = -1;

        for (int i = 0; i < attackList.Count; i++) {
            if (Time.time > attackCooldowns[i]) {
                index = i;
                return true;
            }
        }

        return false;
    }

    public void SpawnAttack() {
            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, targetDirection);
            var attack = GameManager.Instance.CreateAttack(0, Team.ENEMY, netId);

            attack
                .UseEnemyAttacks()
                .SetType(AttackType.IMPACT)
                .SetHitboxSize(new Vector2(1f, 0.5f))
                .SetHitboxOffset(new Vector2(0, 0.0625f))
                .SetPosition(transform.position + 0.5f * Vector3.up + rotation * new Vector2(1f, UnityEngine.Random.Range(-0.6f, 0.6f)))
                .SetRotation(rotation)
                .Finish();
    }
}

[Serializable] public struct EnemyAttack {
    public string animId;
    public AnimationCurve motionCurve;
    public float attackCooldown;
    public float globalCooldown;
}
