using Anaglyph.XRTemplate;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Anaglyph.Lasertag
{
	public class Spawner : MonoBehaviour
	{
		[SerializeField] private float rotateSpeed;
		[SerializeField] private float moveSpeed;

		private GameObject objectToSpawn;
		private GameObject previewObject;
		
		private float rotatingX;
		private float rotatingY;
		private float angleX;
		private float angleY;
		
		private float directionX;
		private float directionY;
		private float directionZ;
		private float distanceX;
		private float distanceY;
		private float distanceZ;

		[SerializeField] private LineRenderer lineRenderer;
		
		private HandedHierarchy hand;
		private bool isGripClicked = false;

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
			
			gameObject.SetActive(false);
		}

		public void SetObjectToSpawn(GameObject objectToSpawn)
		{
			this.objectToSpawn = objectToSpawn;
			SystemManager.Inst.MRSelectedObject = objectToSpawn;

			if(previewObject != null)
				Destroy(previewObject);

			previewObject = InstantiateObjectAsPreview(objectToSpawn);
		}

		private void Update()
		{
			if (objectToSpawn == null || previewObject == null)
			{
				return;
			}
			
			angleY += Time.deltaTime * rotateSpeed * rotatingY;
			angleX += Time.deltaTime * rotateSpeed * rotatingX;
			
			distanceX += Time.deltaTime * moveSpeed * directionX;
			distanceY += Time.deltaTime * moveSpeed * directionY;
			distanceZ += Time.deltaTime * moveSpeed * directionZ;

			lineRenderer.enabled = false;
			previewObject.SetActive(false);

			bool overUI = hand.RayInteractor.IsOverUIGameObject();
			bool overPortal = false;
			
			if (SystemManager.Inst.Portal != null)
			{
				var portalObjects = 
					SystemManager.Inst.Portal.GetComponentsInChildren<XRSimpleInteractable>();

				foreach (var obj in portalObjects)
				{
					overPortal |= hand.RayInteractor.IsHovering(obj);
				}
			}
			
			if(overUI || overPortal)
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

			previewObject.transform.position = result.point +
			                                   (transform.right * distanceX)
			                                   + (transform.up * distanceY)
			                                   + (transform.forward * (distanceZ - 0.1f));
			previewObject.transform.eulerAngles = new (angleX, angleY, 0);
		}

		private void OnEnable()
		{
			Vector3 forw = transform.forward;
			forw.y = 0;
			angleY = Vector3.SignedAngle(Vector3.forward, forw, Vector3.up);
			angleX = 0f;

			distanceX = 0f;
			distanceY = 0f;
			distanceZ = 0f;
		}
		
		private void OnDisable()
		{
			if (previewObject != null)
				previewObject.SetActive(false);
		}

		private void OnRightFire(InputAction.CallbackContext context)
		{
			if (context.performed && context.ReadValueAsButton())
			{
				if (objectToSpawn == null || previewObject == null)
				{
					return;
				}
				
				var position = previewObject.transform.position;
				var rotation = previewObject.transform.rotation;

				var obj = Instantiate(objectToSpawn, position: position, rotation: rotation);
				SystemManager.Inst.MRSelectedObject = null;
				
				obj.transform.SetParent(SystemManager.Inst.MRScene.transform);
				
				gameObject.SetActive(false);
			}
		}

		private void OnRightButtonA(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				directionZ = -1f;
			}
			else if (context.canceled)
			{
				directionZ = 0f;
			}
		}

		private void OnRightButtonB(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				directionZ = 1f;
			}
			else if (context.canceled)
			{
				directionZ = 0f;
			}	
		}
		
		private void OnRightAxis(InputAction.CallbackContext context)
		{
			if (!isGripClicked)
			{
				rotatingX = context.ReadValue<Vector2>().y;
				rotatingY = -context.ReadValue<Vector2>().x;
			}
			else
			{
				directionX = context.ReadValue<Vector2>().x;
				directionY = context.ReadValue<Vector2>().y;
			}
		}

		private void OnRightGrip(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				isGripClicked = true;
				rotatingX = 0f;
				rotatingY = 0f;
			}
			else if (context.canceled)
			{
				isGripClicked = false;
				directionX = 0f;
				directionY = 0f;
			}
		}

		private static GameObject InstantiateObjectAsPreview(GameObject obj)
		{
			GameObject preview = Instantiate(obj);
			preview.SetActive(true);

			return preview;
		}
	}
}