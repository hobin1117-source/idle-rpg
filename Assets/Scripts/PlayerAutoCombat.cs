using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum State
    {
    Idle,
    Chase,
    Patrol,
    Attack
}

    

public class PlayerAutoCombat : MonoBehaviour
{
    public State currentState = State.Idle;
    public float detectRadius = 10f;      // 적 탐색 거리
    public float attackDistance = 2.5f;   // 공격 거리
    public float attackCooldown = 1.5f;   // 공격 속도
    public NavMeshAgent agent;
    public LayerMask enemyLayer;

    Transform targetEnemy;
    bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(State.Idle);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;

            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    // -------------------- FSM 상태 함수들 --------------------

    void Idle()
    {
        targetEnemy = FindEnemy();

        if (targetEnemy != null)
        {
            ChangeState(State.Chase);
            return;
        }

        // 자유 이동을 하고 싶다면(선택)
        // ChangeState(State.Patrol);
    }

    void Patrol()
    {
        // 자유 이동 로직 (필요하면 구현)
        targetEnemy = FindEnemy();
        if (targetEnemy != null)
        {
            ChangeState(State.Chase);
        }
    }

    void Chase()
    {
        if (targetEnemy == null)
        {
            ChangeState(State.Idle);
            return;
        }

        agent.SetDestination(targetEnemy.position);

        float dist = Vector3.Distance(transform.position, targetEnemy.position);

        if (dist <= attackDistance)
        {
            ChangeState(State.Attack);
        }
    }

    void Attack()
    {
        if (targetEnemy == null)
        {
            ChangeState(State.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, targetEnemy.position);
        if (dist > attackDistance)
        {
            ChangeState(State.Chase);
            return;
        }

        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // -------------------- 공격 코루틴 --------------------

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 여기서 애니메이션 재생
        Debug.Log("공격!");

        // 실제 데미지 전달
        // targetEnemy.GetComponent<Enemy>().TakeDamage(10);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // -------------------- 적 탐색 --------------------

    Transform FindEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRadius, enemyLayer);

        if (enemies.Length == 0)
            return null;

        // 가장 가까운 적 선택
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    // -------------------- 상태 전환 함수 --------------------

    void ChangeState(State newState)
    {
        currentState = newState;

        // 상태 전환 로그 (디버깅용)
        Debug.Log("State → " + newState);
    }
}
