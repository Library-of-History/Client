using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 

// 이 스크립트가 부착된 GameObject에는 StringInteraction 컴포넌트가 필수적임을 명시합니다.
[RequireComponent(typeof(StringInteraction))]
public class SocketInteraction : XRSocketInteractor
{
    // 화살을 잡고 있는 손 Interactor (IXRInteractor로 변경)
    private XRBaseInteractor handHoldingArrow = null;
    private XRBaseInteractable currentArrow = null; // 변경된 변수명 적용
    private StringInteraction stringInteraction = null;
    
    // bowInteraction 변수를 선언하고, Inspector에서 할당할 수 있도록 [SerializeField] 추가
    [SerializeField] private BowInteraction bowInteraction = null; 

    private ArrowInteraction currentArrowInteraction = null;
   

    protected override void Awake()
    {
        base.Awake();
        stringInteraction = GetComponent<StringInteraction>(); // StringInteraction은 같은 GameObject에 있을 것으로 예상
        
        // bowInteraction이 Inspector에서 할당되지 않았다면 경고
        if (bowInteraction == null) // bowComponent -> bowInteraction 변경
        {
            Debug.LogWarning("SocketInteraction: Bow Component가 할당되지 않았습니다. 화살 발사 로직에 문제 발생 가능.", this);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // StringInteraction이 선택 해제될 때(활시위 놓을 때) 화살 발사 시도
        if (stringInteraction != null)
        {
            stringInteraction.selectExited.AddListener(OnStringInteractionReleased);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // 이벤트 리스너 해지
        if (stringInteraction != null)
        {
            stringInteraction.selectExited.RemoveListener(OnStringInteractionReleased);
        }
    }

   protected override void OnHoverEntered(HoverEnterEventArgs args)
{
    base.OnHoverEntered(args);
    // 수정 전: Debug.Log($"[SocketInteraction] OnHoverEntered: {args.interactableObject.gameObject.name} 호버링됨.");
    // 수정 후: args.interactableObject를 Component 타입으로 캐스팅하여 gameObject에 접근합니다.
    // 대부분의 경우 args.interactableObject는 XRBaseInteractable을 상속하므로, XRBaseInteractable로 캐스팅하는 것이 더 명확합니다.
    if (args.interactableObject is XRBaseInteractable xrInteractable)
    {
        Debug.Log($"[SocketInteraction] OnHoverEntered: {xrInteractable.gameObject.name} 호버링됨.");
    }
    else // XRBaseInteractable이 아닌 경우를 대비한 방어 코드 (선택 사항)
    {
        Debug.LogWarning($"[SocketInteraction] OnHoverEntered: 호버링된 객체가 XRBaseInteractable이 아닙니다: {args.interactableObject.GetType().Name}");
        return; // 더 이상 진행하지 않음
    }


    if (args.interactableObject is XRBaseInteractable hoveredInteractable)
    {
        Debug.Log($"[SocketInteraction] 호버링된 객체 태그: {hoveredInteractable.gameObject.tag}"); // 여기는 이미 XRBaseInteractable이므로 문제 없음
        Debug.Log($"[SocketInteraction] 활 컴포넌트 유효: {bowInteraction != null}");
        if (bowInteraction != null)
        {
            Debug.Log($"[SocketInteraction] 활 잡힘 상태 (BowHeld): {bowInteraction.BowHeld}");
        }

        if (hoveredInteractable.gameObject.CompareTag("Arrow") && // 여기는 이미 XRBaseInteractable이므로 문제 없음
            bowInteraction != null &&
            bowInteraction.BowHeld)
        {
            Debug.Log("[SocketInteraction] 화살 장전 조건 충족 (태그, 활 잡힘). 장전 시도.");
            if (!hasSelection)
            {
                Debug.Log("[SocketInteraction] 소켓이 비어있음. SelectExit/SelectEnter 호출.");
                interactionManager.SelectExit(args.interactorObject as IXRSelectInteractor, hoveredInteractable);
                interactionManager.SelectEnter(this, hoveredInteractable as IXRSelectInteractable);
            }
            else
            {
                Debug.Log("[SocketInteraction] 소켓에 이미 다른 화살이 장전되어 있음.");
            }
        }
        else
        {
            // 이 else 문에 진입한다면, 어떤 조건이 충족되지 않았는지 특정할 수 있습니다.
            Debug.Log("[SocketInteraction] 화살 장전 조건 미충족. 조건: " +
                      $"\n  - 화살 태그: {hoveredInteractable.gameObject.CompareTag("Arrow")}" +
                      $"\n  - 활 컴포넌트: {(bowInteraction != null)}" +
                      $"\n  - 활 잡힘: {(bowInteraction != null && bowInteraction.BowHeld)}");
        }
    }
}

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // Debug.Log($"[SocketInteraction] OnSelectEntered: 화살이 소켓에 최종적으로 장전됨: {args.interactableObject.gameObject.name}");
        StoreArrow(args.interactableObject as XRBaseInteractable);
    }

    // 소켓에서 화살이 선택 해제되었을 때 (발사 또는 수동 분리)
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // 화살이 소켓에서 분리되면 변수 초기화
        ClearVariables();
        // Debug.Log("화살이 소켓에서 분리됨!");
    }

