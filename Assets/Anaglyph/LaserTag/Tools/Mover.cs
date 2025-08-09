using System;
using Anaglyph.XRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Anaglyph.Lasertag
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;
        [SerializeField] private float moveSpeed;
        
        [SerializeField] private Transform cursor;
        [SerializeField] LineRenderer lineRenderer;
		
        private HandedHierarchy hand;
        private Vector3 storedPos;
        private Vector3 storedRot;
        
        private GameObject target;
        private GameObject selectedObject;
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

        private float offset;
        
        private bool isGripClicked = false;

        private void Awake()
        {
            hand = GetComponentInParent<HandedHierarchy>(true);

            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
            lineRenderer.useWorldSpace = false;
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
            if (selectedObject != null)
            {
                selectedObject.transform.position = storedPos;
                selectedObject.transform.eulerAngles = storedRot;
                selectedObject = null;
            }
        }

        private void Update()
        {
            lineRenderer.enabled = false;
            cursor.gameObject.SetActive(false);

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
            
            if (selectedObject == null)
            {
                Ray ray = new(transform.position, transform.forward);
                bool didHit = Physics.Raycast(ray, out var hitInfo);

                target = hitInfo.collider?.gameObject;

                if (!didHit || target == null)
                {
                    return;
                }

                cursor.gameObject.SetActive(true);

                cursor.position = hitInfo.point;
                lineRenderer.SetPosition(1, Vector3.forward * hitInfo.distance);
            }
            else
            {
                selectedObject.SetActive(false);
                
                Ray ray = new(transform.position, transform.forward);
                bool didHit = EnvironmentMapper.Raycast(ray, 50, out var result, true);
                if(!didHit)
                {
                    return;
                }
                
                selectedObject.SetActive(true);
                
                angleY += Time.deltaTime * rotateSpeed * rotatingY;
                angleX += Time.deltaTime * rotateSpeed * rotatingX;
                
                distanceX += Time.deltaTime * moveSpeed * directionX;
                distanceY += Time.deltaTime * moveSpeed * directionY;
                distanceZ += Time.deltaTime * moveSpeed * directionZ;
                
                selectedObject.transform.position = result.point +
                                                    (transform.right * distanceX)
                                                    + (transform.up * distanceY)
                                                    + (transform.forward * (distanceZ - 0.1f));;

                if (Mathf.Approximately(offset, -90f))
                {
                    selectedObject.transform.eulerAngles = new(offset, angleY + 90f, 90f);
                    selectedObject.transform.eulerAngles -= new Vector3(angleX, 0f, 0f);
                }
                else
                {
                    selectedObject.transform.eulerAngles = new(offset, angleY + 90f, angleX + 90f);
                }
            }
        }

        private void OnRightFire(InputAction.CallbackContext context)
        {
            if (context.performed && context.ReadValueAsButton())
            {
                if (selectedObject == null)
                {
                    var book = target.GetComponentInParent<BookState>();
                    var animHandler = book.gameObject.GetComponent<BookInteraction>();

                    if (animHandler.IsAnimating)
                    {
                        return;
                    }
                    
                    if (book.IsOpened)
                    {
                        offset = -90f;
                    }
                    else
                    {
                        offset = 0f;
                    }
                    
                    selectedObject = book.gameObject;
                    storedPos = selectedObject.transform.position;
                    storedRot = selectedObject.transform.eulerAngles;
                    target = null;
                }
                else
                {
                    storedPos = selectedObject.transform.position;
                    storedRot = selectedObject.transform.eulerAngles;
                    selectedObject = null;
                }
            }
        }
        
        private void OnRightButtonA(InputAction.CallbackContext context)
        {
            if (selectedObject == null)
            {
                return;
            }
            
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
            if (selectedObject == null)
            {
                return;
            }
            
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
            if (selectedObject == null)
            {
                return;
            }
            
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
            if (selectedObject == null)
            {
                return;
            }
            
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
    }
}