using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorController : MonoBehaviour
{
   public InputActionReference triggerActionReference;
   public InputActionReference gripActionReference;

   public Animator handAnimator;
   
   // 애니메이션 블렌드 속도
   [SerializeField] private float animationBlendSpeed = 10f;
   
   // 현재 값과 목표 값
   private float currentPinchValue;
   private float currentFlexValue;
   private float targetPinchValue;
   private float targetFlexValue;

   private void Awake()
   {
      // 애니메이터 할당 확인
      if (handAnimator == null)
      {
         handAnimator = GetComponent<Animator>();
      }
      
      SetupInputActions();
   }

   private void SetupInputActions()
   {
      if (triggerActionReference != null && gripActionReference != null)
      {
         triggerActionReference.action.performed += OnTriggerAction;
         triggerActionReference.action.canceled += OnTriggerAction;
         
         gripActionReference.action.performed += OnGripAction;
         gripActionReference.action.canceled += OnGripAction;
      }
      else
      {
         Debug.LogWarning("Input Action Reference가 인스펙터에 설정되지 않았습니다.");
      }
   }
   
   private void OnTriggerAction(InputAction.CallbackContext context)
   {
      targetPinchValue = context.ReadValue<float>();
   }
   
   private void OnGripAction(InputAction.CallbackContext context)
   {
      targetFlexValue = context.ReadValue<float>();
   }
   
   private void Update()
   {
      // 애니메이션 값 부드럽게 적용
      UpdateAnimationValues();
   }
   
   private void UpdateAnimationValues()
   {
      if (handAnimator == null) return;
      
      // 값 부드럽게 변경
      currentPinchValue = Mathf.Lerp(currentPinchValue, targetPinchValue, Time.deltaTime * animationBlendSpeed);
      currentFlexValue = Mathf.Lerp(currentFlexValue, targetFlexValue, Time.deltaTime * animationBlendSpeed);
      
      // 애니메이터에 값 적용
      handAnimator.SetFloat("Pinch", currentPinchValue);
      handAnimator.SetFloat("Flex", currentFlexValue);
   }

   private void OnEnable()
   {
      triggerActionReference?.action.Enable();
      gripActionReference?.action.Enable();
   }

   private void OnDisable()
   {
      triggerActionReference?.action.Disable();
      gripActionReference?.action.Disable();
   }
}
