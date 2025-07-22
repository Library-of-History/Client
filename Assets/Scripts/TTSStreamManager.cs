using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

public class TTSStreamManager : MonoBehaviour
{
    [Header("질의용 API URL (문장별 URL 스트리밍 반환)")]
    public string apiUrl = "http://localhost:8000/stream";
    public AudioSource audioSource;

    private Queue<string> audioUrlQueue = new Queue<string>();
    private bool isPlaying = false;

    // 외부에서 호출: 사용자의 질의 전달 시작
    public async UniTask StartTTSStream(string query)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new QueryRequest { text = query }));

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new URLStreamHandler(this); // DownloadHandlerScript 커스텀
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest().ToUniTask();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("스트리밍 요청 실패: " + request.error);
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

            Debug.Log("<color=yellow>🎧 재생 시작: " + url + "</color>");

            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
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

    // 구조: POST 요청 Body 용
    [System.Serializable]
    public class QueryRequest
    {
        public string text;
    }
}