    // 소켓에 들어온 화살의 정보 저장
    private void StoreArrow(XRBaseInteractable interactable)
    {
        // currentArrowInSocket 대신 currentArrow 사용
        if (interactable != null && interactable.CompareTag("Arrow")) // 널 체크 추가
        {
            currentArrow = interactable;
            // GetComponent 대신 TryGetComponent를 사용하여 안전하게 가져오기
            // 이전에 currentArrowInteraction.gameObject라고 되어 있었는데, currentArrow가 맞는 것 같습니다.
            if (currentArrow.gameObject.TryGetComponent(out ArrowInteraction arrowInteraction))
            {
                currentArrowInteraction = arrowInteraction;
            }
            else
            {
                Debug.LogWarning("SocketInteraction: 소켓에 들어온 화살에 ArrowInteraction 컴포넌트가 없습니다.", interactable);
            }
        } else { // interactable이 null이거나 태그가 "Arrow"가 아닐 경우
            Debug.LogWarning("SocketInteraction: 장전하려는 객체가 유효한 화살이 아닙니다.", interactable);
        }
    }

    // StringInteraction이 놓였을 때(활시위 놓을 때) 호출되는 리스너 메서드
    private void OnStringInteractionReleased(SelectExitEventArgs args)
    {
        // 소켓에 화살이 있고, 활이 잡혀있고, 활시위가 당겨진 상태였다면 화살 발사 시도
        if (currentArrowInteraction != null && bowInteraction != null && bowInteraction.BowHeld) // bowComponent -> bowInteraction 변경
        {
            // Debug.Log("활시위 놓음 감지, 화살 발사 시도!");
            ReleaseArrowFromSocket(); // 화살 발사 로직
        }
    }

    // XRSocketInteractor가 Interactable을 선택 해제할 때의 움직임 타입 오버라이드
    public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
    {
        get { return XRBaseInteractable.MovementType.Instantaneous; } // 즉시 움직임
    }

    // 소켓에서 화살을 분리하고 ArrowInteraction을 통해 발사
    private void ReleaseArrowFromSocket()
    {
        // ArrowInteraction의 ReleaseArrow 메서드를 호출하여 화살을 발사합니다.
        if (currentArrowInteraction != null && stringInteraction != null)
        {
            currentArrowInteraction.ReleaseArrow(stringInteraction.PullAmount); // StringInteraction의 PullAmount 전달
        }
        else
        {
            Debug.LogWarning("SocketInteraction: 화살 또는 StringInteraction 참조가 없어 화살을 발사할 수 없습니다.");
        }
        // ReleaseArrow 호출 후에는 currentArrowInSocket과 currentArrowInteraction이 ClearVariables에서 초기화됩니다.
    }

    // 변수 초기화
    private void ClearVariables()
    {
        currentArrow = null; // 변경된 변수명 적용
        currentArrowInteraction = null;
        handHoldingArrow = null; // 화살을 잡고 있던 손 참조도 초기화
    }
}