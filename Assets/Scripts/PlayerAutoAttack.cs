using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum PlayerState
{
    Idle,    // 적 없음, 정지
    Chase,   // 적 추적
    Attack   // 공격
}
public class PlayerAutoAttack : MonoBehaviour
{
    public float detectRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    private NavMeshAgent agent;
    private Transform targetEnemy;
    private bool canAttack = true;

    public PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FSM());
    }

    IEnumerator FSM()
    {
        while (true)
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                    IdleState();
                    break;

                case PlayerState.Chase:
                    ChaseState();
                    break;

                case PlayerState.Attack:
                    AttackState();
                    break;
            }

            yield return null;
        }
    }

    void IdleState()
    {
        FindClosestEnemy();

        if (targetEnemy != null)
        {
            currentState = PlayerState.Chase;
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void ChaseState()
    {
        if (targetEnemy == null)
        {
            currentState = PlayerState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetEnemy.position);

        if (distance > detectRange)
        {
            targetEnemy = null;
            currentState = PlayerState.Idle;
        }
        else if (distance <= attackRange)
        {
            agent.isStopped = true;
            currentState = PlayerState.Attack;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(targetEnemy.position);
        }
    }

    void AttackState()
    {
        if (targetEnemy == null)
        {
            currentState = PlayerState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetEnemy.position);
        if (distance > attackRange)
        {
            currentState = PlayerState.Chase;
            return;
        }

        if (canAttack)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        Debug.Log("Player attacks " + targetEnemy.name);
        // TODO: 데미지 적용
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void FindClosestEnemy()
    {
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy.transform;
            }
        }

        if (minDistance <= detectRange)
            targetEnemy = closest;
        else
            targetEnemy = null;
    }
}
