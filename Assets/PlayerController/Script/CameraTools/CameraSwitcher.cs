using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Rendering;

public class CameraSwitcher : MonoBehaviour
{
    [Header("������������")]
    public GameObject TPSCamera,FPSCamera,FocusCamera;
    public UniversalController.ControlMode controlMode;
    public int currentConrolMode = 0;
    public Transform TPSFixPoint, FPSFixPoint;
    UniversalController playerController;
    [Header("��ͷ�۽�")]
    public bool isFixingOnOtherThings;//�Ƿ�۽���עһ������
    public Transform fixingObject;
    private void Start()
    {
        playerController = GetComponent<UniversalController>();
        TPSCamera = GameObject.FindWithTag("TPSCamera");
        FPSCamera = GameObject.FindWithTag("FPSCamera");
        FocusCamera = GameObject.FindWithTag("FocusCamera");
        if (playerController.controlMode != UniversalController.ControlMode.OtherPlayer)
        {
            TPSCamera.GetComponent<CinemachineFreeLook>().Follow = transform;
            TPSCamera.GetComponent<CinemachineFreeLook>().LookAt = TPSFixPoint;
            FPSCamera.GetComponent<CinemachineVirtualCamera>().Follow = FPSFixPoint;
            FocusCamera.GetComponent<CinemachineVirtualCamera>().Follow = TPSFixPoint;
            SwitchControlMode();
        }
    }
    /// <summary>
    /// �л�����ģʽ�����¼�
    /// </summary>
    /// <param name="ctx"></param>
    public void ChangeControlMode(InputAction.CallbackContext ctx)
    {
        currentConrolMode++;
        currentConrolMode = (int)Mathf.Repeat(currentConrolMode, 2);
        SwitchControlMode();
    }
    /// <summary>
    /// �л��۽�ģʽ
    /// </summary>
    /// <param name="ctx"></param>
    public void ChangeFixingMode(InputAction.CallbackContext ctx)
    {
        SwitchFixingMode(!isFixingOnOtherThings);
    }
    /// <summary>
    /// �ı���ҿ���ģʽ
    /// </summary>
    private void SwitchControlMode()
    {
        switch (currentConrolMode)
        {
            case 0:
                controlMode = UniversalController.ControlMode.TPS;
                break;
            case 1:
                controlMode = UniversalController.ControlMode.FPS;
                break;
            default:
                controlMode = UniversalController.ControlMode.TPS;
                break;
        }
        TPSCamera.SetActive(currentConrolMode == 0);
        FPSCamera.SetActive(currentConrolMode == 1);
        FocusCamera.SetActive(false);
        playerController.controlMode = controlMode;
    }
    public void SwitchFixingMode(bool aimState)
    {
        Transform cam = Camera.main.transform;
        float angle = Vector3.Angle(cam.forward, new Vector3(fixingObject.position.x - cam.position.x, 0, fixingObject.position.z - cam.position.z));
        if(angle <= 30)
        {
            isFixingOnOtherThings = aimState;
        }
        if(isFixingOnOtherThings&&fixingObject != null)
        {
            FocusCamera.SetActive(true);
            FocusCamera.GetComponent<CinemachineVirtualCamera>().LookAt = fixingObject;
            TPSCamera.SetActive(false);
            FPSCamera.SetActive(false);
        }
        else
        {
            FocusCamera.SetActive(false);
            FocusCamera.GetComponent<CinemachineVirtualCamera>().LookAt = null;
            SwitchControlMode();
        }
    }
}
