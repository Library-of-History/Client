using Unity.VisualScripting;
using UnityEngine;

public class DestroyPortal : MonoBehaviour
{
    public void OnClick()
    {
        Destroy(SystemManager.Inst.Portal);
        SystemManager.Inst.PortalSelectUI.SetActive(false);
    }
}
