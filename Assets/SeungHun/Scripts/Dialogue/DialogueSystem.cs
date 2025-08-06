using UnityEngine;

public enum ChoiceActionType
{
    Continue,
    ReturnTo,
    EndDialogue
}

[System.Serializable]
public class ResponseDialogue
{
    [TextArea(2, 5)] 
    public string text;

    [Header("음성 설정")] 
    public AudioClip voiceClip;

    [Range(0f, 1f)] 
    public float voiceVolume = 1f;

    public VoiceTimingMode voiceTimingMode = VoiceTimingMode.WithTyping;
    
    public ResponseDialogue() {}

    public ResponseDialogue(string responseText, AudioClip voice = null, float volume = 1f)
    {
        text = responseText;
        voiceClip = voice;
        voiceVolume = volume;
        voiceTimingMode = VoiceTimingMode.WithTyping;
    }
}

[System.Serializable]
public class DialogueChoice
{ 
    [TextArea(1, 3)] 
    public string choiceText;
    
    [Header("응답 대화")]
    public ResponseDialogue[] responseDialogues;
    
    [Header("선택지")]
    [Tooltip("이 선택지를 선택했을 때 행동")]
    public ChoiceActionType actionType = ChoiceActionType.Continue;

    [Tooltip("ReturnTo 타입일 때 돌아갈 노드 인덱스")] 
    public int returnToNodeIndex = 0;
    
    public DialogueChoice() {}

    public DialogueChoice(string choice, ChoiceActionType action = ChoiceActionType.Continue, int returnIndex = 0, params ResponseDialogue[] responses)
    {
        choiceText = choice;
        responseDialogues = responses;
        actionType = action;
        returnToNodeIndex = returnIndex;
    }

    public DialogueChoice(string choice, ChoiceActionType action = ChoiceActionType.Continue, int returnIndex = 0, params string[] responses)
    {
        choiceText = choice;
        actionType = action;
        returnToNodeIndex = returnIndex;
        
        if (responses != null && responses.Length > 0)
        {
            responseDialogues = new ResponseDialogue[responses.Length];
            for (int i = 0; i < responses.Length; i++)
            {
                responseDialogues[i] = new ResponseDialogue(responses[i]);
            }
        }
    }
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 5)] 
    public string dialogue;

    [Header("음성 설정")]
    [Tooltip("이 대화에 재생할 음성 파일")]
    public AudioClip voiceClip;
    
    [Tooltip("음성 볼륨 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
    
    [Header("재생 타이밍")]
    [Tooltip("음성과 타이핑 동기화")]
    public VoiceTimingMode voiceTimingMode = VoiceTimingMode.WithTyping;
    
    [Header("선택지 설정")] 
    public bool hasChoices = false;
    
    [Header("선택지들")]
    public DialogueChoice[] choices;

    [Header("노드 정보")] 
    [Tooltip("이 노드의 고유 ID(ReturnTo에서 사용용")]
    public string nodeId = "";
    
    public DialogueNode() {}

    public DialogueNode(string text, string id = "")
    {
        dialogue = text;
        hasChoices = false;
        nodeId = id;
    }
    
    public DialogueNode(string text, AudioClip voice, string id = "", params DialogueChoice[] nodeChoices)
    {
        dialogue = text;
        voiceClip = voice;
        voiceVolume = 1f;
        voiceTimingMode = VoiceTimingMode.WithTyping;
        hasChoices = nodeChoices != null && nodeChoices.Length > 0;
        choices = nodeChoices;
        nodeId = id;
    }
}

public enum VoiceTimingMode
{
    WithTyping,
    BeforeTyping,
    AfterTyping
}

[System.Serializable]
public class DialogueData
{
    public string npcName;
    public DialogueNode[] dialogueNodes;
    
    public DialogueData() {}

    public DialogueData(string name, string[] sentences)
    {
        npcName = name;
        dialogueNodes = new DialogueNode[sentences.Length];
        for (int i = 0; i < sentences.Length; i++)
        {
            dialogueNodes[i] = new DialogueNode(sentences[i]);
        }
    }
}
