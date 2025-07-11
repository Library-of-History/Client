using System;
using Anaglyph.XRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Anaglyph.Lasertag
{
	public class Deleter : MonoBehaviour
	{
		[SerializeField] private Transform cursor;
		[SerializeField] LineRenderer lineRenderer;
		
		private HandedHierarchy hand;
		private GameObject selectedObject;

		private void Awake()
		{
			hand = GetComponentInParent<HandedHierarchy>(true);

			lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
			lineRenderer.useWorldSpace = false;
		}

		private void Update()
		{
			lineRenderer.enabled = false;
			cursor.gameObject.SetActive(false);

			bool overUI = hand.RayInteractor.IsOverUIGameObject();
			if(overUI)
			{
				return;
			}

			lineRenderer.SetPosition(1, Vector3.forward);
			lineRenderer.enabled = true;

			Ray ray = new(transform.position, transform.forward);
			bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo);
			selectedObject = hitInfo.collider?.gameObject;
			
			if (!didHit || selectedObject == null)
			{
				return;
			}

			cursor.gameObject.SetActive(true);

			cursor.position = hitInfo.point;
			lineRenderer.SetPosition(1, Vector3.forward * hitInfo.distance);
		}

		private void OnRightFire(InputAction.CallbackContext context)
		{
			if (context.performed && context.ReadValueAsButton())
			{
				Destroy(selectedObject);
				gameObject.SetActive(false);
			}
		}
	}
}