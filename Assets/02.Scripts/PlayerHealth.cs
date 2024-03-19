using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("������Ʈ ����")]
    public Bear bear;

    [Header("ü�� ����")]
    public float hp;
    public float initHp;

    [Header("UI ����")]
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