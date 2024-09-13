using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    //足部IK位置
    Vector3 leftPos;
    Vector3 rightPos;
    //足部IK旋转
    Quaternion leftRot;
    Quaternion rightRot;
    //足部原位置
    Vector3 rawLeftPos;
    Vector3 rawRightPos;
    //参数
    public float scanDistance;//射线检测距离
    public bool useFootIK = true;//使用FootIK
    [Range(0, 1f)]
    public float footOffset;//足部位置偏移量
    //是否着地
    public bool leftIsGrounded;
    public bool rightIsGrounded;
    //权重
    float leftWeight;
    float rightWeight;

    Animator animator;
    CharacterController characterController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        rawLeftPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        rawRightPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        if(useFootIK)
        {
            GetGroundInfo(rawLeftPos,out leftPos,out leftRot,out leftIsGrounded);
            GetGroundInfo(rawRightPos, out rightPos,out rightRot, out rightIsGrounded);
            if(leftIsGrounded)
            {
                leftWeight = Mathf.Lerp(leftWeight, 1, 0.001f);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftWeight);
            }
            if (rightIsGrounded)
            {
                rightWeight = Mathf.Lerp(rightWeight, 1, 0.001f);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightWeight);
            }
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftPos);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightPos);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftRot);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightRot);
        }
    }//获取地面信息
    private void GetGroundInfo(Vector3 origin,out Vector3 ikPos,out Quaternion ikRot,out bool isGrounded)
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(origin+Vector3.up*0.2f, Vector3.down, out hit, scanDistance);
        if(isGrounded)
        {
            ikPos = hit.point + Vector3.up * footOffset;
            ikRot = Quaternion.FromToRotation(Vector3.up,hit.normal)*transform.rotation;
        }
        else
        {
            ikPos = origin;
            ikRot = Quaternion.identity;
        }
    }
}
