using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class SystemManager : MonoBehaviour
{
    public static SystemManager Inst { get; private set; }
    public static string ApiUrl = "http://221.163.19.142:58002";

    public AudioManager AudioManagerInst;
    public SceneData SceneDataInst;
    
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
            SceneDataInst.Init();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
