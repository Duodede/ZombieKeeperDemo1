using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Photon.Pun;
using static UniversalController;

public class UniversalController : MonoBehaviourPunCallbacks
{
    private Transform characterTransform;
    private Animator characterAnimator;
    private Transform mainCameraTransform;
    private CharacterController characterController;
    public Vector3 characterMovement;
    Vector3 characterMovementWorldSpace;
    /// <summary>
    /// �ÿ������Ŀ���״̬
    /// </summary>
    public enum ControlMode
    {
        TPS,
        FPS,
        NPC,
        OtherPlayer,
    }
    public ControlMode controlMode = ControlMode.TPS;
    /// <summary>
    /// �ý�ɫ�Ķ���״̬
    /// </summary>
    public enum LocomotionState
    {
        Idle,
        Walk,
        Run,
    }
    public LocomotionState locomotionState = LocomotionState.Idle;

    /// <summary>
    /// �ý�ɫ����״̬
    /// </summary>
    public enum CharacterPosture
    {
        Stand,
        Crouch,
        Jumping,
        Falling,
        Landing,
        Climbing,
    }
    public CharacterPosture characterPosture = CharacterPosture.Stand;
    /// <summary>
    /// �ý�ɫ�ֲ�������Ʒ��״̬
    /// </summary>
    public enum ArmState
    {
        Normal,
        Aim,
        FPSNormal,
    }
    public ArmState armState = ArmState.Normal;
    //��̬����
    float crouchThreshold = 0f;
    float standThreshold = 1f;
    float midairThreshold = 2.1f;
    float landingThreshold = 1f;
    //�˶�����
    public float crouchspeed = 1.5f;
    public float walkspeed = 2.5f;
    public float runspeed = 5.5f;
    public float jumpHeight = 1.5f;
    public float jumpCoolDownTime = 0.5f;
    public float jumpableHeight = 3f;
    public bool isGrounded;
    public bool isFalling;
    public bool isLanding;
    [Range(0f, 1f)]
    public float groundCheckOffset;
    [Range(0f, 1f)]
    public float fallHeight = 0.5f;
    
    public InputParameters inputParameters;

    //����������ϣֵ
    int postureHash;
    int verticalHash;
    int horizontalHash;
    int turnspeedHash;
    int jumpVerticalHash;
    int feetTweenHash;
    int climbParameterHash;
    int controllerModeHash;

    //����
    float gravity = -9.8f;
    //��ֱ�ٶ�
    float verticalVelocity;
    //�Ƿ����Ծ����ֹ������Ծ����BUG��
    bool jumpable;

    //������Ծ�ٶȻ����
    static readonly int CACHE_SIZE = 3;
    Vector3[] velCache = new Vector3[CACHE_SIZE];
    int currentVelCacheIndex = 0;
    Vector3 averageVel;

    //��Ծ���ҽż���
    float feetTween;

    //��Խ�������
    CharacterSensor characterSensor;
    //�Ƿ������
    bool isClimbReady;
    //��������
    int defaultClimbParameter = 0;
    int vaultParameter = 1;
    int climbLowParameter = 2;
    int climbHighParameter = 3;
    int currentClimbParameter;
    Vector3 leftHandPosition;
    Vector3 rightFootPosition;
    Vector3 rightHandPosition;

