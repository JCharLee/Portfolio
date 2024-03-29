using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("컴포넌트 변수")]
    public Animator anim;
    public Rigidbody rb;

    [Header("클래스 변수")]
    public Camera mainCam;
    public PlayerAttack playerAtk;
    public GameManager manager;

    [Header("움직임 관련 변수")]
    public float moveSpeed;     // 이동 속도
    public float rotSpeed;      // 회전 속도
    public float jumpForce;     // 점프 파워
    public bool jumping;        // 점프중
    float h, v;

    [Header("상호작용 관련 변수")]
    public GameObject interactionTarget;    // 상호작용 타겟
    public LayerMask interactableMask;      // 상호작용 레이어
    public Collider[] interactableCols;     // 인지 범위 내 상호작용 콜라이더
    public float interactableRecogRange;    // 상호작용 인지 범위 거리
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

    #region [기본 움직임]
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