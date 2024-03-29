using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("������Ʈ ����")]
    public Animator anim;
    public Rigidbody rb;

    [Header("Ŭ���� ����")]
    public Camera mainCam;
    public PlayerAttack playerAtk;
    public GameManager manager;

    [Header("������ ���� ����")]
    public float moveSpeed;     // �̵� �ӵ�
    public float rotSpeed;      // ȸ�� �ӵ�
    public float jumpForce;     // ���� �Ŀ�
    public bool jumping;        // ������
    float h, v;

    [Header("��ȣ�ۿ� ���� ����")]
    public GameObject interactionTarget;    // ��ȣ�ۿ� Ÿ��
    public LayerMask interactableMask;      // ��ȣ�ۿ� ���̾�
    public Collider[] interactableCols;     // ���� ���� �� ��ȣ�ۿ� �ݶ��̴�
    public float interactableRecogRange;    // ��ȣ�ۿ� ���� ���� �Ÿ�
    IInteraction interactable;

    void Start()
    {
        
    }

    void Update()
    {
        if (manager.gameStart)
        {
            Move();
            Jump();
            Interaction();
        }
    }

    #region [�⺻ ������]
    void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = Rotating(h, v);
        Vector3 moveVec = new Vector3(h, 0, v).normalized;

        if (moveVec != Vector3.zero)
        {
            if (!playerAtk.attacking)
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
            if (!playerAtk.attacking && !jumping)
            {
                jumping = true;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                anim.SetBool("IsJump", true);
            }
        }
    }
    #endregion

    // ��ȣ�ۿ�
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