using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SnapTurnController : MonoBehaviour
{
    [FormerlySerializedAs("snapTurnInputAction")]
    [Header("Input Action 설정")]
    [SerializeField] private InputActionReference SnapTurnInputAction;

    private bool inputActionWasEnabled = true;

    private void Start()
    {
        if (SnapTurnInputAction == null)
        {
            return;
        }

        DialogueManager.OnDialogueStart += OnDialogueStarted;
        DialogueManager.OnDialogueEnd += OnDialogueEnded;
    }

    private void OnDialogueStarted()
    {
        if (SnapTurnInputAction?.action != null)
        {
            inputActionWasEnabled = SnapTurnInputAction.action.enabled;
            SnapTurnInputAction.action.Disable();
            Debug.Log("SnapTurn 비활성화");
        }
    }

    private void OnDialogueEnded()
    {
        if (SnapTurnInputAction?.action != null && inputActionWasEnabled)
        {
            SnapTurnInputAction.action.Enable();
            Debug.Log("SnapTurn 활성화");
        }
    }

    private void OnDestroy()
    {
        DialogueManager.OnDialogueStart -= OnDialogueStarted;
        DialogueManager.OnDialogueEnd -= OnDialogueEnded;
    }
}
