using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("현재 활성화된 NPC UI")] 
    [SerializeField] private NPCDialogueUI currentNPCUI;
    
    [Header("대화 설정")]
    public float typingSpeed = 0.05f;
    private float lastTypingStartTime = 0f;
    private GameObject DialoguePanel => currentNPCUI?.dialoguePanel;
    private TextMeshProUGUI DialogueText => currentNPCUI?.dialogueText;
    private Button NextButton => currentNPCUI?.nextButton;
    private GameObject ChoicesPanel => currentNPCUI?.choicesPanel;
    private Button[] ChoiceButtons => currentNPCUI?.choiceButtons;
    private TextMeshProUGUI[] ChoiceButtonTexts => currentNPCUI?.choiceButtonTexts;
    private Color NormalChoiceColor => currentNPCUI?.normalChoiceColor ?? Color.white;
    private Color SelectedChoiceColor => currentNPCUI?.selectedChoiceColor ?? Color.yellow;
    
    
    private DialogueData currentDialogueData;
    private int currentNodeIndex = 0;
    private bool isTyping = false;
    public bool dialogueActive = false;
    private bool waitingForChoice = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;

    private DialogueChoice[] currentChoices;
    private Queue<ResponseDialogue> choiceResponseQueue;
    private int pendingReturnNodeIndex = -1;

    private int selectedChoiceIndex = 0;

    public static System.Action OnDialogueStart;
    public static System.Action OnDialogueEnd;
    
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
        choiceResponseQueue = new Queue<ResponseDialogue>();
        InitializeUI();
        SetupVRInput();
    }

    private void InitializeUI()
    {
        if (currentNPCUI != null)
        {
            currentNPCUI.SetDialoguePanelActive(false);
            currentNPCUI.SetChoicesPanelActive(false);

            if (NextButton != null)
            {
                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(DisplayNextSentence);
            }
        }
        SetupChoiceButtons();
    }

    private void SetupVRInput()
    {
        if (VRInputManager.Instance != null)
        {
            VRInputManager.Instance.OnPrimaryButtonPressed += OnVRPrimaryButtonPressed;
            VRInputManager.Instance.OnJoystickDirectionChange += OnVRJoystickDirection;
        }
    }
    
    private void SetupChoiceButtons()
    {
        if (ChoiceButtons == null)
            return;

        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (ChoiceButtons[i] != null)
            {
                int choiceIndex = i;
                ChoiceButtons[i].onClick.RemoveAllListeners();
                ChoiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
        }

        if (currentNPCUI != null)
            currentNPCUI.AutoFindChoiceTexts();
    }

    private void SetCurrentNPCUI(NPCDialogueUI npcUI)
    {
        currentNPCUI = npcUI;
        InitializeUI();
        Debug.Log($"DialogueManager: 현재 UI 설정됨 - {npcUI.name}");
    }

    private void OnVRPrimaryButtonPressed()
    {
        if (!dialogueActive)
            return;

        if (waitingForChoice)
        {
            OnChoiceSelected(selectedChoiceIndex);
        }
        else
        {
            if (isTyping && Time.time - lastTypingStartTime < 0.5f)
            {
                Debug.Log("VR: 타이핑 시작 직후 입력 무시됨");
                return;
            }
            
            DisplayNextSentence();
        }
    }

    private void OnVRJoystickDirection(int direction)
    {
        if (!waitingForChoice || currentChoices == null)
            return;
        
        selectedChoiceIndex += direction;
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);

        UpdateChoiceHighlight();
        
        Debug.Log($"VR: 선택지 {selectedChoiceIndex} 선택됨");
    }

    public void StartDialogue(DialogueData data, NPCDialogueUI npcUI)
    {
        Debug.Log($"{data.npcName}와 대화 시작");

        SetCurrentNPCUI(npcUI);
        
        currentDialogueData = data;
        currentNodeIndex = 0;
        dialogueActive = true;
        waitingForChoice = false;
        pendingReturnNodeIndex = -1;
        
        currentNPCUI.SetDialoguePanelActive(true);
        choiceResponseQueue.Clear();
        
        OnDialogueStart?.Invoke();

        DisplayCurrentNode();
    }

    private void DisplayCurrentNode()
    {
        if (currentDialogueData == null || currentNodeIndex >= currentDialogueData.dialogueNodes.Length)
        {
            EndDialogue();
            return;
        }

        DialogueNode currentNode = currentDialogueData.dialogueNodes[currentNodeIndex];
        
        Debug.Log($"노드 {currentNodeIndex} 시작 - '{currentNode.dialogue}'");
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        typingCoroutine = StartCoroutine(TypeSentence(currentNode.dialogue, currentNode));
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            CompleteCurrentSentence();
            return;
        }

        if (waitingForChoice)
            return;

        if (choiceResponseQueue.Count > 0)
        {
            ResponseDialogue response = choiceResponseQueue.Dequeue();

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null; 
            }
                
            typingCoroutine = StartCoroutine(TypeResponseWithVoice(response));
            return;
        }
        
        if (pendingReturnNodeIndex == -2)
        {
            EndDialogue();
            return;
        }
        
        if (pendingReturnNodeIndex >= 0)
        {
            currentNodeIndex = pendingReturnNodeIndex;
            pendingReturnNodeIndex = -1;
            Debug.Log($"노드 {currentNodeIndex}로 돌아갑니다.");
            DisplayCurrentNode();
            return;
        }
        
        currentNodeIndex++;
        DisplayCurrentNode();
    }

    private IEnumerator TypeSentence(string sentence, DialogueNode node = null)
    {
        lastTypingStartTime = Time.time;
        isTyping = true;
        
        if (DialogueText != null)
            DialogueText.text = "";

        yield return null;
        
        currentSentence = sentence;
        
        if (node != null && node.voiceClip != null && currentNPCUI != null)
        {
            switch (node.voiceTimingMode)
            {
                case VoiceTimingMode.BeforeTyping:
                    yield return StartCoroutine(PlayVoiceBeforeTyping(node, sentence));
                    break;
                
                case VoiceTimingMode.WithTyping:
                default:
                    yield return StartCoroutine(PlayVoiceWithTyping(node, sentence));
                    break;  
                
                case VoiceTimingMode.AfterTyping:
                    yield return StartCoroutine(PlayVoiceAfterTyping(node, sentence));
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(TypeText(sentence));
        }

        isTyping = false;

        if (node != null && node.hasChoices && node.choices != null && node.choices.Length > 0)
        {
            ShowChoices(node.choices);
        }
    }

    private IEnumerator PlayVoiceBeforeTyping(DialogueNode node, string sentence)
    {
        currentNPCUI.PlayVoice(node.voiceClip, node.voiceVolume);
        
        yield return new WaitForSeconds(node.voiceClip.length);
        
        yield return StartCoroutine(TypeText(sentence));
    }

    private IEnumerator PlayVoiceWithTyping(DialogueNode node, string sentence)
    {
        currentNPCUI.PlayVoice(node.voiceClip, node.voiceVolume);
        
        float voiceLength = node.voiceClip.length;
        
        float adjustedTypingSpeed;
        if (sentence.Length > 0)
        {
            adjustedTypingSpeed = voiceLength / sentence.Length;
            adjustedTypingSpeed = Mathf.Clamp(adjustedTypingSpeed, 0.01f, 0.2f);
        }
        else
        {
            adjustedTypingSpeed = typingSpeed;
        }
        
        yield return StartCoroutine(TypeTextWithSpeed(sentence, adjustedTypingSpeed));
    }

    private IEnumerator PlayVoiceAfterTyping(DialogueNode node, string sentence)
    {
        yield return StartCoroutine(TypeText(sentence));
        
        currentNPCUI.PlayVoice(node.voiceClip, node.voiceVolume);
        
        yield return new WaitForSeconds(node.voiceClip.length);
    }
    
    private IEnumerator TypeText(string sentence)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            if (DialogueText != null)
                DialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    private IEnumerator TypeTextWithSpeed(string sentence, float customSpeed)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            if (DialogueText != null)
                DialogueText.text += letter;
            yield return new WaitForSecondsRealtime(customSpeed);
        }
    }

    private IEnumerator TypeResponseWithVoice(ResponseDialogue response)
    {
        isTyping = true;
        
        currentSentence = response.text;
        
        if (DialogueText != null)
            DialogueText.text = "";

        if (response.voiceClip != null && currentNPCUI != null)
        {
            switch (response.voiceTimingMode)
            {case VoiceTimingMode.BeforeTyping:
                    yield return StartCoroutine(PlayResponseVoiceBeforeTyping(response));
                    break;
                
                case VoiceTimingMode.WithTyping:
                default:
                    yield return StartCoroutine(PlayResponseVoiceWithTyping(response));
                    break;
                
                case VoiceTimingMode.AfterTyping:
                    yield return StartCoroutine(PlayResponseVoiceAfterTyping(response));
                    break;
            }            
        }
        else
        {
            yield return StartCoroutine(TypeText(response.text));
        }
        
        isTyping = false;
    }
    
    private IEnumerator PlayResponseVoiceBeforeTyping(ResponseDialogue response)
    {
        currentNPCUI.PlayVoice(response.voiceClip, response.voiceVolume);
        yield return new WaitForSeconds(response.voiceClip.length);
        yield return StartCoroutine(TypeText(response.text));
    }

    private IEnumerator PlayResponseVoiceWithTyping(ResponseDialogue response)
    {
        currentNPCUI.PlayVoice(response.voiceClip, response.voiceVolume);
    
        float voiceLength = response.voiceClip.length;
        
        float adjustedTypingSpeed;
        if (response.text.Length > 0)
        {
            adjustedTypingSpeed = voiceLength / response.text.Length;
            adjustedTypingSpeed = Mathf.Clamp(adjustedTypingSpeed, 0.01f, 0.2f);
        }
        else
        {
            adjustedTypingSpeed = typingSpeed;
        }
        
        yield return StartCoroutine(TypeTextWithSpeed(response.text, adjustedTypingSpeed));
    }

    private IEnumerator PlayResponseVoiceAfterTyping(ResponseDialogue response)
    {
        yield return StartCoroutine(TypeText(response.text));
        currentNPCUI.PlayVoice(response.voiceClip, response.voiceVolume);
        yield return new WaitForSeconds(response.voiceClip.length);
    }
    
    private void ShowChoices(DialogueChoice[] choices)
    {
        currentChoices = choices;
        waitingForChoice = true;
        selectedChoiceIndex = 0;

        if (NextButton != null)
            NextButton.gameObject.SetActive(false);
        
        currentNPCUI.SetChoicesPanelActive(true);

        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                ChoiceButtons[i].gameObject.SetActive(true);
                if (ChoiceButtonTexts[i] != null)
                {
                    ChoiceButtonTexts[i].text = choices[i].choiceText;
                }
            }
            else
            {
                ChoiceButtons[i].gameObject.SetActive(false);
            }
        }

        UpdateChoiceHighlight();
        Debug.Log($"선택지 표시됨 : {choices.Length}개");
    }

    private void UpdateChoiceHighlight()
    {
        if (ChoiceButtons == null) 
            return;
        
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (i < currentChoices.Length && ChoiceButtons[i] != null)
            {
                if (ChoiceButtonTexts[i] != null)
                {
                    ChoiceButtonTexts[i].color = (i == selectedChoiceIndex) ? SelectedChoiceColor : NormalChoiceColor;
                }
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (!waitingForChoice || currentChoices == null || choiceIndex >= currentChoices.Length)
            return;
        
        DialogueChoice selectedChoice = currentChoices[choiceIndex];
        Debug.Log($"선택지 선택됨 : {currentChoices[choiceIndex].choiceText}, 액션: {selectedChoice.actionType}");

        if (selectedChoice.responseDialogues != null && selectedChoice.responseDialogues.Length > 0)
        {
            foreach (ResponseDialogue response in selectedChoice.responseDialogues)
            {
                if (response != null && !string.IsNullOrEmpty(response.text))
                {
                    choiceResponseQueue.Enqueue(response);
                }
            }
        }
        
        switch (selectedChoice.actionType)
        {
            // 특정 노드로 돌아가기
            case ChoiceActionType.ReturnTo:
                pendingReturnNodeIndex = selectedChoice.returnToNodeIndex;
                Debug.Log($"응답 출력 후 노드 {selectedChoice.returnToNodeIndex}로 돌아갈 예정");
                break;
            
            // 응답 후 대화 종료
            case ChoiceActionType.EndDialogue:
                pendingReturnNodeIndex = -2;
                Debug.Log("응답 출력 후 대화 종료 예정");
                break;
            
            // 다음 대화 진행
            case ChoiceActionType.Continue:
            default:
                pendingReturnNodeIndex = -1;
                break;
        }
        
        HideChoices();
        DisplayNextSentence();
    }

    private void HideChoices()
    {
        waitingForChoice = false;
        currentChoices = null;
        selectedChoiceIndex = 0;

        currentNPCUI.SetChoicesPanelActive(false);
        
        if (NextButton != null)
            NextButton.gameObject.SetActive(true);
    }
    
    private void CompleteCurrentSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        isTyping = false;

        if (DialogueText != null && !string.IsNullOrEmpty(currentSentence))
        {
            Debug.Log($"'{currentSentence}' 설정됨");
            DialogueText.text = currentSentence;
        }
        else
        {
            Debug.Log("currentSentence가 비어있거나 null");
        }
        
        if (DialogueText != null && !string.IsNullOrEmpty(currentSentence))
        {
            DialogueText.text = currentSentence;
        }

        if (currentNPCUI != null)
        {
            currentNPCUI.StopVoice();
        }
    }

    public void EndDialogue()
    {
        Debug.Log("대화 종료");

        dialogueActive = false;
        waitingForChoice = false;

        if (currentNPCUI != null)
        {
            currentNPCUI.StopVoice();
            currentNPCUI.SetDialoguePanelActive(false);
        }
        
        HideChoices();
        
        Time.timeScale = 1f;

        currentDialogueData = null;
        currentNodeIndex = 0;
        pendingReturnNodeIndex = -1;
        selectedChoiceIndex = 0;
        choiceResponseQueue.Clear();
        currentChoices = null;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentNPCUI = null;
        
        OnDialogueEnd?.Invoke();
    }

    // 대화 강제 중단
    public void InterruptDialogue()
    {
        if (!dialogueActive)
            return;
        
        Debug.Log("대화 중단 - 플레이어가 범위를 벗어남");

        if (currentNPCUI != null)
        {
            currentNPCUI.StopVoice();
        }
        
        dialogueActive = false;
        waitingForChoice = false;

        if (currentNPCUI != null)
        {
            currentNPCUI.SetDialoguePanelActive(false);
            currentNPCUI.SetChoicesPanelActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        currentDialogueData = null;
        currentNodeIndex = 0;
        pendingReturnNodeIndex = -1;
        selectedChoiceIndex = 0;
        choiceResponseQueue.Clear();
        currentChoices = null;
        isTyping = false;
        currentSentence = "";
        
        currentNPCUI = null;
        
        OnDialogueEnd?.Invoke();
    }

    public bool IsDialogueActiveWithNPC(NPCCharacter npc)
    {
        return dialogueActive && currentNPCUI != null && currentNPCUI == npc.npcDialogueUI;
    }

    public NPCCharacter GetCurrentDialogueNPC()
    {
        if (!dialogueActive || currentNPCUI == null)
            return null;

        NPCCharacter[] allNPCS = FindObjectsOfType<NPCCharacter>();
        foreach (var npc in allNPCS)
        {
            if (npc.npcDialogueUI == currentNPCUI)
                return npc;
        }

        return null;
    }
    

    private void OnDestroy()
    {
        if (VRInputManager.Instance != null)
        {
            VRInputManager.Instance.OnPrimaryButtonPressed -= OnVRPrimaryButtonPressed;
            VRInputManager.Instance.OnJoystickDirectionChange -= OnVRJoystickDirection;
        }
    }
}
