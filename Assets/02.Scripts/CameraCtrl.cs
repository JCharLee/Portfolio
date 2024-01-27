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

    float maxDist = 10f;
    float yMax = 90f;

    void Start()
    {
        transform.position = player.transform.position + (Vector3.up * height);
        transform.rotation = player.transform.rotation;
        tilt.eulerAngles = new Vector3(curTilt, transform.eulerAngles.y, transform.eulerAngles.z);
        mainCam.transform.position += tilt.forward * -dist;
    }

    void Update()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        curPan += Input.GetAxis("Mouse X") * camSpeed;
        curTilt -= Input.GetAxis("Mouse Y") * camSpeed;
        curTilt = Mathf.Clamp(curTilt, -yMax, yMax);
        Vector3 playerDir = player.transform.eulerAngles;
        playerDir.y = transform.eulerAngles.y;

        dist -= Input.GetAxis("Mouse ScrollWheel") * camSpeed;
        dist = Mathf.Clamp(dist, 0f, maxDist);

        Vector3 castDir = (mainCam.transform.position - transform.position).normalized;
        Debug.DrawRay(transform.position, castDir, Color.green);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, castDir, out hit, dist))
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
        transform.position = player.transform.position + (Vector3.up * height);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, curPan, transform.eulerAngles.z);
        tilt.eulerAngles = new Vector3(curTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, transform.position + tilt.forward * -dist, 0.03f);
    }
}