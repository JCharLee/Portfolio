using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TargetDistInfo : SerializableDictionary<GameObject, float> { }

public class Player : MonoBehaviour
{
    [Header("컴포넌트 변수")]
    public Animator anim;
    public Rigidbody rb;

    [Header("클래스 변수")]
    public Camera mainCam;
    public GameManager manager;

    [Header("움직임 관련 변수")]
    public float moveSpeed;     // 이동 속도
    public float rotSpeed;      // 회전 속도
    public float jumpForce;     // 점프 파워
    public bool jumping;        // 점프중
    float h, v;

    [Header("공격 관련 변수")]
    public float attackDist;        // 공격 거리
    public float damage;
    public bool attacking;          // 공격중
    public GameObject attackPoint;  // 근접 공격 범위

    [Header("타겟 관련 변수")]
    public GameObject targetEnemy;          // 현재 적 타겟
    public LayerMask enemyMask;             // 적 레이어
    public Collider[] enemyCols;            // 인지 범위 내 적 콜라이더
    public float enemyRecogRange;           // 적 인지 범위 거리
    public TargetDistInfo enemyDistInfo;    // 적 타겟의 거리 정보

    [Header("상호작용 관련 변수")]
    public GameObject interactionTarget;    // 상호작용 타겟
    public LayerMask interactableMask;      // 상호작용 레이어
    public Collider[] interactableCols;     // 인지 범위 내 상호작용 콜라이더
    public float interactableRecogRange;    // 상호작용 인지 범위 거리
    IInteraction interactable;

    void Start()
    {
        attacking = false;
    }

    void Update()
    {
        if (manager.gameStart)
        {
            Move();
            Attack();
            Jump();
            EnemyTargeting();
            Interaction();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyRecogRange);
        Gizmos.DrawWireSphere(transform.position, attackDist);
    }

    #region [기본 움직임]
    void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = Rotating(h, v);
        Vector3 moveVec = new Vector3(h, 0, v).normalized;

        if (moveVec != Vector3.zero)
        {
            if (!attacking)
            {
                transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpeed * Time.deltaTime);
                anim.SetBool("IsMove", moveVec != Vector3.zero);
            }
        }
        else
            anim.SetBool("IsMove", false);
    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = mainCam.transform.TransformDirection(Vector3.forward);
        forward.y = 0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);
        Vector3 newDir = (forward * vertical) + (right * horizontal);

        return newDir;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!attacking && !jumping)
            {
                jumping = true;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                anim.SetBool("IsJump", true);
            }
        }
    }
    #endregion

    #region [공격]
    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !jumping)
        {
            if (targetEnemy != null)
            {
                if (!attacking)
                    StartCoroutine(Rush());
                transform.rotation = Quaternion.LookRotation(targetEnemy.transform.position - transform.position);
            }

            attacking = true;
            anim.SetTrigger("Attack");
        }
    }

    // 공격 포인트 활성화
    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }

    // 적 타겟을 인지 범위 안에서 감지하고 가장 가까운 적을 타겟
    void EnemyTargeting()
    {
        enemyCols = Physics.OverlapSphere(transform.position, enemyRecogRange, enemyMask);

        foreach (Collider col in enemyCols)
        {
            float dist = Vector3.Distance(col.transform.position, transform.position);
            if (!enemyDistInfo.ContainsKey(col.gameObject))
                enemyDistInfo.Add(col.gameObject, dist);
            enemyDistInfo[col.gameObject] = dist;
        }

        foreach (GameObject target in enemyDistInfo.Keys)
        {
            Collider col = target.GetComponent<Collider>();
            if (!enemyCols.Contains(col))
            {
                enemyDistInfo.Remove(target);
                break;
            }
        }

        if (enemyDistInfo.Count != 0)
            targetEnemy = enemyDistInfo.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
        else
            targetEnemy = null;
    }

    // 공격시 타겟 적에게 돌진
    IEnumerator Rush()
    {
        float dist = (targetEnemy.transform.position - transform.position).magnitude;

        while (dist > attackDist)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, 10f * Time.deltaTime);
            dist = (targetEnemy.transform.position - transform.position).magnitude;
            anim.SetBool("IsMove", true);
            yield return null;
        }
    }
    #endregion

    // 상호작용
    void Interaction()
    {
        interactableCols = Physics.OverlapSphere(transform.position, interactableRecogRange, interactableMask);
        if (interactableCols.Length > 0)
        {
            interactionTarget = interactableCols[0].gameObject;
            interactable = interactionTarget.GetComponent<IInteraction>();
            if (Input.GetKeyDown(KeyCode.F))
            {
                interactable.Action(this);
            }
        }
        else
            interactionTarget = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            jumping = false;
            anim.SetBool("IsJump", false);
        }
    }
}