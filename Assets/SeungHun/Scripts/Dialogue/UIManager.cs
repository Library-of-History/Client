using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("현재 활성화된 NPC UI")] 
    [SerializeField] private NPCDialogueUI currentNPCUI;
    
    [Header("상호작용 UI")] 
    private GameObject InteractionPanel => currentNPCUI?.interactionPanel;
    public TextMeshProUGUI InteractionText => currentNPCUI?.interactionText;
    
    private NPCCharacter currentNPC;
    private bool dialogueInProgress = false;
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
        DialogueManager.OnDialogueStart += OnDialogueStarted;
        DialogueManager.OnDialogueEnd += OnDialogueEnded;
    }

    private void OnDialogueStarted()
    {
        dialogueInProgress = true;

        if (InteractionPanel != null && InteractionPanel.activeInHierarchy)
        {
            currentNPCUI.SetInteractionPanelActive(false);
        }
        Debug.Log("UI: 대화 시작, interaction panel 숨김");
    }

    private void OnDialogueEnded()
    {
        dialogueInProgress = false;

        if (currentNPC != null)
        {
            if (IsPlayerInNPCRange(currentNPC))
            {
                ShowInteractionUI(currentNPC);
            }
            else
            {
                Debug.Log("UI: 플레이어가 범위 밖에 있음");
            }
        }
        
        Debug.Log("UI: 대화 종료, interaction panel 복원");
    }
    
    public void ShowInteractionUI(NPCCharacter npc)
    {
        currentNPC = npc;

        if (npc.npcDialogueUI != null)
        {
            currentNPCUI = npc.npcDialogueUI;
        }
        else
        {
            return;
        }

        if (dialogueInProgress)
            return;

        if (InteractionPanel != null)
        {
            currentNPCUI.SetInteractionPanelActive(true);

            if (InteractionText != null)
            {
                currentNPCUI.SetInteractionText($"{npc.npcName}과 대화하기");
            }
        }
        Debug.Log($"UIManager: {npc.npcName}의 interaction panel 표시");
    }

    public void HideInteractionUI()
    {
        if (currentNPCUI != null && InteractionPanel != null)
        {
            currentNPCUI.SetInteractionPanelActive(false);
            Debug.Log($"UIManager: {currentNPC?.npcName}의 interaction Panel 숨김");
        }
        
        currentNPC = null;
        currentNPCUI = null;
    }

    private bool IsPlayerInNPCRange(NPCCharacter npc)
    {
        if (npc == null)
            return false;

        var detection = npc.GetComponent<NPCTriggerDetection>();
        return detection != null && detection.PlayerInRange;
    }
    
    private void OnDestroy()
    {
        DialogueManager.OnDialogueStart -= OnDialogueStarted;
        DialogueManager.OnDialogueEnd -= OnDialogueEnded;
    }

}

