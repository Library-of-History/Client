using System.Collections; // Coroutine을 위해 필요
using UnityEngine; // MonoBehaviour, Vector3, Quaternion 등을 위해 필요
 // XR Interaction Toolkit 관련 클래스
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRGrabInteractable 등을 위해 필요

// 이 스크립트가 부착된 GameObject에는 XRGrabInteractable 컴포넌트가 필수적임을 명시합니다.
[RequireComponent(typeof(XRGrabInteractable))]
public class ArrowInteraction : MonoBehaviour
{
    // 필드 변수명 C# 컨벤션에 맞게 변경 (camelCase)
    private XRGrabInteractable xrGrabInteractable = null;
    private bool isInAir = false; // 화살이 공중에 있는지 여부
    private Vector3 lastTipPosition = Vector3.zero; // 이전 FixedUpdate에서의 화살촉 위치
    private Rigidbody arrowRigidbody = null; // 화살의 Rigidbody 컴포넌트

    [Header("Arrow Settings")] // Inspector에서 설정을 구분하기 위한 헤더
    [SerializeField] private float launchSpeedMultiplier = 1.0f; // 화살 발사 속도에 곱해질 계수 (이전 'speed')
    [SerializeField] private Transform tipTransform; // 화살촉의 Transform (충돌 감지에 사용) (이전 'tipPosition')
    [SerializeField] private LayerMask collisionLayers; // 화살이 충돌하여 꽂힐 수 있는 레이어 마스크 (Inspector에서 설정)
    [SerializeField] private float minLinecastDistance = 0.01f; // 라인캐스트 최소 거리 (너무 짧은 거리는 무시)

    private void Awake()
    {
        // 컴포넌트 참조 가져오기
        arrowRigidbody = GetComponent<Rigidbody>();
        xrGrabInteractable = GetComponent<XRGrabInteractable>();

        // 초기 상태 설정
        isInAir = false;
        // tipTransform이 할당되지 않은 경우를 대비한 널 체크
        lastTipPosition = (tipTransform != null) ? tipTransform.position : transform.position; 
        
        // Rigidbody 보간 모드 설정 (부드러운 움직임)
        arrowRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        
        // 시작 시 화살의 물리 비활성화 (활에 장전된 상태)
        SetPhysics(false); 
        
        // 화살이 처음에는 잡을 수 있도록 XRGrabInteractable 컴포넌트를 활성화합니다.
        // 콜라이더는 SetPhysics(false)에서 isKinematic이 true가 되면서 활성화됩니다.
        if (xrGrabInteractable != null)
        {
            xrGrabInteractable.enabled = true; // 잡을 수 있도록 Interactable 활성화
        }
    }         

    private void FixedUpdate()
    {
        // 화살이 공중에 있을 때만 충돌 감지 및 위치 업데이트
        if (isInAir)
        {
            CheckCollision();
            // 다음 FixedUpdate를 위해 현재 화살촉 위치를 저장합니다.
            // tipTransform이 null일 경우를 대비한 체크
            lastTipPosition = (tipTransform != null) ? tipTransform.position : transform.position;
        }
    }

    private void CheckCollision()
    {
        // tipTransform이 유효한지 확인
        if (tipTransform == null)
        {
            Debug.LogWarning("ArrowInteraction: tipTransform이 할당되지 않았습니다. 충돌 감지 불가.");
            return;
        }

        // 라인캐스트 시작점과 끝점 사이의 거리가 너무 짧으면 불필요한 체크를 피합니다.
        float distance = Vector3.Distance(lastTipPosition, tipTransform.position);
        if (distance < minLinecastDistance)
        {
            return;
        }

        // Physics.Linecast를 사용하여 이전 위치부터 현재 화살촉 위치까지 충돌 감지
        // collisionLayers에 설정된 레이어만 감지합니다.
        if (Physics.Linecast(lastTipPosition, tipTransform.position, out RaycastHit hitInfo, collisionLayers))
        {
            // 충돌한 대상에 Rigidbody가 있다면
            if (hitInfo.rigidbody != null) 
            {
                // 화살 Rigidbody의 보간을 해제하여 꽂히는 순간 정확한 위치에 고정
                arrowRigidbody.interpolation = RigidbodyInterpolation.None;
                
                // 화살을 충돌한 대상의 자식으로 설정하여 함께 움직이도록 합니다.
                transform.SetParent(hitInfo.transform); 

                // 꽂힌 대상에 화살의 운동량(linearVelocity)을 힘으로 전달합니다.
                hitInfo.rigidbody.AddForce(arrowRigidbody.linearVelocity, ForceMode.Impulse);
            }
            StopArrow(); // 충돌 발생 시 화살 정지
        }
    }

