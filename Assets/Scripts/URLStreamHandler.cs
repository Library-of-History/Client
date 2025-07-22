using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class URLStreamHandler : DownloadHandlerScript
{
    private TTSStreamManager _manager;
    private StringBuilder _buffer = new StringBuilder();

    public URLStreamHandler(TTSStreamManager manager) : base(new byte[8192])
    {
        _manager = manager;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0) return false;

        string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
        _buffer.Append(chunk);

        // URL은 "\n" 구분자로 들어온다고 가정
        string fullText = _buffer.ToString();
        int newlineIndex;

        while ((newlineIndex = fullText.IndexOf('\n')) != -1)
        {
            string line = fullText.Substring(0, newlineIndex).Trim();
            _buffer.Remove(0, newlineIndex + 1);
            fullText = _buffer.ToString();

            if (!string.IsNullOrEmpty(line))
            {
                Debug.Log("<color=cyan>🌐 URL 수신됨:</color> " + line);
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    _manager.EnqueueAudioUrl(line).Forget(); // 받은 URL을 큐에 추가
                });
            }
        }

        return true;
    }
}