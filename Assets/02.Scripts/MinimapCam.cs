using System.Collections;
using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class MinimapCam : MonoBehaviour
{
    public float dist;
    public Player player;
    public Image playerMark;

    float minDist = 30f;
    float maxDist = 80f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            if (dist < maxDist)
                dist += 10f;
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            if (dist > minDist)
                dist -= 10f;
    }

    void LateUpdate()
    {
        playerMark.transform.position = player.transform.position + Vector3.up * (dist - 5f);
        transform.position = player.transform.position + Vector3.up * dist;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}