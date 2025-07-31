using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Inst { get; private set; }
    
    public string Token;
    public UIEnvironment CurrentEnv = UIEnvironment.MR;
    public GameObject Portal;
    public GameObject PortalSelectUI;
    public GameObject MRSelectedObject;
    
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
