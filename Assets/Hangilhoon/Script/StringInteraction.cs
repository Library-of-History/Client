using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Interactables 네임스페이스 포함

public class StringInteraction : XRBaseInteractable
{
    [SerializeField] public Transform stringStartPoint; // 활 시위의 고정된 시작점
    [SerializeField] public Transform stringEndPoint;   // 활 시위의 고정된 끝점
    [SerializeField] private GameObject arrowPrefab;    // 생성할 화살 프리팹
    [SerializeField] private Transform arrowSocket;     // 화살이 부착될 소켓 트랜스폼 (시위와 함께 움직일 대상)
    //[SerializeField] private LineRenderer bowStringRenderer; // 활 시위를 그릴 LineRenderer

    private GameObject currentArrow;
    private ArrowInteraction currentArrowInteraction;

    private IXRInteractor _stringInteractor = null; // IXRInteractor 사용 권장
    
    private Vector3 _pullPosition;
    private Vector3 _pullDirection;
    private Vector3 _targetDirection;

    public float PullAmount { get; private set; } = 0.0f;

    // 월드 포지션 속성은 그대로 유지
    public Vector3 stringStartWorldPosition { get => stringStartPoint.position; }
    public Vector3 stringEndWorldPosition { get => stringEndPoint.position; }

    protected override void Awake()
    {
        base.Awake();
        // LineRenderer의 시작점과 끝점 설정 (초기 활 시위 모습)
        // if (bowStringRenderer != null && bowStringRenderer.positionCount >= 2)
        // {
        //     bowStringRenderer.SetPosition(0, stringStartPoint.localPosition);
        //     // bowStringRenderer.SetPosition(1, stringEndPoint.localPosition); // 1번 인덱스는 당겨지는 중간점
        // }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // 인터랙터 할당 (IXRInteractor로 캐스팅)
        _stringInteractor = args.interactorObject as IXRInteractor;

        // 화살 생성 및 시위에 장전
        if (arrowPrefab != null && currentArrow == null)
        {
            // 화살을 stringEndPoint가 아닌 arrowSocket에 인스턴스화
            currentArrow = Instantiate(arrowPrefab, arrowSocket.position, Quaternion.identity);
            currentArrow.transform.SetParent(arrowSocket); // arrowSocket의 자식으로 설정하여 함께 움직이게 함
            currentArrowInteraction = currentArrow.GetComponent<ArrowInteraction>();

            // Rigidbody 비활성화(장전 중)
            var rb = currentArrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            // 화살의 Z축(앞 방향)이 활의 반대 방향을 바라보도록 설정
            Vector3 bowBackwardDirection = (stringStartPoint.position - stringEndPoint.position).normalized;
            currentArrow.transform.rotation = Quaternion.LookRotation(bowBackwardDirection);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // 화살 발사
        if (currentArrow != null && currentArrowInteraction != null)
        {
            currentArrow.transform.SetParent(null); // 부모 해제
            var rb = currentArrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            
            // 발사 방향은 시위 시작점에서 끝점 방향으로, 당겨진 정도에 따라 힘을 가함
            // PullAmount가 이미 0~1 사이로 클램프되어 있으므로 그대로 사용
            currentArrowInteraction.ReleaseArrow(PullAmount); // ArrowInteraction의 발사 로직 사용

            currentArrow = null;
            currentArrowInteraction = null;
        }

        // 인터랙터 참조 해제 및 PullAmount 초기화
        _stringInteractor = null;
        PullAmount = 0.0f;

        // 시위 렌더러 초기화 (시위를 놓았을 때 원래 위치로 돌아가도록)
        // if (bowStringRenderer != null && bowStringRenderer.positionCount >= 2)
        // {
        //     // 시위의 중간점을 시작점과 끝점의 중간으로 설정하거나, stringEndPoint로 설정
        //     bowStringRenderer.SetPosition(1, stringEndPoint.localPosition); 
        // }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        
        if (this._stringInteractor != null)
        {
            this._pullPosition = this._stringInteractor.transform.position;
            this.PullAmount = CalculatePull(this._pullPosition);

            // 화살이 장전되어 있으면 시위 위치에 맞춰 이동/회전
            if (currentArrow != null)
            {
                // 화살의 Z축(앞 방향)이 활의 반대 방향을 바라보도록 설정
                Vector3 bowBackwardDirection = (stringStartPoint.position - stringEndPoint.position).normalized;
                currentArrow.transform.rotation = Quaternion.LookRotation(bowBackwardDirection);

                // 중요: 화살의 위치는 stringStartPoint와 stringEndPoint 사이를 PullAmount에 따라 보간
                // arrowSocket은 활의 '정지' 위치에 화살을 두는 역할을 하고,
                // 여기서 currentArrow.transform.position은 당겨진 시위 위치를 따르도록 합니다.
                currentArrow.transform.position = Vector3.Lerp(stringStartPoint.position, stringEndPoint.position, PullAmount);

                // 만약 화살이 socketTransform의 자식으로 설정되어 있다면, localPosition을 업데이트해야 할 수 있습니다.
                // currentArrow.transform.localPosition = Vector3.Lerp(stringStartPoint.localPosition, stringEndPoint.localPosition, PullAmount);
                // 이 경우, stringStartPoint와 stringEndPoint도 localPosition으로 변경해야 함
            }
            
        }
    }

    private float CalculatePull(Vector3 currentPullWorldPosition)
    {
        this._pullDirection = currentPullWorldPosition - stringStartPoint.position;
        this._targetDirection = stringEndPoint.position - stringStartPoint.position;
        float maxLength = _targetDirection.magnitude;
        
        if (maxLength < Mathf.Epsilon) // 0으로 나누는 것을 방지
        {
            return 0f;
        }

        _targetDirection.Normalize();

        float pullValue = Vector3.Dot(_pullDirection, _targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0, 1);
    }
}