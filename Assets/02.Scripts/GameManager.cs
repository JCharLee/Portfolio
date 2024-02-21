using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool gameStart;

    void Start()
    {
        gameStart = false;
        StartCoroutine("GameActive");
    }

    IEnumerator GameActive()
    {
        yield return new WaitForSeconds(1f);
        gameStart = true;
    }
}