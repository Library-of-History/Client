using UnityEngine;

public class VRPlayerTracker : MonoBehaviour
{
    [Header("VR 컴포넌트")]
    [Tooltip("VR 헤드셋 (Main Camera)")]
    public Transform headTransform;

    [Tooltip("몸체 Transform (XR Origin)")] 
    public Transform bodyTransform;

    [Header("추적 설정")] 
    public float lookAtHeightOffset = 0.2f;

    public float bodyHeightOffset = 1f;

    public bool showDebugInfo = false;

    private Vector3 cachedHeadPosition;
    private Vector3 cachedBodyPosition;
    private Vector3 cachedLookAtPosition;

    private void Start()
    {
        AutoSetupVRComponents();
        ValidateSetup();
    }

    private void Update()
    {
        UpdateCachedPositions();
    }

    private void AutoSetupVRComponents()
    {
        if (headTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                headTransform = mainCamera.transform;
            }
        }

        if (bodyTransform == null)
        {
            bodyTransform = transform;
        }
    }

    private void ValidateSetup()
    {
        if (headTransform == null)
        {
            Debug.LogError("Head Transform 설정되지 않음");
        }

        if (bodyTransform == null)
        {
            Debug.LogError("Body Transform이 설정되지 않음");
        }
    }

    private void UpdateCachedPositions()
    {
        if (headTransform != null)
        {
            cachedHeadPosition = headTransform.position;
            cachedLookAtPosition = headTransform.position + Vector3.down * lookAtHeightOffset;
        }

        if (bodyTransform != null)
        {
            cachedBodyPosition = bodyTransform.position + Vector3.up * bodyHeightOffset;
        }
    }

    public Vector3 GetLookAtPosition()
    {
        return cachedLookAtPosition;
    }

    public Vector3 GetBodyPosition()
    {
        return cachedBodyPosition;
    }

    public Vector3 GetHeadPosition()
    {
        return cachedHeadPosition;
    }

    public bool IsLookingAt(Vector3 targetPosition, float angleThreshold = 45f)
    {
        if (headTransform == null)
            return false;

        Vector3 directionToTarget = (targetPosition - headTransform.position).normalized;
        Vector3 headForward = headTransform.forward;

        float angle = Vector3.Angle(headForward, directionToTarget);
        return angle <= angleThreshold;
    }

    public float GetDistanceTo(Vector3 targetPosition)
    {
        return Vector3.Distance(cachedBodyPosition, targetPosition);
    }
}
