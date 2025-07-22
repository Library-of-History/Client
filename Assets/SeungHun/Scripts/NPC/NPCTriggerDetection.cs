using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NPCTriggerDetection : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private bool detectOnlyInFront = false;
    [SerializeField] private float frontAngle = 120f;
    [SerializeField] private LayerMask playerLayerMask = -1;
    
    [Header("비주얼")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Transform promptPosition;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;
    
    [System.Serializable]
    public class PlayerProximityEvent : UnityEvent<Transform> { }
    
    [Header("이벤트")]
    public PlayerProximityEvent OnPlayerEnter;
    public PlayerProximityEvent OnPlayerExit;
    public UnityEvent OnPlayerStay; // 플레이어 범위 내에 있을 때 지속 호출
    
    private bool playerInRange = false;
    private Transform currentPlayer;
    private SphereCollider detectionCollider;
    private Coroutine stayCoroutine;
    
    public bool PlayerInRange => playerInRange;
    public Transform CurrentPlayer => currentPlayer;
    public float DetectionRadius => detectionRadius;

    public LayerMask PlayerLayerMask => playerLayerMask;

    private void Awake()
    {
        SetupDetectionCollider();
        InitializeComponents();
    }

    private void Start()
    {
        ValidateSetup();   
    }

    private void SetupDetectionCollider()
    {
        GameObject detectionZone = new GameObject("DetectionZone");
        detectionZone.transform.SetParent(transform);
        detectionZone.transform.localPosition = Vector3.zero;
        detectionZone.transform.localRotation = Quaternion.identity;
        detectionZone.transform.localScale = Vector3.one;
        
        detectionCollider = detectionZone.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
        
        NPCDetectionZone detectionZoneScript = detectionZone.AddComponent<NPCDetectionZone>();
        detectionZoneScript.Initialize(this);
    }

    private void InitializeComponents()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);

            if (promptPosition != null)
            {
                interactionPrompt.transform.position = promptPosition.position;
                interactionPrompt.transform.SetParent(promptPosition);
            }
        }
    }

    private void ValidateSetup()
    {
        if (showDebugInfo)
        {
            if (interactionPrompt == null)
                Debug.LogWarning($"{gameObject.name}: Interaction Prompt 설정 안됨");
            
            if (GameObject.FindGameObjectWithTag("Player") == null)
                Debug.LogWarning($"Player 없음");
        }
    }
    
    public void HandlePlayerEnter(Transform player)
    {
        if (playerInRange && currentPlayer == player)
            return;

        if (detectOnlyInFront && !IsPlayerInFront(player))
            return;

        playerInRange = true;
        currentPlayer = player;

        StartProximityFeedback();
        
        OnPlayerEnter?.Invoke(player);

        if (stayCoroutine != null)
            StopCoroutine(stayCoroutine);
        stayCoroutine = StartCoroutine(PlayerStayRoutine());

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 플레이어 감지 거리 내에 들어옴 : {player.name}");
        }
    }


    public void HandlePlayerExit(Transform player)
    {
        if (!playerInRange || currentPlayer != player) 
            return;
        
        playerInRange = false;
        currentPlayer = null;

        StopProximityFeedback();

        if (stayCoroutine != null)
        {
            StopCoroutine(stayCoroutine);
            stayCoroutine = null;
        }
        
        OnPlayerExit?.Invoke(player);

        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 플레이어 감지 거리 벗어남 : {player.name}");
        }
    }

    private bool IsPlayerInFront(Transform player)
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle <= frontAngle * 0.5f;
    }

    private IEnumerator PlayerStayRoutine()
    {
        while (playerInRange)
        {
            OnPlayerStay?.Invoke();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void StartProximityFeedback()
    {
        StartCoroutine(ProximityFeedbackSequence());
    }

    private void StopProximityFeedback()
    {
        StartCoroutine(ExitFeedbackSequence());
    }


    private IEnumerator ProximityFeedbackSequence()
    {
        yield return new WaitForSeconds(0.2f);

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            
            CanvasGroup canvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, 0.3f));
            }
        }
    }

    private IEnumerator ExitFeedbackSequence()
    {
        if (interactionPrompt != null && interactionPrompt.activeInHierarchy)
        {
            CanvasGroup canvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, 0.2f));
            }
            
            interactionPrompt.SetActive(false);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elaspedTime = 0;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            elaspedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }

}
