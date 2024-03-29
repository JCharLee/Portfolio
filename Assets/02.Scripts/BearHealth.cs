using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BearHealth : MonoBehaviour
{
    public float hp;
    public float initHp;

    public Canvas canvas;
    public Image hpBar;
    public Bear bear;
    public PlayerAttack playerAtk;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnDamage()
    {
        hp -= playerAtk.basicDamage;
        hpBar.fillAmount = (hp / initHp);

        if (hp <= 0f)
        {
            bear.state = Bear.State.die;
            canvas.gameObject.SetActive(false);
        }
    }
}