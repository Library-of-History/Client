using Anaglyph.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(-100)]
public class SystemManager : MonoBehaviour
{
    public static SystemManager Inst { get; private set; }
    
    public GameObject LoginUI;
    public GameObject SystemUI;
    public static string ApiUrl = "http://221.163.19.142:58002";

    public AudioManager AudioManagerInst;
    public SceneData SceneDataInst;
    
    public string Token;
    public UIEnvironment CurrentEnv = UIEnvironment.MR;
    public string CurrentSceneName;
    public string CurrentSelectedBookName;
    public PlayableDirector CurrentEndingCutscene;
    
    public GameObject MRScene;
    public SceneSwitchFadeEffect FadeUI;
    public GameObject Portal;
    public GameObject PortalSelectUI;
    public GameObject ScreenShotUI;
    public GameObject MRSelectedObject;
    public GameObject CurrentReadingBook;

    public GameObject Docent;
    public GameObject DocentSummonEffect;
    public bool IsDocentProcessing = false;

    public int TutorialState = 0;
    
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

    public void ActiveLoginMode()
    {
        var loginMenuPositioner = LoginUI.GetComponent<MenuPositioner>();
        loginMenuPositioner.EnableMenuAction();
        
        if (!LoginUI.activeSelf)
        {
            loginMenuPositioner.ToggleVisible();
        }
    }
    
    public void DeActiveLoginMode()
    {
        var loginMenuPositioner = LoginUI.GetComponent<MenuPositioner>();
        loginMenuPositioner.DisableMenuAction();

        var managers = LoginUI.GetComponentsInChildren<LoginManager>(true);
        managers[0].InitPage();
        managers[1].InitPage();
        
        if (LoginUI.activeSelf)
        {
            loginMenuPositioner.ToggleVisible();
        }
    }

    public void ActiveGamePlayMode()
    {
        var systemMenuPositioner = SystemUI.GetComponent<MenuPositioner>();
        systemMenuPositioner.EnableMenuAction();
        
        if (!SystemUI.activeSelf)
        {
            systemMenuPositioner.ToggleVisible();
        }

        var uiControllerPresenter = SystemUI.GetComponentInChildren<UIControllerPresenter>(true);
        uiControllerPresenter.SetInitState();
    }
    
    public void DeActiveGamePlayMode()
    {
        var systemMenuPositioner = SystemUI.GetComponent<MenuPositioner>();
        systemMenuPositioner.DisableMenuAction();
        
        if (SystemUI.activeSelf)
        {
            systemMenuPositioner.ToggleVisible();
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
        
        DocentSummonEffect.transform.SetParent(Docent.transform);
        DocentSummonEffect.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        DocentSummonEffect.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        DocentSummonEffect.GetComponent<ParticleSystem>().Play();
    }
    
    public void DeleteDocent()
    {
        DocentSummonEffect.transform.SetParent(null);
        DocentSummonEffect.GetComponent<ParticleSystem>().Play();
        Docent.SetActive(false);
    }
}
