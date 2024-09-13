using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GPSGuideController : MonoBehaviourPunCallbacks
{
    public GameObject playerPositionSign;
    Transform GPSCamera;
    Vector3 mainCameraForward;
    Camera mainCamera;
    bool isMine;//这个角色是否是自己控制的
    void Start()
    {
        GPSCamera = GameObject.FindWithTag("GPS").transform;
        isMine = photonView.IsMine;
        playerPositionSign.SetActive(isMine);
        mainCamera = Camera.main;
    }
    void Update()
    {
        if(isMine)
        {
            GPSCamera.position = transform.position;
            mainCameraForward = mainCamera.transform.forward;
            GPSCamera.rotation = Quaternion.LookRotation(new Vector3(mainCameraForward.x, 0, mainCameraForward.z));
        }
    }
}
