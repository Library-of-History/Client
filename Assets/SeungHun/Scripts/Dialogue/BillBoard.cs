using UnityEngine;

public class SimpleVRBillboard : MonoBehaviour
{
    [Header("VR Camera Settings")]
    public Transform vrCameraTransform;
    
    [Header("Billboard Settings")]
    [Tooltip("Y축 회전을 고정")]
    public bool lockY = true;
    
    [Tooltip("부드러운 회전을 사용합니다")]
    public bool smoothRotation = true;
    
    [Tooltip("부드러운 회전 속도")]
    public float rotationSpeed = 5f;
    
    [Header("Performance Settings")]
    [Tooltip("업데이트 간격 (초)")]
    public float updateInterval = 0.1f;
    
    [Tooltip("최대 표시 거리 (0이면 거리 제한 없음)")]
    public float maxDistance = 0f;
    
    private float lastUpdateTime;
    private Quaternion targetRotation;
    
    private void Start()
    {
        if (vrCameraTransform == null)
        {
            FindVRCamera();
        }
        
        targetRotation = transform.rotation;
    }
    
    private void FindVRCamera()
    {
        if (Camera.main != null)
        {
            vrCameraTransform = Camera.main.transform;
            Debug.Log("VR Camera found: Main Camera");
            return;
        }
        
        GameObject mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObj != null)
        {
            vrCameraTransform = mainCameraObj.transform;
            Debug.Log("VR Camera found: MainCamera tag");
            return;
        }
        
        Camera firstCamera = FindFirstObjectByType<Camera>();
        if (firstCamera != null)
        {
            vrCameraTransform = firstCamera.transform;
            Debug.Log("VR Camera found: " + firstCamera.name);
            return;
        }
    }
    
    private void Update()
    {
        if (vrCameraTransform == null)
        {
            FindVRCamera();
            return;
        }
        
        if (Time.time - lastUpdateTime < updateInterval)
        {
            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                                    Time.deltaTime * rotationSpeed);
            }
            return;
        }
        
        lastUpdateTime = Time.time;
        
        if (maxDistance > 0f)
        {
            float distance = Vector3.Distance(transform.position, vrCameraTransform.position);
            if (distance > maxDistance)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        
        CalculateBillboardRotation();
        
        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }
    }
    
    private void CalculateBillboardRotation()
    {
        Vector3 targetPosition = vrCameraTransform.position;
        
        if (lockY)
        {
            targetPosition.y = transform.position.y;
        }
        
        Vector3 direction = targetPosition - transform.position;
        
        if (direction.magnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }
    }
    
    public void SetVRCamera(Transform newCamera)
    {
        vrCameraTransform = newCamera;
    }
}