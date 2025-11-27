using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Attacking
}
public class NPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Wandering")]
    public float walkSpeed = 2f;
    public float minWanderDist = 3f;
    public float maxWanderDist = 10f;
    public float minWait = 1f;
    public float maxWait = 3f;

    [Header("Combat")]
    public float detectDistance = 6f;   // 플레이어 감지 거리
    public float attackDistance = 2f;   // 공격 거리
    public float attackRate = 1f;
    public int damage = 10;
    private float lastAttackTime;

    private AIState state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        agent.speed = walkSpeed;
        SetState(AIState.Wandering);
        WanderToNewSpot();
    }

    private void Update()
    {
        Transform player = CharacterManager.Instance.Player.transform;
        float dist = Vector3.Distance(transform.position, player.position);

        // 모든 상태에서 공격 거리 체크
        if (dist < attackDistance)
        {
            SetState(AIState.Attacking);
        }
        else if (dist > detectDistance && state == AIState.Attacking)
        {
            // 멀어지면 Wandering 복귀
            SetState(AIState.Wandering);
            WanderToNewSpot();
        }

        // FSM
        switch (state)
        {
            case AIState.Wandering:
                if (agent.remainingDistance < 0.3f)
                {
                    SetState(AIState.Idle);
                    Invoke(nameof(WanderToNewSpot), Random.Range(minWait, maxWait));
                }
                break;

            case AIState.Attacking:
                Attack(player);
                break;
        }

        animator.SetBool("Moving", agent.velocity.magnitude > 0.1f);
    }

    void WanderToNewSpot()
    {
        if (state != AIState.Idle && state != AIState.Wandering) return;

        SetState(AIState.Wandering);

        Vector3 randomPos = transform.position + Random.insideUnitSphere * Random.Range(minWanderDist, maxWanderDist);

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, maxWanderDist, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void Attack(Transform player)
    {
        agent.isStopped = true;

        // 플레이어 바라보기
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (Time.time - lastAttackTime > attackRate)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            // 플레이어 데미지
           
        }
    }

    void SetState(AIState s)
    {
        state = s;

        switch (s)
        {
            case AIState.Wandering:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                break;

            case AIState.Idle:
                agent.isStopped = true;
                break;

            case AIState.Attacking:
                agent.isStopped = true;
                break;
        }
    }
}
