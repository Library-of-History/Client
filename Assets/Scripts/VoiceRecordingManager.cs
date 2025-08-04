using UnityEngine;
using UnityEngine.InputSystem;

public class VoiceRecordingManager : MonoBehaviour
{
    private TTSStreamManager streamManager;
    
    private AudioClip trimmedClip;
    private AudioClip recordedClip;
    private string micDevice;
    
    private bool isRecording = false;

    private void Awake()
    {
        streamManager = GetComponent<TTSStreamManager>();
    }

    public void OnLeftAxisClick(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (SystemManager.Inst.IsDocentProcessing)
            {
                return;
            }
            
            if (!isRecording)
            {
                isRecording = true;
                StartRecording();
                return;
            }

            isRecording = false;
            SystemManager.Inst.IsDocentProcessing = true;
            StopRecording();
        }
    }

    private void StartRecording()
    {
        micDevice = Microphone.devices[0];
        recordedClip = Microphone.Start(micDevice, false, 3000, 44100);
    }

    private void StopRecording()
    {
        if (!Microphone.IsRecording(micDevice))
            return;

        int pos = Microphone.GetPosition(micDevice);
        Microphone.End(micDevice);

        if (pos <= 0)
            return;

        float[] samples = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(samples, 0);

        // 실제 녹음된 데이터 길이만큼 자르기, 채널 수 고려
        float[] trimmedSamples = new float[pos * recordedClip.channels];
        System.Array.Copy(samples, trimmedSamples, trimmedSamples.Length);

        // 새 AudioClip 생성 (이름, 샘플 프레임수, 채널 수, 샘플링 레이트, PCM 데이터 여부)
        trimmedClip = AudioClip.Create("TrimmedClip", pos, recordedClip.channels, recordedClip.frequency, false);
        trimmedClip.SetData(trimmedSamples, 0);

        var filename = "MyQuery.wav";
        SavWav.Save(filename, trimmedClip);
        streamManager.StartTTSStream(filename);
        
        // trimmedClip을 AudioSource 등에 연결해 재생할 수 있음
    }
}
