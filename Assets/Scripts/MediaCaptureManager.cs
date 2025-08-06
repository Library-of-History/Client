using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class MediaCaptureManager : MonoBehaviour
{
    private StringBuilder sb;
    private string saveDir;
    
    private int count;
    private bool delayForNextCapture = false;

    private void Awake()
    {
        sb = new StringBuilder();
        saveDir = Path.Combine(Application.persistentDataPath, SystemManager.Inst.CurrentSceneName);

        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        var files = Directory.GetFiles(saveDir);
        count = files.Length + 1;
    }
    
    public void OnLeftButtonX(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (!delayForNextCapture)
            {
                sb.Clear();
                sb.Append(Path.Combine(saveDir, count.ToString()));
                sb.Append(".png");
                
                ScreenCapture.CaptureScreenshot(sb.ToString());
                count++;
                delayForNextCapture = true;
                Invoke(nameof(ChangeFlag), 3f);
            }
        }
    }

    private void ChangeFlag()
    {
        delayForNextCapture = false;
    }
}
