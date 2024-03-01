using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TargetDistInfo : SerializableDictionary<GameObject, float> { }

public class Player : MonoBehaviour
{
    [Header("������Ʈ ����")]
    public Animator anim;
    public Rigidbody rb;

    [Header("Ŭ���� ����")]
    public Camera mainCam;
    public GameManager manager;

    [Header("������ ���� ����")]
    public float moveSpeed;     // �̵� �ӵ�
    public float rotSpeed;      // ȸ�� �ӵ�
    public float jumpForce;     // ���� �Ŀ�
    public bool jumping;        // ������
    float h, v;

    [Header("���� ���� ����")]
    public bool attacking;          // ������
    public GameObject attackPoint;  // ���� ����

    [Header("Ÿ�� ���� ����")]
    public GameObject targetEnemy;          // ���� �� Ÿ��
    public LayerMask enemyMask;             // �� ���̾�
    public Collider[] enemyCols;            // ���� ���� �� �� �ݶ��̴�
    public float enemyRecogRange;           // �� ���� ���� �Ÿ�
    public TargetDistInfo enemyDistInfo;    // �� Ÿ���� �Ÿ� ����

    [Header("��ȣ�ۿ� ���� ����")]
    public GameObject interactionTarget;    // ��ȣ�ۿ� Ÿ��
    public LayerMask interactableMask;      // ��ȣ�ۿ� ���̾�
    public Collider[] interactableCols;     // ���� ���� �� ��ȣ�ۿ� �ݶ��̴�
    public float interactableRecogRange;    // ��ȣ�ۿ� ���� ���� �Ÿ�
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

    // �޺��� ���� ���콺 Ŭ�� �ν� �ִϸ��̼� �̺�Ʈ
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

    // �� Ÿ���� ���� ���� �ȿ��� �����ϰ� ���� ����� ���� Ÿ����
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