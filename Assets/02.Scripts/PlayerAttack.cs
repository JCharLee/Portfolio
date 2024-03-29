using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("컴포넌트 변수")]
    public Animator anim;

    [Header("클래스 변수")]
    public Player player;
    public GameManager manager;

    public enum WeaponType { melee, range, caster }
    [Header("무기 타입")]
    public WeaponType type;
    public Sprite[] typeImg;

    [Header("공격 변수")]
    public float basicDamage;
    public float attackDist;
    public bool attacking;
    public GameObject attackPoint;

    [Header("타겟 변수")]
    public GameObject targetEnemy;
    public LayerMask enemyMask;
    public Collider[] enemyCols;
    public float enemyRecogRange;
    public Dictionary<GameObject, float> enemyDistInfo;

    [Header("스킬 변수")]
    public float mp;
    public float initMp;

    [Header("UI 변수")]
    public Image curTypeImg;
    public Image mpBar;
    public TextMeshProUGUI mpText;

    void Start()
    {
        enemyDistInfo = new Dictionary<GameObject, float>();
        attacking = false;
        mpText.text = $"{mp} / {initMp}";
        mpBar.fillAmount = (mp / initMp);
    }

    void Update()
    {
        if (manager.gameStart)
        {
            TypeSet();
            ChangeWeapon();
            TargetEnemey();
            BasicAttack();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyRecogRange);
        Gizmos.DrawWireSphere(transform.position, attackDist);
    }

    void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            type = WeaponType.melee;

        if (Input.GetKeyDown(KeyCode.F2))
            type = WeaponType.range;

        if (Input.GetKeyDown(KeyCode.F3))
            type = WeaponType.caster;
    }

    void TypeSet()
    {
        switch (type)
        {
            case WeaponType.melee:
                attackDist = 3f;
                break;
            case WeaponType.range:
                attackDist = 10f;
                break;
            case WeaponType.caster:
                attackDist = 10f;
                break;
        }
    }

    // 적 타겟을 인지 범위 안에서 감지하고 가장 가까운 적을 타겟
    void TargetEnemey()
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

    void BasicAttack()
    {
        if (Input.GetMouseButtonDown(0) && !player.jumping)
        {
            switch (type)
            {
                case WeaponType.melee:
                    if (targetEnemy != null)
                    {
                        StartCoroutine(Rush());
                        transform.rotation = Quaternion.LookRotation(targetEnemy.transform.position - transform.position);
                    }
                    anim.SetTrigger("MeleeAttack");
                    break;
                case WeaponType.range:
                    break;
                case WeaponType.caster:
                    break;
            }
            attacking = true;
        }
    }

    public void AttackEnd()
    {
        attacking = false;
    }

    // 공격 포인트 활성화
    public void AttackPointActive()
    {
        attackPoint.SetActive(true);
    }

    // 근접 공격시 적에게 돌진
    IEnumerator Rush()
    {
        float dist = Vector3.Distance(targetEnemy.transform.position, transform.position);

        if (dist <= attackDist) yield break;

        while (dist > attackDist)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, 10f * Time.deltaTime);
            dist = Vector3.Distance(targetEnemy.transform.position, transform.position);
            anim.SetBool("IsMove", true);
            yield return null;
        }
    }
}