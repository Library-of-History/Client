using UnityEngine;

public class VRInteractionManager : MonoBehaviour
{
   public static VRInteractionManager Instance;

   private NPCCharacter currentInteractableNPC;
   private float lastDialogueEndTime = 0f;
   private float interactionCooldown = 0.5f;
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
      if (VRInputManager.Instance != null)
      {
         VRInputManager.Instance.OnPrimaryButtonPressed += HandleVRInteraction;
      }

      DialogueManager.OnDialogueEnd += OnDialogueEnded;
   }

   private void OnDialogueEnded()
   {
      lastDialogueEndTime = Time.time;
      Debug.Log("VR Interaction Manager: 대화 종료, 쿨다운 시작");
   }
   private void HandleVRInteraction()
   {
      if (Time.time - lastDialogueEndTime < interactionCooldown)
      {
         Debug.Log("VR Interaction Manager: 쿨다운 중, 상호작용 무시");
         return;
      }
      
      if (DialogueManager.Instance != null && !DialogueManager.Instance.dialogueActive)
      {
         if (currentInteractableNPC != null && currentInteractableNPC.canTalk)
         {
            currentInteractableNPC.StartDialogue();
         }
      }
   }

   public void SetCurrentNPC(NPCCharacter npc)
   {
      currentInteractableNPC = npc;
      Debug.Log($"VR 상호작용 NPC 설정: {npc.npcName}");
   }

   public void ClearCurrentNPC()
   {
      if (currentInteractableNPC != null)
      {
         Debug.Log($"VR 상호작용 NPC 해제: {currentInteractableNPC.npcName}");
      }
      currentInteractableNPC = null;
   }

   private void OnDestroy()
   {
      if (VRInputManager.Instance != null)
      {
         VRInputManager.Instance.OnPrimaryButtonPressed -= HandleVRInteraction;
      }
      
      DialogueManager.OnDialogueEnd -= OnDialogueEnded;
   }
}
