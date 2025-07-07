using Anaglyph.XRTemplate;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Anaglyph.Lasertag
{
	public class Spawner : MonoBehaviour
	{
		[SerializeField] private float rotateSpeed;

		[SerializeField] private GameObject objectToSpawn;
		private GameObject previewObject;
		private float rotating;
		private float angle;

		[SerializeField] private LineRenderer lineRenderer;
		
		private HandedHierarchy hand;

		public static Spawner Left { get; private set; }
		public static Spawner Right { get; private set; }

		private void Awake()
		{
			hand = GetComponentInParent<HandedHierarchy>(true);

			if (hand.Handedness == InteractorHandedness.Left)
				Left = this;
			else if (hand.Handedness == InteractorHandedness.Right)
				Right = this;

			lineRenderer.useWorldSpace = false;
			lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
			
			previewObject = InstantiateObjectAsPreview(objectToSpawn);
		}

		public void SetObjectToSpawn(GameObject objectToSpawn)
		{
			this.objectToSpawn = objectToSpawn;

			if(previewObject != null)
				Destroy(previewObject);

			previewObject = InstantiateObjectAsPreview(objectToSpawn);
		}

		private void Update()
		{
			angle += rotating * Time.deltaTime * rotateSpeed;

			lineRenderer.enabled = false;
			previewObject.SetActive(false);

			if (previewObject == null)
			{
				return;
			}

			bool overUI = hand.RayInteractor.IsOverUIGameObject();
			if(overUI)
			{
				return;
			}

			lineRenderer.SetPosition(1, Vector3.forward);
			lineRenderer.enabled = true;

			Ray ray = new(transform.position, transform.forward);
			bool didHit = EnvironmentMapper.Raycast(ray, 50, out var result, true);
			if(!didHit)
			{
				return;
			}

			previewObject.SetActive(true);

			previewObject.transform.position = result.point;
			previewObject.transform.eulerAngles = new(0, angle, 0);
		}

		private void OnEnable()
		{
			Vector3 forw = transform.forward;
			forw.y = 0;
			angle = Vector3.SignedAngle(Vector3.forward, forw, Vector3.up);
		}
		private void OnDisable()
		{
			if (previewObject != null)
				previewObject.SetActive(false);
		}

		private void OnFire(InputAction.CallbackContext context)
		{
			if (context.performed && context.ReadValueAsButton())
			{
				var position = previewObject.transform.position;
				var rotation = previewObject.transform.rotation;

				Instantiate(objectToSpawn, position: position, rotation: rotation);
			}
		}
		
		private void OnAxis(InputAction.CallbackContext context)
		{
			rotating = -context.ReadValue<Vector2>().x;
		}

		private static GameObject InstantiateObjectAsPreview(GameObject obj)
		{
			GameObject preview = Instantiate(obj);
			preview.SetActive(true);

			return preview;
		}
	}
}