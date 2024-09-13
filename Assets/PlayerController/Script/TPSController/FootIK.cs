using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    //�㲿IKλ��
    Vector3 leftPos;
    Vector3 rightPos;
    //�㲿IK��ת
    Quaternion leftRot;
    Quaternion rightRot;
    //�㲿ԭλ��
    Vector3 rawLeftPos;
    Vector3 rawRightPos;
    //����
    public float scanDistance;//���߼�����
    public bool useFootIK = true;//ʹ��FootIK
    [Range(0, 1f)]
    public float footOffset;//�㲿λ��ƫ����
    //�Ƿ��ŵ�
    public bool leftIsGrounded;
    public bool rightIsGrounded;
    //Ȩ��
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
    }//��ȡ������Ϣ
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