    void Start()
    {
        characterTransform = transform;
        characterAnimator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        postureHash = Animator.StringToHash("PostureFactor");
        verticalHash = Animator.StringToHash("VerticalSpeed");
        horizontalHash = Animator.StringToHash("HorizontalSpeed");
        turnspeedHash = Animator.StringToHash("TurnSpeed");
        jumpVerticalHash = Animator.StringToHash("JumpVerticalSpeed");
        feetTweenHash = Animator.StringToHash("FeetTween");
        climbParameterHash = Animator.StringToHash("ClimbType");
        controllerModeHash = Animator.StringToHash("ControllerMode");
        mainCameraTransform = Camera.main.transform;
        characterSensor = GetComponent<CharacterSensor>();
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            controlMode = ControlMode.OtherPlayer;
        }
    }
    void Update()
    {
        if (controlMode == ControlMode.OtherPlayer)
            return;
        CheckGround();
        SwitchCharacterState();
        CalculateCharaterMoveDirection();
        Jumpable();
        Jump();
        SetupAnimator();
    }
    private void FixedUpdate()
    {
        CalculateGravity();
    }
    void CheckGround()
    {
        if (Physics.SphereCast(characterTransform.position + Vector3.up * groundCheckOffset, characterController.radius, Vector3.down, out RaycastHit hitInfo, groundCheckOffset - characterController.radius + 2 * characterController.skinWidth))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            isFalling = !Physics.Raycast(characterTransform.position, Vector3.down, fallHeight);
        }
    }
    /// <summary>
    /// �л���ɫ״̬
    /// </summary>
    void SwitchCharacterState()
    {
        switch (characterPosture)
        {
            //վ����̬����(��)��ת��
            case CharacterPosture.Stand:
                if (verticalVelocity > 0)
                {
                    characterPosture = CharacterPosture.Jumping;
                }
                else if (!isGrounded && isFalling)
                {
                    characterPosture = CharacterPosture.Falling;
                }
                else if (inputParameters.isCrouching)
                {
                    characterPosture = CharacterPosture.Crouch;
                }
                else if (isClimbReady)
                {
                    characterPosture = CharacterPosture.Climbing;
                }
                break;

            //�¶���̬������ת��
            case CharacterPosture.Crouch:
                if (!isGrounded && isFalling)
                {
                    characterPosture = CharacterPosture.Falling;
                }
                else if (!inputParameters.isCrouching)
                {
                    characterPosture = CharacterPosture.Stand;
                }
                break;

            //ˤ�䣬һ��ת��
            case CharacterPosture.Falling:
                if (isGrounded)
                {
                    StartCoroutine(JumpCoolDown());
                }
                if (isLanding)
                {
                    characterPosture = CharacterPosture.Landing;
                }
                break;

            //��Ծ��һ��ת��
            case CharacterPosture.Jumping:
                if (isGrounded)
                {
                    StartCoroutine(JumpCoolDown());
                }
                if (isLanding)
                {
                    characterPosture = CharacterPosture.Landing;
                }
                break;

            //��½��һ��ת��
            case CharacterPosture.Landing:
                if (!isLanding)
                {
                    characterPosture = CharacterPosture.Stand;
                }
                break;

            //������һ��ת��
            case CharacterPosture.Climbing:

                if (!characterAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Climb") && !characterAnimator.IsInTransition(0))
                {
                    characterPosture = CharacterPosture.Stand;
                }
                break;
        }
        isClimbReady = false;

        if (inputParameters.moveInput.magnitude == 0)
        {
            locomotionState = LocomotionState.Idle;
        }
        else if (!inputParameters.isRunning)
        {
            locomotionState = LocomotionState.Walk;
        }
        else
        {
            locomotionState = LocomotionState.Run;
        }

        if (inputParameters.isAiming)
        {
            armState = ArmState.Aim;
        }
        else if(controlMode == ControlMode.FPS)
        {
            armState = ArmState.FPSNormal;
        }
        else
        {
            armState = ArmState.Normal;
        }
    }
    /// <summary>
    /// �����ɫ���ƶ�����
    /// </summary>
    void CalculateCharaterMoveDirection()
    {
        Vector3 cameraForwardProjection = new Vector3(mainCameraTransform.forward.x, 0, mainCameraTransform.forward.z).normalized;
        switch(controlMode)
        {
            case ControlMode.FPS:
                characterMovementWorldSpace = new Vector3(inputParameters.moveInput.normalized.x,0, inputParameters.moveInput.normalized.y);
                characterMovement = characterMovementWorldSpace;
                break;
            case ControlMode.NPC:
                break;
            case ControlMode.TPS:
                characterMovementWorldSpace = cameraForwardProjection * inputParameters.moveInput.y + mainCameraTransform.right * inputParameters.moveInput.x;
                characterMovement = characterTransform.InverseTransformVector(characterMovementWorldSpace);
                break;

        }
    }
    /// <summary>
    /// ��������
    /// </summary>
    void CalculateGravity()
    {
        if(characterPosture != CharacterPosture.Jumping && characterPosture != CharacterPosture.Falling)
        {
            if(!isGrounded)
            {
                verticalVelocity += gravity * Time.fixedDeltaTime;
            }
            else
            {
                verticalVelocity = gravity * Time.fixedDeltaTime;
            }
            return;
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }
    }
    void Jumpable()
    {
        if (inputParameters.isJumping)
        {
            jumpable = !Physics.SphereCast(characterTransform.position + Vector3.up * characterController.height, characterController.radius, Vector3.up,out RaycastHit hit,jumpableHeight- characterController.height);
        }
    }
    /// <summary>
    /// ��Ծ
    /// </summary>
    void Jump()
    {
        if(characterPosture == CharacterPosture.Stand && inputParameters.isJumping && jumpable)
        {
            float velOffset = 0f;
            switch(locomotionState)
            {
                case LocomotionState.Run:
                    velOffset = 1f;
                    break;
                case LocomotionState.Walk:
                    velOffset = 0.5f;
                    break;
                case LocomotionState.Idle:
                    velOffset = 0f;
                    break;
            }
            switch(characterSensor.ClimbDetect(characterTransform,characterMovementWorldSpace,velOffset))
            {
                case CharacterSensor.NextCharacterMovement.Jump:
                    verticalVelocity = Mathf.Sqrt(-2*gravity*jumpHeight);
                    feetTween = Mathf.Repeat(characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime,1);
                    feetTween = feetTween < 0.5f ? 1 : -1;
                    switch(locomotionState)
                    {
                        case LocomotionState.Walk:
                              feetTween *= 2;
                              break;
                        case LocomotionState.Run:
                              feetTween *= 3;
                              break;
                        default:
                              feetTween = Random.Range(-0.5f, 1f);
                              break;
                    }
                    break;
                case CharacterSensor.NextCharacterMovement.ClimbLow:
                    isClimbReady = true;
                    currentClimbParameter = climbLowParameter;
                    leftHandPosition = characterSensor.ledge + Vector3.Cross(-characterSensor.climbHitNormal, Vector3.up) * 0.3f;
                    break;
                case CharacterSensor.NextCharacterMovement.ClimbHigh:
                    isClimbReady = true;
                    currentClimbParameter = climbHighParameter;
                    rightHandPosition = characterSensor.ledge + Vector3.Cross(characterSensor.climbHitNormal, Vector3.up) * 0.3f + characterSensor.climbHitNormal.normalized * 0.2f + Vector3.down*0.1f;
                    rightFootPosition = characterSensor.ledge + Vector3.down * 1.2f + characterSensor.climbHitNormal.normalized * 0.2f;
                    break;
                case CharacterSensor.NextCharacterMovement.Vault:
                    rightHandPosition = characterSensor.ledge + characterSensor.climbHitNormal.normalized * 0.2f; 
                    isClimbReady = true;
                    currentClimbParameter = vaultParameter;
                    break;
                default:
                    currentClimbParameter = defaultClimbParameter;
                    break;
            }
            
        }
    }
    IEnumerator JumpCoolDown()
    {
        landingThreshold = Mathf.Clamp(verticalVelocity, -10f, 0f);
        landingThreshold /= 20f;
        landingThreshold += 1f;
        isLanding = true;
        characterPosture = CharacterPosture.Landing;
        yield return new WaitForSeconds(jumpCoolDownTime);
        isLanding = false;
    }
    /// <summary>
    /// ���ö�����
    /// </summary>
    void SetupAnimator()
    {
        if (characterPosture == CharacterPosture.Stand)//վ��
        {
            characterAnimator.SetFloat(postureHash, standThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    characterAnimator.SetFloat(verticalHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    characterAnimator.SetFloat(verticalHash, characterMovement.z * walkspeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    characterAnimator.SetFloat(verticalHash, characterMovement.z * runspeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (characterPosture == CharacterPosture.Crouch)//Ǳ��
        {
            characterAnimator.SetFloat(postureHash, crouchThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    characterAnimator.SetFloat(verticalHash, 0, 0.1f, Time.deltaTime);
                    break;
                default:
                    characterAnimator.SetFloat(verticalHash, characterMovement.magnitude * crouchspeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (characterPosture == CharacterPosture.Jumping)//�Ϳ�
        {
            characterAnimator.SetFloat(postureHash, midairThreshold, 0.1f, Time.deltaTime);
            characterAnimator.SetFloat(jumpVerticalHash, verticalVelocity, 0.1f, Time.deltaTime);
            characterAnimator.SetFloat(feetTweenHash, feetTween, 0.1f, Time.deltaTime);
        }
        else if (characterPosture == CharacterPosture.Landing)//��½��ȴ
        {
            characterAnimator.SetFloat(postureHash, landingThreshold, 0.03f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    characterAnimator.SetFloat(verticalHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    characterAnimator.SetFloat(verticalHash, characterMovement.magnitude * walkspeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    characterAnimator.SetFloat(verticalHash, characterMovement.magnitude * runspeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (characterPosture == CharacterPosture.Falling)//׹��
        {
            characterAnimator.SetFloat(postureHash, midairThreshold, 0.1f, Time.deltaTime);
            characterAnimator.SetFloat(jumpVerticalHash, verticalVelocity, 0.1f, Time.deltaTime);
        }
        else if (characterPosture == CharacterPosture.Climbing)//����
        {
            characterAnimator.SetInteger(climbParameterHash, currentClimbParameter);
            if(characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Climb Low"))
            {
                currentClimbParameter = defaultClimbParameter;
                characterAnimator.MatchTarget(leftHandPosition, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.1f);
                characterAnimator.MatchTarget(leftHandPosition + Vector3.up * 0.18f, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.up, 0f), 0.1f, 0.3f); 
            }
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Climb High"))
            {
                currentClimbParameter = defaultClimbParameter;
                characterAnimator.MatchTarget(rightFootPosition, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.13f);
                characterAnimator.MatchTarget(rightHandPosition, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.2f, 0.32f);
            }
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Vault"))
            {
                currentClimbParameter = defaultClimbParameter;
                characterAnimator.MatchTarget(rightHandPosition + Vector3.up * 0.1f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.45f); 
            }
        }
        if (armState == ArmState.Normal)//����׼ת��
        {
            float rad = Mathf.Atan2(characterMovement.x, characterMovement.z);
            characterAnimator.SetFloat(turnspeedHash, rad, 0.1f, Time.deltaTime);
            characterTransform.Rotate(Vector3.up * rad * 200f * Time.deltaTime);
        }
        else if(armState == ArmState.FPSNormal)//��һ�˳�ת��
        {
            characterTransform.rotation = Quaternion.LookRotation(new Vector3(mainCameraTransform.forward.x, 0, mainCameraTransform.forward.z));
            switch (locomotionState)
            {
                case LocomotionState.Walk:
                    characterAnimator.SetFloat(horizontalHash, characterMovement.x*walkspeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    characterAnimator.SetFloat(horizontalHash, characterMovement.x * runspeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Idle:
                    characterAnimator.SetFloat(horizontalHash, 0, 0.1f, Time.deltaTime);
                    break;
            }
        }
        switch (controlMode)
        {
            case ControlMode.TPS:
                if(armState != ArmState.Aim)
                {
                    characterAnimator.SetInteger(controllerModeHash, 0);
                }
                break;
            case ControlMode.FPS:
                characterAnimator.SetInteger(controllerModeHash, 1);
                break;
            case ControlMode.OtherPlayer:
                if(armState != ArmState.Aim)
                {
                    characterAnimator.SetInteger(controllerModeHash, 0);
                }
                break;
        }
    }
    /// <summary>
    /// ��������ǰƽ���ٶ�
    /// </summary>
    /// <param name="newVel"></param>
    /// <returns></returns>
    Vector3 AverageVel(Vector3 newVel)
    {
        velCache[currentVelCacheIndex] = newVel;
        currentVelCacheIndex++;
        currentVelCacheIndex %= CACHE_SIZE;
        Vector3 average = Vector3.zero;
        foreach (Vector3 vel in velCache)
        {
            average += vel;
        }
        return average / CACHE_SIZE;
    }
    /// <summary>
    /// ������Ƹ������任
    /// </summary>
    private void OnAnimatorMove()
    {
        characterController.enabled = characterAnimator.GetInteger(climbParameterHash) == 0;
        if (controlMode == ControlMode.OtherPlayer)
            return;
        if(characterPosture == CharacterPosture.Climbing)
        {
            characterAnimator.ApplyBuiltinRootMotion();
        }
        else if(characterPosture != CharacterPosture.Jumping && characterPosture != CharacterPosture.Falling)
        {
            characterController.enabled = true;
            Vector3 animatorDeltaPosition = characterAnimator.deltaPosition;
            animatorDeltaPosition.y = verticalVelocity*Time.deltaTime;
            characterController.Move(animatorDeltaPosition);
            averageVel = AverageVel(characterAnimator.velocity);
        }
        else
        {
            characterController.enabled = true;
            averageVel.y = verticalVelocity;
            Vector3 animatorDeltaPosition = averageVel * Time.deltaTime;
            animatorDeltaPosition.y = verticalVelocity * Time.deltaTime;
            characterController.Move(animatorDeltaPosition);
        }
    }
}
/// <summary>
/// ���Ʋ�������
/// </summary>
[System.Serializable]
public class InputParameters
{
    public Vector2 moveInput;
    public bool isRunning;
    public bool isCrouching;
    public bool isAiming;
    public bool isJumping;
}