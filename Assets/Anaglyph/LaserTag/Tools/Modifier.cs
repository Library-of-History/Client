using Anaglyph.XRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Anaglyph.Lasertag
{
    public class Modifier : MonoBehaviour
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
            if(overUI)
            {
                return;
            }

            lineRenderer.SetPosition(1, Vector3.forward);
            lineRenderer.enabled = true;
            
            if (selectedObject == null)
            {
                Ray ray = new(transform.position, transform.forward);
                bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo);
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
                selectedObject.transform.eulerAngles = new(angleX, angleY, 0);
            }
        }

        private void OnRightFire(InputAction.CallbackContext context)
        {
            if (context.performed && context.ReadValueAsButton())
            {
                if (selectedObject == null)
                {
                    selectedObject = target;
                    storedPos = selectedObject.transform.position;
                    storedRot = selectedObject.transform.eulerAngles;
                    target = null;
                }
                else
                {
                    storedPos = selectedObject.transform.position;
                    storedRot = selectedObject.transform.eulerAngles;
                    selectedObject = null;
                    gameObject.SetActive(false);
                }
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
    }
}