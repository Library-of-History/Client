using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using VideoKit;

[RequireComponent(typeof(VideoKitRecorder))]
public class MediaCaptureManager : MonoBehaviour
{
    private VideoKitRecorder recorder;
    
    private bool isRecording = false;
    private bool delayForNextCapture = false;

    private void Awake()
    {
        recorder = GetComponent<VideoKitRecorder>();
    }
    
    public void OnLeftButtonX(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (!delayForNextCapture)
            {
                ScreenCapture.CaptureScreenshot("sample.png");
                delayForNextCapture = true;
                Invoke(nameof(ChangeFlag), 3f);
            }
        }
    }

    public void OnLeftButtonY(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (isRecording)
            {
                isRecording = false;
                recorder.StopRecording();
                return;
            }

            isRecording = true;
            recorder.StartRecording();
        }
    }

    private void ChangeFlag()
    {
        delayForNextCapture = false;
    }
}
