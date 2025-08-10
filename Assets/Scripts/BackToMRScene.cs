using UnityEngine;
using UnityEngine.SceneManagement;
using Anaglyph.Menu;
using Anaglyph.XRTemplate;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

public class BackToMRScene : MonoBehaviour
{
    public void OnClick()
    {
        Ending();
    }

    private async UniTaskVoid Ending()
    {
        var cutscene = SystemManager.Inst.CurrentEndingCutscene;
        cutscene.Play();
        
        while (cutscene.state == PlayState.Playing)
        {
            await UniTask.Yield();
        }
        
        SystemManager.Inst.FadeUI.FadeToWhite(() =>
        {
            SystemManager.Inst.SystemUI.GetComponentInChildren<UIControllerPresenter>(true).EnvSwitch();
            var menuPositioner = SystemManager.Inst.SystemUI.GetComponent<MenuPositioner>();
            
            if (SystemManager.Inst.SystemUI.activeSelf)
            {
                menuPositioner.ToggleVisible();
            }
            
            menuPositioner.ToggleVisible();
            menuPositioner.ToggleVisible();
        
            var asyncOp = SceneManager.UnloadSceneAsync(SystemManager.Inst.CurrentSceneName);

            CheckSceneUnloaded(asyncOp);
            
            SystemManager.Inst.MRScene.SetActive(true);
            PassthroughManager.SetPassthrough(true); 
        });
    }

    private async UniTaskVoid CheckSceneUnloaded(AsyncOperation asyncOp)
    {
        while (!asyncOp.isDone)
        {
            await UniTask.Yield();
        }
        
        SystemManager.Inst.FadeUI.SetCamera();
        SystemManager.Inst.FadeUI.ResetFadeEffect(() =>
        {
            SystemManager.Inst.CurrentReadingBook.GetComponent<BookInteraction>().FinishBookInteractionSequence(() =>
            {
                SystemManager.Inst.CurrentReadingBook.GetComponent<BookState>().UI.SetActive(true);
                SystemManager.Inst.CurrentReadingBook = null;
            });
        });
    }
}
