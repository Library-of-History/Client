using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class HandAnimation : MonoBehaviour
{
    public InputActionProperty leftPinch;
    public InputActionProperty leftGrip;

    public InputActionProperty rightPinch;
    public InputActionProperty rightGrip;

    public Animator animator;

    void Update()
    {
        var leftTriggerValue = leftPinch.action.ReadValue<float>();
         animator.SetFloat("Left Trigger", leftTriggerValue);

         var leftGripValue = leftGrip.action.ReadValue<float>();
         animator.SetFloat("Left Grip", leftGripValue);
        
        
        var rightTriggerValue = rightPinch.action.ReadValue<float>();
         animator.SetFloat("Right Trigger", rightTriggerValue);
        
        var rightGripValue = rightGrip.action.ReadValue<float>();
        animator.SetFloat("Right Grip", rightGripValue);
        
    }
}
