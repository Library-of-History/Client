using System.IO;
using System.Collections;
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
                StartCoroutine(CaptureAndSave());
                delayForNextCapture = true;
                Invoke(nameof(ChangeFlag), 3f);
            }
        }
    }
    
    private IEnumerator CaptureAndSave()
    {
        yield return new WaitForEndOfFrame();

        int width = 320;
        int height = 240;

        var rt = new RenderTexture(width, height, 24);
        Camera.main.targetTexture = rt;
        Camera.main.Render();

        RenderTexture.active = rt;
        
        Texture2D screenImage = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenImage.Apply();

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

        byte[] imageBytes = screenImage.EncodeToPNG();
        string filepath = Path.Combine(saveDir, count + ".jpg");
        File.WriteAllBytes(filepath, imageBytes);

        count++;
        Destroy(screenImage);
    }

    private void ChangeFlag()
    {
        delayForNextCapture = false;
    }
}
