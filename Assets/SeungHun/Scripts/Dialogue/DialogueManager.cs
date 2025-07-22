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
    private Queue<string> choiceResponseQueue;
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
        choiceResponseQueue = new Queue<string>();
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
        currentSentence = currentNode.dialogue;
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
            string response = choiceResponseQueue.Dequeue();
            currentSentence = response;
            typingCoroutine = StartCoroutine(TypeSentence(response));
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
        isTyping = true;
        if (DialogueText != null)
            DialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (DialogueText != null)
              DialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;

        if (node != null && node.hasChoices && node.choices != null && node.choices.Length > 0)
        {
            ShowChoices(node.choices);
        }
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
                Image buttonImage = ChoiceButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = (i == selectedChoiceIndex) ? SelectedChoiceColor : NormalChoiceColor;
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
            foreach (string response in selectedChoice.responseDialogues)
            {
                if (!string.IsNullOrEmpty(response))
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
        }
        
        isTyping = false;
        
        if (DialogueText != null)
         DialogueText.text = currentSentence;
    }

    public void EndDialogue()
    {
        Debug.Log("대화 종료");

        dialogueActive = false;
        waitingForChoice = false;
        
        currentNPCUI.SetDialoguePanelActive(false);
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

    private void OnDestroy()
    {
        if (VRInputManager.Instance != null)
        {
            VRInputManager.Instance.OnPrimaryButtonPressed -= OnVRPrimaryButtonPressed;
            VRInputManager.Instance.OnJoystickDirectionChange -= OnVRJoystickDirection;
        }
    }
}
