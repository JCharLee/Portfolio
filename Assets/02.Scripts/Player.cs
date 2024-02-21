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

    [Header("움직임 변수")]
    public float moveSpeed;
    public float rotSpeed;
    public float jumpForce;
    public bool jumping;

    [Header("공격 변수")]
    public bool attacking;
    public GameObject attackPoint;

    [Header("타겟 변수")]
    public LayerMask targetMask;
    public Collider[] cols;
    public float targetRecogRange;
    public TargetDistInfo targetDistInfo;
    public GameObject targetEnemy;

    public Camera mainCam;
    public GameManager manager;

    float h, v;

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
        }
    }

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

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !jumping)
        {
            if (targetEnemy != null)
            {
                if (!attacking)
                    StartCoroutine("Rush");
                transform.rotation = Quaternion.LookRotation(targetEnemy.transform.position - transform.position);
            }

            attacking = true;
            anim.SetTrigger("Attack");
        }
    }

    IEnumerator Rush()
    {
        float dist = (targetEnemy.transform.position - transform.position).magnitude;

        while (dist > 2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, 10f * Time.deltaTime);
            dist = (targetEnemy.transform.position - transform.position).magnitude;
            anim.SetBool("IsMove", true);
            yield return null;
        }
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

    // 콤보를 위한 마우스 클릭 인식 애니메이션 이벤트
    public void ComboStartCheck()
    {
        anim.SetBool("IsCombo", false);
        StartCoroutine(ComboAttack());

        IEnumerator ComboAttack()
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            anim.SetBool("IsCombo", true);
        }
    }

    public void ComboEndCheck()
    {
        if (!anim.GetBool("IsCombo"))
        {
            attacking = false;
            anim.ResetTrigger("Attack");
        }
    }

    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }

    // 적 타겟을 인지 범위 안에서 감지하고 가장 가까운 적을 타겟팅
    void EnemyTargeting()
    {
        cols = Physics.OverlapSphere(transform.position, targetRecogRange, targetMask);

        foreach (Collider col in cols)
        {
            float dist = Vector3.Distance(col.transform.position, transform.position);
            if (!targetDistInfo.ContainsKey(col.gameObject))
                targetDistInfo.Add(col.gameObject, dist);
            targetDistInfo[col.gameObject] = dist;
        }

        foreach (GameObject target in targetDistInfo.Keys)
        {
            Collider col = target.GetComponent<Collider>();
            if (!cols.Contains(col))
            {
                targetDistInfo.Remove(target);
                break;
            }
        }

        if (targetDistInfo.Count != 0)
            targetEnemy = targetDistInfo.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
        else
            targetEnemy = null;
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