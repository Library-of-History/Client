using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Inst { get; private set; }
    public static string ApiUrl = "http://221.163.19.142:58002";

    public SceneData ScenesData;
    
    public string Token;
    public UIEnvironment CurrentEnv = UIEnvironment.MR;
    public string CurrentSceneName;
    public string CurrentSelectedBookName;
    
    public GameObject MRScene;
    public GameObject SystemUI;
    public GameObject Portal;
    public GameObject PortalSelectUI;
    public GameObject MRSelectedObject;
    
    public bool IsDocentProcessing = false;
    
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            ScenesData.Init();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
