using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Anaglyph.XRTemplate
{
	// TODO: find a better way to do this
	public class DisableOverUI : MonoBehaviour
	{
		[SerializeField] private Behaviour[] behaviours;
		[SerializeField] private XRRayInteractor rayInteractor;

		private bool overUI = true;
		private bool overPortal = true;

		private void Update()
		{
			bool newOverUI = rayInteractor.IsOverUIGameObject();
			bool newOverPortal = false;
			
			if (SystemManager.Inst.Portal != null)
			{
				var portalObjects = 
					SystemManager.Inst.Portal.GetComponentsInChildren<XRSimpleInteractable>();

				foreach (var obj in portalObjects)
				{
					newOverPortal |= rayInteractor.IsHovering(obj);
				}
			}

			if (newOverUI != overUI || newOverPortal != overPortal)
			{
				overUI = newOverUI;
				overPortal = newOverPortal;
				foreach (Behaviour behaviour in behaviours)
				{
					behaviour.enabled = !overPortal & !overUI;
				}
			}
		}
	}
}
