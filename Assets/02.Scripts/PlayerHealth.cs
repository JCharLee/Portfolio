using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("컴포넌트 변수")]
    public Bear bear;

    [Header("체력 변수")]
    public float hp;
    public float initHp;

    [Header("UI 변수")]
    public Image hpBar;
    public TextMeshProUGUI hpText;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnDamage()
    {
        hp -= bear.damage;

        hpText.text = $"{hp} / {initHp}";
        hpBar.fillAmount = (hp / initHp);
    }
}