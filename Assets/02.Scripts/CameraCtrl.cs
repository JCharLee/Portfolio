using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public float height;
    public float camSpeed;
    public float curPan;
    public float curTilt;
    public float dist;
    public Player player;
    public Transform tilt;
    public Camera mainCam;

    public GameManager manager;

    int layerMask;
    float maxDist = 10f;
    float yMax = 90f;

    void Start()
    {
        mainCam.transform.position = transform.position + tilt.forward * -dist;
        layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Obstacle");
    }

    void Update()
    {
        // ���콺 Ŀ��
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (manager.gameStart)
            CamCtrl();
    }

    void CamCtrl()
    {
        // ī�޶� ���� ����
        curPan += Input.GetAxis("Mouse X") * camSpeed;
        curTilt -= Input.GetAxis("Mouse Y") * camSpeed;
        curTilt = Mathf.Clamp(curTilt, -yMax, yMax);
        Vector3 playerDir = player.transform.eulerAngles;
        playerDir.y = transform.eulerAngles.y;

        // ī�޶� �Ÿ� ����
        dist -= Input.GetAxis("Mouse ScrollWheel") * camSpeed;
        dist = Mathf.Clamp(dist, 0f, maxDist);

        // ī�޶� �浹 ó��
        Vector3 castDir = (mainCam.transform.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, castDir, out hit, dist, layerMask))
        {
            if (hit.point != Vector3.zero)
                mainCam.transform.position = hit.point;
            else
            {
                mainCam.transform.localPosition = Vector3.zero;
                mainCam.transform.Translate(playerDir * dist);
            }
        }
    }

    void LateUpdate()
    {
        // ī�޶� ����
        transform.position = player.transform.position + (Vector3.up * height);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, curPan, transform.eulerAngles.z);
        tilt.eulerAngles = new Vector3(curTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, transform.position + tilt.forward * -dist, 0.03f);
    }
}