using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed;
    public float aimSpeed;
    public float rotSpeed;
    public float jumpForce;

    float h, v;
    Vector3 target;

    bool isJump;
    bool keyMoving;

    Animator anim;
    Rigidbody rb;
    Camera mainCam;

    readonly int isMove = Animator.StringToHash("IsMove");
    readonly int jumpMotion = Animator.StringToHash("IsJump");
    readonly int airMotion = Animator.StringToHash("OnAir");
    readonly int speedFloat = Animator.StringToHash("Speed");
    readonly int rifleMode = Animator.StringToHash("RifleOn");
    readonly int aimMode = Animator.StringToHash("Aim");

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    void Start()
    {
        //target = transform.position;
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
        KeyMove();
        MouseMove();
        Sprint();
        Jump();
    }

    void KeyMove()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0)
            anim.SetBool(isMove, true);
        else
            anim.SetBool(isMove, false);

        Vector3 moveDir = Rotating(h, v);
        if (!(h == 0 && v == 0))
        {
            keyMoving = true;
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpeed * Time.deltaTime);
        }
        else
            keyMoving = false;
    }

    void MouseMove()
    {
        if (keyMoving) return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                target = hit.point;
            }
        }
        Vector3 dir = target - transform.position;
        Vector3 rotDir = new Vector3(dir.x, 0f, dir.z);
        if (!keyMoving)
        {
            if (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(rotDir), rotSpeed * Time.deltaTime);
                anim.SetBool(isMove, true);
            }
            else
                anim.SetBool(isMove, false);
        }
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
}