using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum EnemyState
{
    Roam,
    Chase,
    Attack
}
public class EnemyAI : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Roam;
    public Transform player;
    public float chaseRange = 20f;      // 감지 범위 넓게
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float roamRadius = 5f;
    public float roamDelay = 3f;
    public float reactionDelay = 0.5f;  // 감지 후 잠시 대기

    private NavMeshAgent agent;
    private bool canAttack = true;
    private bool isReacting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FSM());
        StartCoroutine(RoamRoutine());
    }

    IEnumerator FSM()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            switch (currentState)
            {
                case EnemyState.Roam:
                    if (distance <= chaseRange && !isReacting)
                        StartCoroutine(StartChaseWithDelay());
                    break;

                case EnemyState.Chase:
                    agent.isStopped = false;
                    agent.SetDestination(player.position);

                    if (distance <= attackRange)
                        currentState = EnemyState.Attack;
                    else if (distance > chaseRange)
                        currentState = EnemyState.Roam;
                    break;

                case EnemyState.Attack:
                    agent.isStopped = true;

                    if (distance > attackRange)
                        currentState = EnemyState.Chase;
                    else if (canAttack)
                        StartCoroutine(PerformAttack());
                    break;
            }

            yield return null;
        }
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            if (currentState == EnemyState.Roam)
            {
                Vector3 roamPos = RandomNavMeshPoint(roamRadius);
                agent.SetDestination(roamPos);
            }
            yield return new WaitForSeconds(roamDelay);
        }
    }

    Vector3 RandomNavMeshPoint(float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    IEnumerator StartChaseWithDelay()
    {
        isReacting = true;
        yield return new WaitForSeconds(reactionDelay); // 잠시 대기
        currentState = EnemyState.Chase;
        isReacting = false;
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        Debug.Log("Attack!");
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
