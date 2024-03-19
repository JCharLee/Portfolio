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
    public float patrolSpeed;
    public float chaseSpeed;
    public float damping;

    [Header("범위 변수")]
    public float chaseRange;
    public float attackDist;

    [Header("상태 bool 변수")]
    public bool isCombat;
    public bool isPatrol;
    public bool isChase;
    public bool isReturn;
    public bool isAttack;
    public bool isDie;

    [Header("플레이어 변수")]
    public LayerMask playerMask;
    public GameObject player;
    

    void Start()
    {
        agent.autoBraking = false;
        agent.updateRotation = false;

        SetWayPoint();
        StartCoroutine(Action());
        StartCoroutine(CheckState());
    }

    void Update()
    {
        anim.SetBool("IsCombat", isCombat);

        if (!agent.isStopped && state != State.idle)
        {
            Quaternion rot = Quaternion.LookRotation(agent.desiredVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * damping);
        }

        if (!isPatrol) return;

        if (agent.velocity.sqrMagnitude >= 0.2f * 0.2f && agent.remainingDistance <= 0.5f)
        {
            state = State.idle;
            nextIdx = Random.Range(0, wayPoint.Count);
            Patrol();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 obj in wayPoint)
            Gizmos.DrawSphere(obj, 1f);

        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.DrawWireSphere(transform.position, attackDist);
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
    // 순찰
    void Patrol()
    {
        if (agent.isPathStale) return;

        isPatrol = true;
        agent.speed = patrolSpeed;
        damping = 1f;
        agent.destination = wayPoint[nextIdx];
        agent.isStopped = false;
    }

    // 추적
    void Chase(Vector3 pos)
    {
        if (agent.isPathStale) return;

        isChase = true;
        agent.speed = chaseSpeed;
        damping = 7f;
        agent.destination = pos;
        agent.isStopped = false;
    }

    // 복귀
    void Return()
    {
        if (agent.isPathStale) return;

        isReturn = true;
        agent.speed = chaseSpeed;
        damping = 7f;
        agent.destination = wayPoint[nextIdx];
        agent.isStopped = false;
    }

    // 멈춤
    void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        isPatrol = false;
    }

    // 공격 포인트 활성화
    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }
    #endregion

    #region [행동 코루틴]
    // 상태별 설정
    IEnumerator Action()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);

            switch (state)
            {
                case State.idle:
                    Stop();
                    isCombat = false;
                    isChase = false;
                    isReturn = false;
                    isAttack = false;
                    anim.SetBool("IsPatrol", false);
                    anim.SetBool("IsChase", false);
                    yield return new WaitForSeconds(3f);
                    state = State.patrol;
                    break;
                case State.patrol:
                    Patrol();
                    isCombat = false;
                    isChase = false;
                    isReturn = false;
                    isAttack = false;
                    anim.SetBool("IsPatrol", true);
                    anim.SetBool("IsChase", false);
                    break;
                case State.chase:
                    Chase(player.transform.position);
                    isCombat = true;
                    isPatrol = false;
                    isReturn = false;
                    isAttack = false;
                    anim.SetBool("IsChase", true);
                    anim.SetBool("IsPatrol", false);
                    break;
                case State.goBack:
                    Return();
                    isCombat = false;
                    isPatrol = false;
                    isChase = false;
                    isAttack = false;
                    anim.SetBool("IsChase", true);
                    anim.SetBool("IsPatrol", false);
                    break;
                case State.attack:
                    Stop();
                    isChase = false;
                    isAttack = true;
                    anim.SetBool("IsChase", false);
                    anim.SetTrigger("OnAttack");
                    anim.SetInteger("AttackIdx", Random.Range(0, 4));
                    yield return new WaitForSeconds(attackSpd);
                    break;
                case State.die:
                    Stop();
                    isCombat = false;
                    isAttack = false;
                    isDie = true;
                    anim.SetTrigger("IsDie");
                    GetComponent<CapsuleCollider>().enabled = false;
                    break;
            }
        }
    }

    // 상태 변환
    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(1f);

        while (!isDie)
        {
            if (state == State.die) yield break;

            float playerDist = Vector3.Distance(player.transform.position, transform.position);
            float wayPointDist = Vector3.Distance(wayPoint[nextIdx], transform.position);
            Collider[] cols = Physics.OverlapSphere(transform.position, chaseRange, playerMask);
            if (playerDist <= attackDist)
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

            yield return new WaitForSeconds(0.3f);
        }
    }
    #endregion
}