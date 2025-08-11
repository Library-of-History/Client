using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class StaticQuoteQueue : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    private static readonly int isThinkingHash = Animator.StringToHash("IsThinking");
    private static readonly int isTalkingHash = Animator.StringToHash("IsTalking");

    private Queue<AudioClip> queue = new Queue<AudioClip>();
    private bool isPlaying = false;
    
    private void Awake()
    {
        audioSource.spatialBlend = 0f;
        SystemManager.Inst.AudioManagerInst.OnDocentVolumeChanged += VolumeChange;
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public async UniTaskVoid EnqueueAudio(AudioClip audioClip)
    {
        lock (queue)
        {
            queue.Enqueue(audioClip);
        }

        if (!isPlaying)
        {
            await PlayQueue();
        }
    }

    private async UniTask PlayQueue()
    {
        isPlaying = true;
        animator.SetBool(isThinkingHash, false);
        await UniTask.Yield();
        animator.SetBool(isTalkingHash, true);
        
        while (true)
        {
            AudioClip audioClip = null;

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    audioClip = queue.Dequeue();
                }
            }

            audioSource.clip = audioClip;
            audioSource.Play();
                    
            await UniTask.WaitUntil(() => !audioSource.isPlaying);

            lock (queue)
            {
                if (queue.Count == 0)
                {
                    break;
                }
            }
        }
        
        animator.SetBool(isThinkingHash, false);
        animator.SetBool(isTalkingHash, false);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        
        SystemManager.Inst.DeleteDocent();
        isPlaying = false;
    }
    
    private void VolumeChange(float value)
    {
        audioSource.volume = value;
    }
}
