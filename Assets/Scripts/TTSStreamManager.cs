using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using VideoKit;

public class TTSStreamManager : MonoBehaviour
{
    private string apiUrl = "http://221.163.19.142:58026/voice-interaction";
    private AudioSource audioSource;
    private Queue<string> audioUrlQueue = new Queue<string>();
    
    private bool isPlaying = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // 외부에서 호출: 사용자의 질의 전달 시작
    public async UniTask StartTTSStream(string filename)
    {
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new QueryRequest { text = query }));
        //
        // UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        // request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        // request.downloadHandler = new URLStreamHandler(this); // DownloadHandlerScript 커스텀
        // request.SetRequestHeader("Content-Type", "multipart/form-data");
        
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
            Debug.Log("스트리밍 시작..");
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

        while (true)
        {
            string url = null;

            lock (audioUrlQueue)
            {
                if (audioUrlQueue.Count > 0)
                    url = audioUrlQueue.Dequeue();
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
}