using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInputManager : MonoBehaviour
{
   public static VRInputManager Instance;

   [Header("VR Input Action References")] 
   [SerializeField] private InputActionReference primaryButtonAction;
   [SerializeField] private InputActionReference primary2DAxisAction;
   
   [FormerlySerializedAs("rightController")]
   [Header("XR Controller References")]
   [SerializeField] private Transform rightControllerTransform;
   
   private Vector2 joystickInput = Vector2.zero;
   private Vector2 joystickInputLastFrame = Vector2.zero;
   private bool joystickMoved = false;

   public System.Action OnPrimaryButtonPressed;
   public System.Action<Vector2> OnJoystickMove;
   public System.Action<int> OnJoystickDirectionChange;

   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         Destroy(gameObject);
      }
   }

   private void Start()
   {
      SetupInputActions();
      FindRightController();
   }

   private void SetupInputActions()
   {
      // a 버튼 이벤트 연결
      if (primaryButtonAction != null)
      {
         primaryButtonAction.action.performed += OnPrimaryButtonPerformed;
         primaryButtonAction.action.Enable();
      }
      else
      {
         Debug.LogWarning("primary Button Action 설정되지 않음");
      }
      
      // 조이스틱 이벤트 연결
      if (primary2DAxisAction != null)
      {
         primary2DAxisAction.action.performed += OnPrimary2DAxisPerformed;
         primary2DAxisAction.action.canceled += OnPrimary2DAxisCanceled;
         primary2DAxisAction.action.Enable();
      }
      else
      {
         Debug.LogWarning("Primary 2D Axis Action 설정되지 않음");
      }
   }

   private void FindRightController()
   {
      if (rightControllerTransform == null)
      {
         GameObject rightControllerObj = GameObject.Find("Right Controller");
         if (rightControllerObj != null)
         {
            rightControllerTransform = rightControllerObj.transform;
         }
      }
   }

   private void OnPrimaryButtonPerformed(InputAction.CallbackContext context)
   {
      OnPrimaryButtonPressed?.Invoke();
      Debug.Log("VR: A 버튼 눌림");
   }

   private void OnPrimary2DAxisPerformed(InputAction.CallbackContext context)
   {
      joystickInput = context.ReadValue<Vector2>();
      OnJoystickMove?.Invoke(joystickInput);
      joystickMoved = true;
   }

   private void OnPrimary2DAxisCanceled(InputAction.CallbackContext context)
   {
      joystickInput = Vector2.zero;
      joystickMoved = false;
   }
   
   private void Update()
   {
      CheckJoystickDirection();
      joystickInputLastFrame = joystickInput;
   }
   
   private void CheckJoystickDirection()
   {
      if (!joystickMoved)
         return;
      
      float threshold = 0.7f;

      if (joystickInput.y > threshold && joystickInputLastFrame.y <= threshold)
      {
         OnJoystickDirectionChange?.Invoke(-1);
         Debug.Log("VR: 조이스틱 위");
      }

      if (joystickInput.y < -threshold && joystickInputLastFrame.y >= -threshold)
      {
         OnJoystickDirectionChange?.Invoke(1);
         Debug.Log("VR: 조이스틱 아래");
      }
   }
   
   public Vector2 GetJoystickInput()
   {
      return joystickInput;
   }

   public bool IsControllerConnected()
   {
      return rightControllerTransform != null;
   }

   public Transform GetRightControllerTransform()
   {
      return rightControllerTransform;
   }

   private void OnDestroy()
   {
      if (primaryButtonAction != null)
      {
         primaryButtonAction.action.performed -= OnPrimaryButtonPerformed;
         primaryButtonAction.action.Disable();
      }

      if (primary2DAxisAction != null)
      {
         primary2DAxisAction.action.performed -= OnPrimary2DAxisPerformed;
         primary2DAxisAction.action.canceled -= OnPrimary2DAxisCanceled;
         primary2DAxisAction.action.Disable();
      }
   }
}
