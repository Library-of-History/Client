using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BowInteraction : XRGrabInteractable
{
    private LineRenderer bowString;
    private StringInteraction stringInteraction;

    [SerializeField] private Transform socketTransform;
    public bool BowHeld { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        stringInteraction = GetComponentInChildren<StringInteraction>();
        bowString = GetComponentInChildren<LineRenderer>();
        this.movementType = MovementType.Instantaneous;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        BowHeld = true;
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        BowHeld = false;
        base.OnSelectExited(args);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase); // 1. 부모 클래스의 메서드 호출

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic) // 2. 업데이트 단계 확인
        {
            if (stringInteraction != null) // 3. stringInteraction 객체 유효성 확인
            {
                UpdateBow(stringInteraction.PullAmount); // 4. 활 업데이트 메서드 호출
            }
        }
    }

    private void UpdateBow(float pullAmount)
    {
        float xPositionStart = stringInteraction.stringStartPoint.localPosition.x; // 1. 시위 시작점의 X 위치
        float xPositionEnd = stringInteraction.stringEndPoint.localPosition.x; // 2. 시위 끝점의 X 위치

        Vector3 linePosition = Vector3.right * Mathf.Lerp(xPositionStart, xPositionEnd, pullAmount); // 3. 시위 위치 계산

        bowString.SetPosition(1, linePosition); // 4. 활 시위 라인 렌더러 업데이트
        socketTransform.localPosition = linePosition; // 5. 소켓 트랜스폼 위치 업데이트
    }
}