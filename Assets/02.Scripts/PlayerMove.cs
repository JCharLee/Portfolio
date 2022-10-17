using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public enum Mode { None, Melee, Range }
    public Mode mode;

    public float moveSpeed;
    public float aimSpeed;
    public float rotSpeed;
    public float jumpForce;

    public GameObject meleeWeapon;
    public GameObject rangeWeapon;
    public CameraCtrl camCtrl;

    float h, v;

    bool isJump;
    bool isMelee;
    bool isRange;
    bool isCombat;

    Animator anim;
    Rigidbody rb;
    Camera mainCam;

    readonly int isMove = Animator.StringToHash("IsMove");
    readonly int jumpMotion = Animator.StringToHash("IsJump");
    readonly int airMotion = Animator.StringToHash("OnAir");
    readonly int speedFloat = Animator.StringToHash("Speed");
    readonly int speedX = Animator.StringToHash("MoveX");
    readonly int speedY = Animator.StringToHash("MoveY");
    readonly int modeCombat = Animator.StringToHash("IsCombat");
    readonly int modeMelee = Animator.StringToHash("IsMelee");
    readonly int modeRange = Animator.StringToHash("IsRange");

    void Awake()
    {
        mode = Mode.None;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJump = false;
            anim.SetBool(airMotion, false);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJump = true;
            anim.SetBool(airMotion, true);
        }
    }

    void Update()
    {
        Movement();
        Sprint();
        Jump();
        ModeChange();
    }

    void Movement()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = Rotating(h, v);
        transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
        if (Input.GetMouseButton(1))
        {
            isCombat = true;
            transform.rotation = Quaternion.LookRotation(Rotating(0f, 1f));
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * camCtrl.camSpeed);
            if (Moving())
            {
                anim.SetFloat(speedX, h, 0.1f, Time.deltaTime);
                anim.SetFloat(speedY, v, 0.1f, Time.deltaTime);
            }
            else
            {
                anim.SetFloat(speedX, 0f, 0.1f, Time.deltaTime);
                anim.SetFloat(speedY, 0f, 0.1f, Time.deltaTime);
            }
        }
        else
        {
            isCombat = false;
            if (Moving())
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpeed * Time.deltaTime);
        }
        anim.SetBool(modeCombat, Input.GetMouseButton(1));
        anim.SetBool(isMove, Moving());
    }

    bool Moving()
    {
        if (!(h == 0 && v == 0))
            return true;
        else
            return false;
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
            if (!isJump)
            {
                isJump = true;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                anim.SetTrigger(jumpMotion);
                anim.SetBool(airMotion, true);
            }
        }
    }

    void Sprint()
    {
        if (isMelee || isRange) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 8f;
            anim.SetFloat(speedFloat, 1f, 0.1f, Time.deltaTime);
        }
        else
        {
            moveSpeed = 5f;
            anim.SetFloat(speedFloat, 0f, 0.1f, Time.deltaTime);
        }
    }

    void ModeChange()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            switch (mode)
            {
                case Mode.None:
                    mode = Mode.Melee;
                    meleeWeapon.SetActive(true);
                    isMelee = true;
                    anim.SetBool(modeMelee, true);
                    break;
                case Mode.Melee:
                    mode = Mode.Range;
                    meleeWeapon.SetActive(false);
                    rangeWeapon.SetActive(true);
                    isMelee = false;
                    isRange = true;
                    anim.SetBool(modeMelee, false);
                    anim.SetBool(modeRange, true);
                    break;
                case Mode.Range:
                    mode = Mode.None;
                    rangeWeapon.SetActive(false);
                    isRange = false;
                    anim.SetBool(modeRange, false);
                    break;
            }
        }
    }
}