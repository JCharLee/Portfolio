using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackPoint : MonoBehaviour
{
    PlayerHealth playerHealth;
    BearHealth bearHealth;

    private void OnEnable()
    {
        StartCoroutine("AutoDisable");
    }

    private void OnTriggerEnter(Collider other)
    {
        switch(other.tag)
        {
            case "Player":
                playerHealth = other.GetComponent<PlayerHealth>();
                playerHealth.OnDamage();
                break;
            case "Enemy":
                bearHealth = other.GetComponent<BearHealth>();
                bearHealth.OnDamage();
                break;
        }
    }

    IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
}