using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class NPCDialogueUI : MonoBehaviour
{
    [Header("대화 UI 요소들")] 
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("선택지 UI")] 
    public GameObject choicesPanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceButtonTexts;

    [Header("상호작용 UI")] 
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;
    
    [Header("음성 재생")]
    public AudioSource voiceAudioSource;
    
    [Header("VR 설정")] 
    public Color normalChoiceColor = Color.white;
    public Color selectedChoiceColor = Color.yellow;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }

        SetupVoiceAudioSource();
    }

    private void SetupVoiceAudioSource()
    {
        if (voiceAudioSource == null)
        {
            voiceAudioSource = GetComponent<AudioSource>();
        }

        if (voiceAudioSource == null)
        {
            voiceAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.loop = false;
        voiceAudioSource.spatialBlend = 1f;
    }

    public void SetDialoguePanelActive(bool active)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(active);
    }

    public void SetChoicesPanelActive(bool active)
    {
        if (choicesPanel != null)
            choicesPanel.SetActive(active);
    }

    public void SetInteractionPanelActive(bool active)
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(active);
    }

    public void SetInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text;
        }
    }
    
    public void AutoFindChoiceTexts()
    {
        if (choiceButtonTexts == null || choiceButtonTexts.Length == 0)
        {
            choiceButtonTexts = new TextMeshProUGUI[choiceButtons.Length];
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtonTexts[i] = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();   
                }
            }
        }
    }

    public void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (voiceAudioSource != null && clip != null)
        {
            voiceAudioSource.clip = clip;
            voiceAudioSource.volume = volume;
            voiceAudioSource.Play();

            Debug.Log($"음성 재생 시작 - {clip.name}");
        }
    }

    public void StopVoice()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
            Debug.Log("음성 재생 중단");
        }
    }

    public bool IsVoicePlaying()
    {
        return voiceAudioSource != null && voiceAudioSource.isPlaying;
    }

    public float GetVoiceLength()
    {
        return voiceAudioSource != null && voiceAudioSource.clip != null ? 
            voiceAudioSource.clip.length : 0f;
    }
    
}
