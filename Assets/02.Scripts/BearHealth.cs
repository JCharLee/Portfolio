using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BearHealth : MonoBehaviour
{
    public float hp;
    public float initHp;

    public Image hpBar;
    public Bear bear;
    public Player player;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnDamage()
    {
        hp -= player.damage;
        hpBar.fillAmount = (hp / initHp);

        if (hp <= 0f)
        {
            bear.state = Bear.State.die;
        }
    }
}