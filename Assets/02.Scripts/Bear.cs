using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bear : MonoBehaviour
{
    public enum State { idle, patrol, chase, goBack, attack, die }
    [Header("����")]
    public State state = State.idle;

    [Header("������Ʈ ����")]
    public Animator anim;
    public Collider col;
    public NavMeshAgent agent;
    public BearHealth health;

    [Header("���� ����")]
    public float damage;
    public float attackSpd;
    public bool attacking;
    public GameObject attackPoint;

    [Header("WayPoint ���� ����")]
    public int nextIdx;
    public List<Vector3> wayPoint;

    [Header("NavMeshAgent ����")]
    public float moveSpeed;
    public float runSpeed;
    public float damping;

    [Header("���� ����")]
    public float chaseRange;
    public float attackDist;

    [Header("���� bool ����")]
    public bool isCombat;
    public bool isIdle;
    public bool isPatrol;
    public bool isChase;
    public bool isReturn;
    public bool isAttack;
    public bool isDie;

    [Header("�÷��̾� ����")]
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

    // ������ ���� ����Ʈ ����
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
    

    #region [���º� �ൿ]
    // ���
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

    // ����
    void Patrol()
    {
        if (agent.isPathStale) return;

        AgentActive(moveSpeed, 1f, wayPoint[nextIdx]);

        isPatrol = true;
        isReturn = false;

        anim.SetBool("IsPatrol", true);
        anim.SetBool("IsChase", false);
    }

    // ����
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

    // ����
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

    // ����
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

    // ���� ����Ʈ Ȱ��ȭ
    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }

    // ����
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

    #region [Nav �۵�]
    // NavAgent Ȱ��
    void AgentActive(float speed, float damp, Vector3 pos)
    {
        agent.speed = speed;
        damping = damp;
        agent.destination = pos;
        agent.isStopped = false;
    }

    // NavAgent ����
    void AgentStop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    #endregion

    #region [���� üũ �ڷ�ƾ]
    // ���º� ����
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

    // ���� üũ
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