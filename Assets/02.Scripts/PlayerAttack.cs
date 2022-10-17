using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Collider meleeArea;
    public ParticleSystem muzz;

    bool attacking;

    Animator anim;
    PlayerMove playerMove;

    readonly int attack = Animator.StringToHash("IsAttack");
    readonly int meleeIdx = Animator.StringToHash("MeleeIdx");

    void Start()
    {
        anim = GetComponent<Animator>();
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            switch (playerMove.mode)
            {
                case PlayerMove.Mode.None:
                    return;
                case PlayerMove.Mode.Melee:
                    StartCoroutine(MeleeAttack());
                    break;
                case PlayerMove.Mode.Range:
                    StartCoroutine(RangeAttack());
                    break;
            }
        }

        Targeting();
    }

    IEnumerator MeleeAttack()
    {
        if (!attacking)
        {
            attacking = true;
            anim.SetInteger(meleeIdx, Random.Range(0, 3));
            anim.SetTrigger(attack);
            yield return new WaitForSeconds(0.7f);
            meleeArea.enabled = true;
            yield return new WaitForSeconds(0.1f);
            meleeArea.enabled = false;
            yield return new WaitForSeconds(0.5f);
            attacking = false;
        }
    }

    IEnumerator RangeAttack()
    {
        if (!attacking)
        {
            attacking = true;
            anim.SetTrigger(attack);
            muzz.Play();
            yield return new WaitForSeconds(0.1f);
            anim.SetTrigger(attack);
            muzz.Play();
            yield return new WaitForSeconds(0.1f);
            anim.SetTrigger(attack);
            muzz.Play();
            yield return new WaitForSeconds(0.8f);
            attacking = false;
        }
    }

    void Targeting()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("targeting");
        }
    }
}