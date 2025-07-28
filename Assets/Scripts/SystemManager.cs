using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Inst { get; private set; }
    public string Token;
    
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
