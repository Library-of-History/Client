using UnityEngine;
using UnityEngine.Playables;
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
    public PlayableDirector CurrentEndingCutscene;
    
    public GameObject MRScene;
    public GameObject SystemUI;
    public SceneSwitchFadeEffect FadeUI;
    public GameObject Portal;
    public GameObject PortalSelectUI;
    public GameObject MRSelectedObject;
    public GameObject CurrentReadingBook;

    public GameObject Docent;
    public bool IsDocentProcessing = false;
    private Vector3 offset = new Vector3(-0.3f, -0.4f, 1f);
    
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

    public void SummonDocent()
    {
        Docent.SetActive(true);
        
        Transform camTransform = Camera.main.transform;
        Vector3 flatForward = camTransform.forward.normalized;
        Matrix4x4 pose = Matrix4x4.LookAt(camTransform.position, camTransform.position + flatForward, Vector3.up);
        
        Docent.transform.position = pose.MultiplyPoint(offset);
        Vector3 forward = -1 * (Docent.transform.position - camTransform.position).normalized;
        Docent.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
    
    public void DeleteDocent()
    {
        Docent.SetActive(false);
    }
}
