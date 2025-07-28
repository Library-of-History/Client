using UnityEngine;
using UnityEngine.InputSystem;

public class MediaCaptureManager : MonoBehaviour
{
    private bool isRecording = false;
    private bool delayForNextCapture = false;
    
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
            if (!isRecording)
            {
                return;
            }
        }
    }

    private void ChangeFlag()
    {
        delayForNextCapture = false;
    }
}
