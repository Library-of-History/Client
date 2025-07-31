using Anaglyph.Menu;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PortalSelectReceiver : MonoBehaviour
{
    public void OnSelected()
    {
        if (SystemManager.Inst.MRSelectedObject == null)
        {
            var canvas = SystemManager.Inst.PortalSelectUI.GetComponentInChildren<Canvas>();
            var menuPositioner = SystemManager.Inst.PortalSelectUI.GetComponent<MenuPositioner>();

            if (!canvas.enabled)
            {
                canvas.enabled = true;
                menuPositioner.ToggleVisible();
            }
            
            menuPositioner.ToggleVisible();
        }
    }
}
