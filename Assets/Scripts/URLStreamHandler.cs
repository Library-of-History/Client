using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class URLStreamHandler : DownloadHandlerScript
{
    private TTSStreamManager _manager;
    private StringBuilder _buffer = new StringBuilder();

    private string Url = "http://221.163.19.142:58026";

    public URLStreamHandler(TTSStreamManager manager) : base(new byte[8192])
    {
        _manager = manager;
    }
    
    [System.Serializable]
    public class BaseMessage
    {
        public string type;
    }

    
    [System.Serializable]
    public class AudioData
    {
        public string text;
        public string url;
    }

    [System.Serializable]
    public class AudioUrlMessage
    {
        public AudioData data;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0) return false;

        string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
        _buffer.Append(chunk);

        // URL은 "\n" 구분자로 들어온다고 가정
        string fullText = _buffer.ToString();
        int newlineIndex;

        while ((newlineIndex = fullText.IndexOf("\n\n")) != -1)
        {
            string line = fullText.Substring(6, newlineIndex - 6).Trim();
            _buffer.Remove(0, newlineIndex + 2);
            fullText = _buffer.ToString();
            
            Debug.Log(line);

            if (!string.IsNullOrEmpty(line))
            {
                try
                {
                    BaseMessage msg = JsonUtility.FromJson<BaseMessage>(line);
                    if (msg.type == "audio_url")
                    {
                        AudioUrlMessage body = JsonUtility.FromJson<AudioUrlMessage>(line);
                        
                        string url = body.data.url;
                        url = Url + url;
                        
                        Debug.Log(url);

                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            _manager.EnqueueAudioUrl(url).Forget();  // URL만 큐에 추가
                        });
                    }
                    else
                    {
                        Debug.LogWarning("파싱된 JSON 형식이 예상과 다름");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON 파싱 실패: " + e.Message);
                }
            }
        }

        return true;
    }
}