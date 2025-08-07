using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource docentVoiceSource;
    [SerializeField] private AudioSource charaVoiceSource;

    public AudioSource GetBgmSource()
    {
        return bgmSource;
    }
    
    public AudioSource GetSfxSource()
    {
        return sfxSource;
    }
    
    public AudioSource GetDocentVoiceSource()
    {
        return docentVoiceSource;
    }
    
    public AudioSource GetCharaVoiceSource()
    {
        return charaVoiceSource;
    }
}
