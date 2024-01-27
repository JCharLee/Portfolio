using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public float rotSpeed;
    public float jumpForce;
    public bool attacking;
    public bool jumping;
    public Camera mainCam;
    public Animator anim;
    public Rigidbody rb;
    public GameObject attackPoint;

    float h, v;

    void Start()
    {
        attacking = false;
    }

    void Update()
    {
        Move();
        Attack();
        Jump();
    }

    private void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = Rotating(h, v);

        if ((h != 0) || (v != 0))
        {
            if (!attacking)
            {
                transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpeed * Time.deltaTime);
                anim.SetBool("IsMove", true);
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

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !jumping)
        {
            attacking = true;
            anim.SetTrigger("Attack");
        }
    }

    private void Jump()
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            jumping = false;
            anim.SetBool("IsJump", false);
        }
    }
}