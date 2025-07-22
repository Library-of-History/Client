using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HammerStone : MonoBehaviour
{
    [Header("물리 속성")]
    public float mass = 1f;

    [Header("햅틱 피드백 설정")]
    public bool enableHaptics = true;
    public float minHapticForce = 3f;
    
    [Header("충돌 감지")] 
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    private float swingPower = 0f;
    
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor;
    private void Start()
    {
        // Rigidbody 설정
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // XR Grab 설정
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (!grabInteractable)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();   
        }
        
        // 이벤트 연결
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 속도 계산
        currentVelocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        
        // 가속도 기반 스윙 파워 계산
        float acceleration = currentVelocity.magnitude - previousPosition.magnitude;

        if (acceleration > 5f) // 가속 중일 때
        {
            swingPower = Mathf.Min(swingPower + acceleration * Time.fixedDeltaTime * 10f, 100f);
        }
        else // 감속 중일 때
        {
            swingPower = Mathf.Max(0, swingPower - 30f * Time.fixedDeltaTime);
        }

        previousPosition = transform.position;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        currentInteractor = null;
    }
    
    // 충돌 시 힘 전달
    public float GetImpactForce()
    {
        //현재 속도 + 누적된 스윙 파워
        float velocityForce = currentVelocity.magnitude * mass;
        return (velocityForce + swingPower) * 0.5f;
    }

    public void SendHapticFeedback(float intensity, float duration)
    {
        if (!enableHaptics || currentInteractor == null)
        {
            return;
        }
        
        HapticUtil.SendHaptic(currentInteractor, intensity, duration);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
