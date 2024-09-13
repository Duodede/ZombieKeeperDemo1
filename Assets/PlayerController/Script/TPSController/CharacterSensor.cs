using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSensor : MonoBehaviour
{
    /// <summary>
    /// 确定玩家下一步执行动作
    /// </summary>
    public enum NextCharacterMovement
    {
        Jump,
        ClimbLow,
        ClimbHigh,
        Vault,
    }
    public NextCharacterMovement nextMovement = NextCharacterMovement.Jump;
    //攀爬参数
    public float lowClimbHeight = 0.5f;
    public float hightClimbHeight = 1.6f;
    public float checkDistance = 1.0f;
    public float bodyHeight = 1.0f;
    public Vector3 climbHitNormal;
    float climbDistance;
    float climbAngle = 45f;
    public Vector3 ledge;
    private void Start()
    {
        
    }
    /// <summary>
    /// 攀爬检测
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public NextCharacterMovement ClimbDetect(Transform characterTransform,Vector3 inputDirection,float offset)
    {
        climbDistance = Mathf.Cos(climbAngle)*checkDistance;
        if (Physics.Raycast(characterTransform.position + Vector3.up * lowClimbHeight, characterTransform.forward, out RaycastHit obsHit, checkDistance + offset))
        {
            climbHitNormal = obsHit.normal;
            if(Vector3.Angle(-climbHitNormal,characterTransform.forward)>climbAngle || Vector3.Angle(-climbHitNormal,inputDirection)>climbAngle)
            {
                return NextCharacterMovement.Jump;
            }
            if (Physics.Raycast(characterTransform.position + Vector3.up * lowClimbHeight, -climbHitNormal, out RaycastHit firstWallHit, climbDistance + offset))
            {
                 if (Physics.Raycast(characterTransform.position + Vector3.up * (lowClimbHeight + bodyHeight), -climbHitNormal, out RaycastHit secondWallHit, climbDistance + offset))
                 {
                     if (Physics.Raycast(characterTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 2), -climbHitNormal, out RaycastHit thirdWallHit, climbDistance + offset))
                     {
                         if (Physics.Raycast(characterTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 3), -climbHitNormal, climbDistance + offset))
                         {
                             
                             return NextCharacterMovement.Jump;
                         }
                         else if (Physics.Raycast(thirdWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                         {
                             ledge = ledgeHit.point;
                             return NextCharacterMovement.ClimbHigh;
                         }
                     }
                     else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                     {
                         ledge = ledgeHit.point;
                         if (ledge.y - characterTransform.position.y > hightClimbHeight)
                         {
                             return NextCharacterMovement.ClimbHigh;
                         }
                         else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f, Vector3.down, bodyHeight))
                         {
                             return NextCharacterMovement.ClimbLow;
                         }
                         else
                         {
                             return NextCharacterMovement.Vault;
                         }
                     }
                 }
                else if (Physics.Raycast(firstWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                {
                    ledge = ledgeHit.point;
                    if (ledge.y - characterTransform.position.y > hightClimbHeight)
                    {
                        return NextCharacterMovement.ClimbHigh;
                    }
                    else if (Physics.Raycast(firstWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f, Vector3.down, bodyHeight))
                    {
                        return NextCharacterMovement.ClimbLow;
                    }
                    else
                    {
                        return NextCharacterMovement.Vault;
                    }
                }
            }
            else if (Physics.Raycast(obsHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
            {
                ledge = ledgeHit.point;
                if (ledge.y - characterTransform.position.y > hightClimbHeight)
                {
                    return NextCharacterMovement.ClimbHigh;
                }
                else if (Physics.Raycast(obsHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f, Vector3.down, bodyHeight))
                {
                    return NextCharacterMovement.ClimbLow;
                }
                else
                {
                    return NextCharacterMovement.Vault;
                }
            }
        }
        return NextCharacterMovement.Jump;
    }
}
