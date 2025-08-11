using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

public class TTSStreamManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    
    private Animator animator;
    private static readonly int isThinkingHash = Animator.StringToHash("IsThinking");
    private static readonly int isTalkingHash = Animator.StringToHash("IsTalking");

    private string apiUrl = "/voice-interaction";
    private Queue<string> audioUrlQueue = new Queue<string>();
    
    private bool isPlaying = false;

    private void Awake()
    {
        audioSource.spatialBlend = 0f;
        SystemManager.Inst.AudioManagerInst.OnDocentVolumeChanged += VolumeChange;
        
        apiUrl = SystemManager.ApiUrl + apiUrl;
        animator = SystemManager.Inst.Docent.GetComponent<Animator>();
    }

    private void VolumeChange(float value)
    {
        audioSource.volume = value;
    }

    // 외부에서 호출: 사용자의 질의 전달 시작
    public async UniTask StartTTSStream(string filename)
    {
        animator.SetBool(isThinkingHash, true);
        animator.SetBool(isTalkingHash, false);
        
        string filepath = Path.Combine(Application.persistentDataPath, filename);
        byte[] wavData = File.ReadAllBytes(filepath);
        
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio_file", wavData, Path.GetFileName(filepath), "audio/wav");
        
        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);
        request.downloadHandler = new URLStreamHandler(this);
        request.SetRequestHeader("Authorization", "Bearer " + SystemManager.Inst.Token);

        await request.SendWebRequest().ToUniTask();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("스트리밍 요청 실패: " + request.error);
        }
        else
        {
            Debug.Log("스트리밍 성공!");
            CheckIfDonePlaying();
        }
    }

    // 큐에 URL이 추가되면 재생 시작
    public async UniTaskVoid EnqueueAudioUrl(string url)
    {
        lock (audioUrlQueue)
        {
            audioUrlQueue.Enqueue(url);
        }

        if (!isPlaying)
        {
            await PlayQueue();
        }
    }

    // 큐에서 하나씩 꺼내서 재생
    private async UniTask PlayQueue()
    {
        isPlaying = true;
        animator.SetBool(isThinkingHash, false);
        animator.SetBool(isTalkingHash, true);

        while (true)
        {
            string url = null;

            lock (audioUrlQueue)
            {
                if (audioUrlQueue.Count > 0)
                {
                    url = audioUrlQueue.Dequeue();
                }
            }

            if (string.IsNullOrEmpty(url))
                break;

            Debug.Log("재생 시작: " + url);

            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                    audioSource.clip = clip;
                    audioSource.Play();
                    
                    await UniTask.WaitUntil(() => !audioSource.isPlaying);
                }
                else
                {
                    Debug.LogError("오디오 재생 실패: " + req.error);
                }
            }
        }

        isPlaying = false;
    }

    private async UniTaskVoid CheckIfDonePlaying()
    {
        while (audioSource.isPlaying || audioUrlQueue.Count > 0)
        {
            await UniTask.Yield();
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(10));
        
        audioSource.clip = null;
        SystemManager.Inst.IsDocentProcessing = false;
        animator.SetBool(isThinkingHash, false);
        animator.SetBool(isTalkingHash, false);

        SystemManager.Inst.DeleteDocent();
    }
}