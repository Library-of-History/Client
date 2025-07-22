using UnityEngine;

public enum ChoiceActionType
{
    Continue,
    ReturnTo,
    EndDialogue
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)] 
    public string choiceText;
    
    [TextArea(2, 5)]
    public string[] responseDialogues;
    
    [Header("선택지")]
    [Tooltip("이 선택지를 선택했을 때 행동")]
    public ChoiceActionType actionType = ChoiceActionType.Continue;

    [Tooltip("ReturnTo 타입일 때 돌아갈 노드 인덱스")] 
    public int returnToNodeIndex = 0;
    
    public DialogueChoice() {}

    public DialogueChoice(string choice, ChoiceActionType action = ChoiceActionType.Continue, int returnIndex = 0, params string[] responses)
    {
        choiceText = choice;
        responseDialogues = responses;
        actionType = action;
        returnToNodeIndex = returnIndex;
    }
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 5)] 
    public string dialogue;

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
    
    public DialogueNode(string text, string id = "", params DialogueChoice[] nodeChoices)
    {
        dialogue = text;
        hasChoices = true;
        choices = nodeChoices;
        nodeId = id;
    }
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
