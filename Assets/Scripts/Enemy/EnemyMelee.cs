using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyMelee : NetworkBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rbody;

    private float nextAttack;
    private Vector3 targetDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isServer)
            return;

        Transform target = controller.GetCurrentTarget();
        if (controller.GetActionable() && target != null && Time.time > nextAttack) {
            controller.StartAction();
            animator.SetTrigger("action0");
            

            targetDirection = (target.position - transform.position).normalized;

            nextAttack = Time.time + 3f;


            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, targetDirection);
            var attack = GameManager.Instance.CreateAttack(0, Team.ENEMY, netId);
            
            attack
                .UseEnemyAttacks()
                .SetType(AttackType.IMPACT)
                .SetHitboxSize(new Vector2(1f, 0.5f))
                .SetHitboxOffset(new Vector2(0, 0.0625f))
                .SetPosition(transform.position + 0.5f * Vector3.up + rotation * new Vector2(1f, Random.Range(-0.6f, 0.6f)))
                .SetRotation(rotation)
                .Finish();
        }

        if (controller.Acting) {
            rbody.velocity = 15 * targetDirection;
        }
    }
}
