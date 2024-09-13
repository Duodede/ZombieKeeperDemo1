using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputFunctions : MonoBehaviour
{
    public InputParameters outputParameters;
    private UniversalController controller;
    GameObject typeField;
    NetworkGameManager gameManager;
    private void Start()
    {
        TryGetComponent<UniversalController>(out controller);
        if (controller.controlMode == UniversalController.ControlMode.OtherPlayer)
            return;
        typeField = GameObject.FindWithTag("TypeField");
        typeField.SetActive(false);
        if(GameObject.FindWithTag("Manager") != null)
        {
            GameObject.FindWithTag("Manager").TryGetComponent<NetworkGameManager>(out gameManager);
        }
    }
    private void Update()
    {
        if(controller != null)
        {
            controller.inputParameters = outputParameters;
        }
    }
    public void GetMoveInput(InputAction.CallbackContext ctx)
    {
        outputParameters.moveInput = ctx.ReadValue<Vector2>();
    }
    public void GetCrouchInput(InputAction.CallbackContext ctx)
    {
        outputParameters.isCrouching = ctx.ReadValueAsButton();
    }
    public void GetRunInput(InputAction.CallbackContext ctx)
    {
        outputParameters.isRunning = ctx.ReadValueAsButton();
    }
    public void GetJumpInput(InputAction.CallbackContext ctx)
    {
        outputParameters.isJumping = ctx.ReadValueAsButton();
    }
    public void GetTypeInput(InputAction.CallbackContext ctx)
    {
        typeField.SetActive(!typeField.activeSelf);
    }
    public void GetEnterInput(InputAction.CallbackContext ctx)
    {
        gameManager.SendMessage();
    }
}
