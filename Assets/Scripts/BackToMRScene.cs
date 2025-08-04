using UnityEngine;
using UnityEngine.SceneManagement;
using Anaglyph.Menu;
using Anaglyph.XRTemplate;

public class BackToMRScene : MonoBehaviour
{
    public void OnClick()
    {
        SystemManager.Inst.SystemUI.GetComponentInChildren<UIControllerPresenter>().EnvSwitch();
        
        if (SystemManager.Inst.SystemUI.activeSelf)
        {
            SystemManager.Inst.SystemUI.GetComponent<MenuPositioner>().ToggleVisible();
        }
        
        SceneManager.UnloadSceneAsync("LifeStyle_Prehistory_Paleolithic");
        
        SystemManager.Inst.MRScene.SetActive(true);
        PassthroughManager.SetPassthrough(true);
    }
}
