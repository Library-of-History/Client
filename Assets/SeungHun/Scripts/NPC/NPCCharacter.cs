using System.Collections;
using UnityEngine;

public class NPCCharacter : MonoBehaviour
{
    [Header("NPC 정보")] 
    public string npcName;

    [Header("대화 내용")]
    public DialogueData advancedDialogue;

    [Header("NPC UI")]
    public NPCDialogueUI npcDialogueUI;
    
    [Header("대화 관련")] 
    public bool canTalk = true;
    public float talkCooldown = 2f;

    [Header("VR 추적 설정")]
    [Tooltip("(true): 헤드셋 위치 추적, (false): 몸체 위치 추적")]
    public bool trackHeadPosition = true;

    [Tooltip("플레이어를 바라볼 때 Y축만 회전")] 
    public bool yAxisRotationOnly = true;

    [Tooltip("회전 속도")] 
    public float rotationSpeed = 2f;
    
    private NPCTriggerDetection detection;
    private VRPlayerTracker vrPlayerTracker;
    private bool isInCooldown = false;
    private Quaternion originalRotation;
    
    private int currentDialogueIndex = 0;

    private void Start()
    {
        InitializeNPC();
        originalRotation = transform.rotation;

        if (npcDialogueUI == null)
        {
            npcDialogueUI = GetComponentInChildren<NPCDialogueUI>();
        }

        if (npcDialogueUI == null)
        {
            Debug.LogError($"{npcName}: NPCDialogueUI 없음");
        }
    }

    private void InitializeNPC()
    {
        detection = GetComponent<NPCTriggerDetection>();

        if (detection == null)
        {
            Debug.Log($"{npcName}: NPCTriggerDetection 컴포넌트 없음");
            return;
        }

        SetupDetectionEvents();
    }

    private void SetupDetectionEvents()
    {
        detection.OnPlayerEnter.AddListener(OnPlayerApproach);
        detection.OnPlayerExit.AddListener(OnPlayerLeave);
        detection.OnPlayerStay.AddListener(OnPlayerStayNearby);
    }

    public void OnPlayerApproach(Transform player)
    {
        Debug.Log($"{npcName}: {player.name} 다가옴");
        
        vrPlayerTracker = player.GetComponent<VRPlayerTracker>();
        if (vrPlayerTracker == null)
        {
            Debug.LogWarning($"{npcName}: VRPlayerTracker 없음");
        }
        
        StartCoroutine(LookAtPlayer());
        
        canTalk = true;
        
        UIManager.Instance?.ShowInteractionUI(this);
        
        if (VRInteractionManager.Instance != null)
        {
            VRInteractionManager.Instance.SetCurrentNPC(this);
        }
    }

    public void OnPlayerLeave(Transform player)
    {
        Debug.Log($"{npcName}: {player.name} 멀어짐");

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActiveWithNPC(this))
        {
            Debug.Log($"{npcName}: 대화 중단 - 플레이어가 범위 벗어남");
            DialogueManager.Instance.InterruptDialogue();
        }
        
        vrPlayerTracker = null;
        StartCoroutine(ReturnToIdleState());
        
        canTalk = false;
        
        UIManager.Instance?.HideInteractionUI();

        if (VRInteractionManager.Instance != null)
        {
            VRInteractionManager.Instance.ClearCurrentNPC();
        }

        ResetDialogueState();
    }

    public void OnPlayerStayNearby()
    {
        if (detection.CurrentPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, detection.CurrentPlayer.position);
            if (distance < 1f)
            {
                Vector3 direction = (transform.position - detection.CurrentPlayer.position).normalized;
                transform.position += direction * Time.deltaTime * 0.5f;
            }
        }
    }

    private void ResetDialogueState()
    {
        currentDialogueIndex = 0;
        isInCooldown = false;
        
        Debug.Log($"{npcName}: 대화 상태 초기화 완료");
    }
    
    public void StartDialogue()
    {
        if (!canTalk || isInCooldown)
        {
            Debug.Log($"{npcName}: 지금은 대화할 수 없음");
            return;
        }
        
        if (npcDialogueUI == null)
            return;

        if (advancedDialogue != null && advancedDialogue.dialogueNodes != null && advancedDialogue.dialogueNodes.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(advancedDialogue, npcDialogueUI);
        }
        else
        {
            Debug.LogWarning($"{npcName}: 대화 데이터가 설정되지 않음");
        }
        
        StartCoroutine(DialogueCooldown());
    }

    private IEnumerator DialogueCooldown()
    {
        isInCooldown = true;
        yield return new WaitForSeconds(talkCooldown);
        isInCooldown = false;
    }
    
    private IEnumerator LookAtPlayer()
    {
        while (detection.PlayerInRange && detection.CurrentPlayer != null)
        {
            Vector3 targetPosition;

            if (vrPlayerTracker != null)
            {
                targetPosition = trackHeadPosition ?
                    vrPlayerTracker.GetLookAtPosition() :
                    vrPlayerTracker.GetBodyPosition();
            }
            else
            {
                targetPosition = detection.CurrentPlayer.position;
            }

            Vector3 directionToPlayer = (targetPosition - transform.position);

            if (yAxisRotationOnly)
            {
                directionToPlayer.y = 0;
            }

            if (directionToPlayer.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);   
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            
            yield return null;
        }
    }

    private IEnumerator ReturnToIdleState()
    {
        while (Quaternion.Angle(transform.rotation, originalRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 1f);
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}
