using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bear : MonoBehaviour
{
    public enum State { idle, patrol, chase, goBack, attack, die }
    [Header("상태")]
    public State state = State.idle;

    [Header("컴포넌트 변수")]
    public Animator anim;
    public Collider col;
    public NavMeshAgent agent;
    public BearHealth health;

    [Header("공격 변수")]
    public float damage;
    public float attackSpd;
    public bool attacking;
    public GameObject attackPoint;

    [Header("WayPoint 지정 변수")]
    public int nextIdx;
    public List<Vector3> wayPoint;

    [Header("NavMeshAgent 변수")]
    public float moveSpeed;
    public float runSpeed;
    public float damping;

    [Header("범위 변수")]
    public float chaseRange;
    public float attackDist;

    [Header("상태 bool 변수")]
    public bool isCombat;
    public bool isIdle;
    public bool isPatrol;
    public bool isChase;
    public bool isReturn;
    public bool isAttack;
    public bool isDie;

    [Header("플레이어 변수")]
    public LayerMask playerMask;
    public GameObject player;

    WaitForSeconds ws = new WaitForSeconds(0.3f);
    IEnumerator actionRoutine;
    IEnumerator checkRoutine;
    IEnumerator standbyRoutine;
    IEnumerator attackRoutine;

    void Start()
    {
        agent.autoBraking = false;
        agent.updateRotation = false;

        actionRoutine = Action();
        checkRoutine = CheckState();

        SetWayPoint();
        StartCoroutine(actionRoutine);
        StartCoroutine(checkRoutine);
    }

    void Update()
    {
        anim.SetBool("IsCombat", isCombat);

        if (!agent.isStopped && state != State.idle)
        {
            Quaternion rot = Quaternion.LookRotation(agent.desiredVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * damping);
        }

        if (state == State.attack)
        {
            transform.LookAt(player.transform);
        }

        if (!isPatrol) return;

        if (agent.velocity.sqrMagnitude >= 0.2f * 0.2f && agent.remainingDistance <= 0.5f)
        {
            state = State.idle;
            nextIdx = Random.Range(0, wayPoint.Count);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 obj in wayPoint)
            Gizmos.DrawSphere(obj, 1f);

        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.DrawWireSphere(transform.position, attackDist);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, agent.desiredVelocity);
    }

    // 무작위 순찰 포인트 지정
    void SetWayPoint()
    {
        wayPoint = new List<Vector3>();

        for (int i = 0; i < 4; i++)
        {
            int xPoint = Random.Range(-5, 5);
            int zPoint = Random.Range(-5, 5);
            Vector3 pos = transform.position + new Vector3(xPoint, 0, zPoint);
            wayPoint.Add(pos);
        }
        nextIdx = Random.Range(0, wayPoint.Count);
    }
    

    #region [상태별 행동]
    // 대기
    IEnumerator Standby()
    {
        AgentStop();

        isCombat = false;
        isIdle = true;
        isPatrol = false;
        isChase = false;
        isReturn = false;

        anim.SetBool("IsPatrol", false);
        anim.SetBool("IsChase", false);

        yield return new WaitForSeconds(3f);

        isIdle = false;
        if (state != State.chase)
            state = State.patrol;
    }

    // 순찰
    void Patrol()
    {
        if (agent.isPathStale) return;

        AgentActive(moveSpeed, 1f, wayPoint[nextIdx]);

        isPatrol = true;
        isReturn = false;

        anim.SetBool("IsPatrol", true);
        anim.SetBool("IsChase", false);
    }

    // 추적
    void Chase()
    {
        if (agent.isPathStale) return;
        if (isReturn) return;

        if (standbyRoutine != null)
            StopCoroutine(standbyRoutine);
        AgentActive(runSpeed, 7f, player.transform.position);

        isCombat = true;
        isPatrol = false;
        isChase = true;

        anim.SetBool("IsChase", true);
        anim.SetBool("IsPatrol", false);
    }

    // 복귀
    void Return()
    {
        if (agent.isPathStale) return;

        AgentActive(runSpeed, 7f, wayPoint[nextIdx]);

        isCombat = false;
        isChase = false;
        isReturn = true;

        anim.SetBool("IsChase", true);
        anim.SetBool("IsPatrol", false);
    }

    // 공격
    IEnumerator Attack()
    {
        AgentStop();

        isChase = false;
        isAttack = true;

        anim.SetBool("IsChase", false);
        anim.SetBool("IsPatrol", false);
        anim.SetTrigger("OnAttack");
        anim.SetInteger("AttackIdx", Random.Range(0, 4));

        yield return new WaitForSeconds(attackSpd);

        isAttack = false;
    }

    // 공격 포인트 활성화
    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }

    // 죽음
    void Die()
    {
        AgentStop();
        StopAllCoroutines();

        isCombat = false;
        isPatrol = false;
        isChase = false;
        isReturn = false;
        isAttack = false;
        isDie = true;

        anim.SetTrigger("IsDie");
        col.enabled = false;
    }
    #endregion

    #region [Nav 작동]
    // NavAgent 활성
    void AgentActive(float speed, float damp, Vector3 pos)
    {
        agent.speed = speed;
        damping = damp;
        agent.destination = pos;
        agent.isStopped = false;
    }

    // NavAgent 멈춤
    void AgentStop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    #endregion

    #region [상태 체크 코루틴]
    // 상태별 설정
    IEnumerator Action()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.idle:
                    if (!isIdle)
                    {
                        standbyRoutine = Standby();
                        StartCoroutine(standbyRoutine);
                        standbyRoutine = null;
                    }
                    break;
                case State.patrol:
                    Patrol();
                    break;
                case State.chase:
                    Chase();
                    break;
                case State.goBack:
                    Return();
                    break;
                case State.attack:
                    if (!isAttack)
                    {
                        attackRoutine = Attack();
                        StartCoroutine(attackRoutine);
                        attackRoutine = null;
                    }
                    break;
                case State.die:
                    Die();
                    break;
            }

            yield return ws;
        }
    }

    // 상태 체크
    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(1f);

        while (!isDie)
        {
            if (state == State.die) yield break;

            float playerDist = Vector3.Distance(player.transform.position, transform.position);
            float wayPointDist = Vector3.Distance(wayPoint[nextIdx], transform.position);
            Collider[] cols = Physics.OverlapSphere(transform.position, chaseRange, playerMask);
            if (playerDist < attackDist)
                state = State.attack;
            else if (cols.Length > 0)
                state = State.chase;
            else if (wayPointDist > 5f)
            {
                if (state != State.idle && state != State.patrol)
                    state = State.goBack;
            }
            else
            {
                if (state != State.idle)
                    state = State.patrol;
            }
            
            yield return ws;
        }
    }
    #endregion
}