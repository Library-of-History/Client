/*
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 
// 필요한 경우 추가:
// using UnityEngine.XR.Interaction.Toolkit.Utilities; // GetOldestInteractableSelected 확장 메서드가 여기에 있을 수 있습니다.

// 이 스크립트가 부착된 GameObject에는 StringInteraction 컴포넌트가 필수적임을 명시합니다.
[RequireComponent(typeof(StringInteraction))]
public class SocketInteraction : XRSocketInteractor
{
    // 화살을 잡고 있는 손 Interactor (IXRInteractor로 변경)
    private XRBaseInteractor handHoldingArrow = null; // 현재 사용되지 않으므로 경고가 뜰 수 있습니다.
    private XRBaseInteractable currentArrow = null; // 현재 소켓에 장전된 화살
    private StringInteraction stringInteraction = null; // 활시위 당김을 관리하는 컴포넌트
    
    // bowInteraction 변수를 선언하고, Inspector에서 할당할 수 있도록 [SerializeField] 추가
    [SerializeField] private BowInteraction bowInteraction = null; // 활의 BowInteraction 컴포넌트

    private ArrowInteraction currentArrowInteraction = null; // 장전된 화살의 ArrowInteraction 컴포넌트
   

    protected override void Awake()
    {
        base.Awake();
        stringInteraction = GetComponent<StringInteraction>(); // StringInteraction은 같은 GameObject에 있을 것으로 예상
        
        // bowInteraction이 Inspector에서 할당되지 않았다면 경고
        if (bowInteraction == null)
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

    // 화살이 소켓 위로 호버링될 때 호출
    protected override void OnHoverEntered(HoverEnterEventArgs args)
{
    base.OnHoverEntered(args);
    
    if (args.interactableObject is XRBaseInteractable xrInteractable)
    {
        Debug.Log($"[SocketInteraction] OnHoverEntered: {xrInteractable.gameObject.name} 호버링됨.");
    }
    else
    {
        Debug.LogWarning($"[SocketInteraction] OnHoverEntered: 호버링된 객체가 XRBaseInteractable이 아닙니다: {args.interactableObject.GetType().Name}");
        return; 
    }

    if (args.interactableObject is XRBaseInteractable hoveredInteractable)
    {
        Debug.Log($"[SocketInteraction] 호버링된 객체 태그: {hoveredInteractable.gameObject.tag}");
        Debug.Log($"[SocketInteraction] 활 컴포넌트 유효: {bowInteraction != null}");
        if (bowInteraction != null)
        {
            Debug.Log($"[SocketInteraction] 활 잡힘 상태 (BowHeld): {bowInteraction.BowHeld}");
        }

        if (hoveredInteractable.gameObject.CompareTag("Arrow") && // 화살 태그 확인
            bowInteraction != null && // 활 컴포넌트 유효성 확인
            bowInteraction.BowHeld) // 활이 잡힌 상태인지 확인
        {
            Debug.Log("[SocketInteraction] 화살 장전 조건 충족 (태그, 활 잡힘). 장전 시도.");
            
            // 소켓이 비어있는지 확인
            if (!hasSelection) 
            {
                // 화살을 호버링하는 Interactor를 가져옵니다.
                IXRSelectInteractor currentInteractor = args.interactorObject as IXRSelectInteractor;
                
                // 현재 Interactor가 화살을 잡고 있는지 확인 (선택 사항)
                // 잡고 있다면 SelectExit을 먼저 시도하여 손에서 화살을 놓게 합니다.
                IXRSelectInteractable selectedByInteractor = currentInteractor?.GetOldestInteractableSelected();

                if (currentInteractor != null && 
                    currentInteractor.hasSelection && 
                    selectedByInteractor == hoveredInteractable) 
                {
                    Debug.Log("[SocketInteraction] Interactor가 화살을 잡고 있음. SelectExit 먼저 호출.");
                    interactionManager.SelectExit(currentInteractor, hoveredInteractable);
                }
                else
                {
                    Debug.Log("[SocketInteraction] Interactor가 화살을 잡고 있지 않거나 다른 것 선택 중. SelectExit 스킵.");
                }
                
                // 항상 SelectEnter를 시도하여 소켓이 화살을 잡도록 합니다.
                // 이전 SelectExit 호출 여부와 상관없이 소켓이 잡는 로직을 진행합니다.
                interactionManager.SelectEnter(this, hoveredInteractable as IXRSelectInteractable);
                Debug.Log("[SocketInteraction] 소켓이 화살을 잡기 위해 SelectEnter 호출.");
            }
            else // 소켓에 이미 다른 화살이 장전되어 있음
            {
                Debug.Log("[SocketInteraction] 소켓에 이미 다른 화살이 장전되어 있음.");
            }
        }
        else // 장전 조건 중 하나라도 미충족
        {
            Debug.Log("[SocketInteraction] 화살 장전 조건 미충족. 조건: " +
                      $"\n  - 화살 태그: {hoveredInteractable.gameObject.CompareTag("Arrow")}" +
                      $"\n  - 활 컴포넌트: {(bowInteraction != null)}" +
                      $"\n  - 활 잡힘: {(bowInteraction != null && bowInteraction.BowHeld)}");
        }
    }
}

    // 화살이 소켓에 장전될 때(=활에 화살이 끼워질 때)
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        StoreArrow(args.interactableObject as XRBaseInteractable);

        if (stringInteraction != null && args.interactorObject is IXRSelectInteractor selectInteractor)
        {
            // 이미 시위를 잡고 있다면 추가로 SelectEnter 하지 않음
            if (!selectInteractor.interactablesSelected.Contains(stringInteraction as IXRSelectInteractable))
            {
                // 손이 화살을 잡고 있다면, 그게 시위가 아니면 먼저 놓게 한다
                var selected = selectInteractor.GetOldestInteractableSelected();
                if (selected != null && selected != stringInteraction as IXRSelectInteractable)
                {
                    Debug.Log("[SocketInteraction] 손이 화살을 잡고 있어 먼저 놓습니다.");
                    interactionManager.SelectExit(selectInteractor, selected);

                    // 한 프레임 뒤에 시위를 잡게 한다
                    StartCoroutine(SelectStringInteractionNextFrame(selectInteractor));
                    return;
                }
                // 그 다음 시위를 잡게 한다
                Debug.Log("[SocketInteraction] 손이 시위를 잡도록 SelectEnter 호출");
                interactionManager.SelectEnter(selectInteractor, stringInteraction as IXRSelectInteractable);
            }
            else
            {
                Debug.Log("[SocketInteraction] 이미 시위를 잡고 있으므로 SelectEnter를 호출하지 않습니다.");
            }
        }
    }

    // 코루틴 추가
    private IEnumerator SelectStringInteractionNextFrame(IXRSelectInteractor selectInteractor)
    {
        yield return null; // 한 프레임 대기
        if (!selectInteractor.interactablesSelected.Contains(stringInteraction as IXRSelectInteractable))
        {
            Debug.Log("[SocketInteraction] (코루틴) 한 프레임 뒤에 손이 시위를 잡도록 SelectEnter 호출");
            interactionManager.SelectEnter(selectInteractor, stringInteraction as IXRSelectInteractable);
        }
    }

    // 소켓에서 화살이 선택 해제되었을 때 (발사 또는 수동 분리)
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // 화살이 소켓에서 분리되면 변수 초기화
        ClearVariables();
        Debug.Log("화살이 소켓에서 분리됨!");
    }

    // 소켓에 들어온 화살의 정보 저장
    private void StoreArrow(XRBaseInteractable interactable)
    {
        if (interactable != null && interactable.CompareTag("Arrow")) // 널 체크 및 태그 확인
        {
            currentArrow = interactable;
            // ArrowInteraction 컴포넌트 가져오기
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
        if (currentArrowInteraction != null && bowInteraction != null && bowInteraction.BowHeld)
        {
            Debug.Log("활시위 놓음 감지, 화살 발사 시도!");
            ReleaseArrowFromSocket(); // 화살 발사 로직
        }
        else
        {
            Debug.Log("[SocketInteraction] 화살 발사 조건 미충족. 조건: " +
                      $"\n  - 화살 장전됨: {(currentArrowInteraction != null)}" +
                      $"\n  - 활 컴포넌트: {(bowInteraction != null)}" +
                      $"\n  - 활 잡힘: {(bowInteraction != null && bowInteraction.BowHeld)}");
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
        // ReleaseArrow 호출 후에는 currentArrow와 currentArrowInteraction이 ClearVariables에서 초기화됩니다.
    }

    // 변수 초기화
    private void ClearVariables()
    {
        currentArrow = null;
        currentArrowInteraction = null;
        handHoldingArrow = null; // 이 변수는 현재 사용되지 않습니다.
    }
}
*/