    private void StopArrow()
    {
        isInAir = false; // 공중 상태 해제
        SetPhysics(false); // 물리 비활성화 (중력 끄고 Kinematic으로)
        
        // 화살이 꽂힌 후 다시 잡을 수 있도록 XRGrabInteractable 컴포넌트를 활성화합니다.
        // 콜라이더는 SetPhysics(false)에서 isKinematic이 true가 되면서 활성화됩니다.
        if (xrGrabInteractable != null)
        {
            xrGrabInteractable.enabled = true; // 다시 잡을 수 있도록 Interactable 활성화
        }
    }

    // 화살의 물리 상태를 설정하는 유틸리티 메서드
    private void SetPhysics(bool usePhysics)
    {
        arrowRigidbody.useGravity = usePhysics; // 중력 사용 여부
        arrowRigidbody.isKinematic = !usePhysics; // usePhysics가 true면 isKinematic은 false (물리 활성화), false면 true (물리 비활성화)
        
        // 물리 비활성화 시 속도 초기화 (잔여 속도 방지)
        if (!usePhysics)
        {
            arrowRigidbody.linearVelocity = Vector3.zero;
            arrowRigidbody.angularVelocity = Vector3.zero;
        }
    }

    // 활시위 당김 정도(powerValue)를 받아 화살을 발사하는 public 메서드
    public void ReleaseArrow(float powerValue) // 매개변수명 명확화 (value -> powerValue)
    {
        isInAir = true; // 화살을 공중 상태로 설정
        SetPhysics(true); // 물리 활성화
        MaskAndFire(powerValue); // 화살 발사 및 콜라이더/상호작용 처리
        StartCoroutine(RotateWithVelocity()); // 속도에 맞춰 회전하는 코루틴 시작
        
        // 발사 직전 화살촉 위치를 저장하여 다음 FixedUpdate에서 라인캐스트 시작점으로 사용
        lastTipPosition = (tipTransform != null) ? tipTransform.position : transform.position;
    }

    // 화살 발사 및 상호작용 관련 처리
    private void MaskAndFire(float power)
    {
        // 발사 시 XRGrabInteractable 컴포넌트를 비활성화하여 다시 잡히는 것을 방지합니다.
        // 이 방법이 interactionLayerMask를 사용하는 것보다 더 명확하고 권장됩니다.
        if (xrGrabInteractable != null)
        {
            xrGrabInteractable.enabled = false; 
        }
        
        // 계산된 힘으로 화살 Rigidbody에 즉각적인 힘을 가합니다.
        Vector3 force = transform.forward * power * launchSpeedMultiplier; 
        arrowRigidbody.AddForce(force, ForceMode.Impulse);
    }

    // 화살의 속도 방향으로 회전하는 코루틴
    private IEnumerator RotateWithVelocity()
    {
        // Rigidbody의 linearVelocity가 정확한 값을 가질 때까지 기다립니다.
        yield return new WaitForFixedUpdate(); 
        
        while(isInAir) // 화살이 공중에 있는 동안 계속 회전
        {
            // 화살이 거의 정지해 있지 않고, 유효한 속도를 가질 때만 회전 계산
            if (arrowRigidbody.linearVelocity.sqrMagnitude > Mathf.Epsilon) // 속도가 0에 가까운지 체크
            {
                // Rigidbody의 현재 선형 속도 방향으로 화살을 회전시킵니다.
                Quaternion newRotation = Quaternion.LookRotation(arrowRigidbody.linearVelocity);
                // 부드러운 회전을 위해 Quaternion.Slerp 사용도 고려할 수 있습니다.
                // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
                this.transform.rotation = newRotation;
            }
            yield return null; // 다음 프레임까지 대기 (Update 루프에 맞춰)
        }
    }
}