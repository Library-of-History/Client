using System;
using Anaglyph.Menu;
using Anaglyph.XRTemplate;
using UnityEngine;

public class SummonPortal : MonoBehaviour
{
    [SerializeField] private GameObject portal;
    
    private Camera mainCamera;
    private Transform camTransform => mainCamera.transform;
    private Vector3 offset = new Vector3(0f, -1.1f, 1.2f);
        
    private void OnEnable()
    {
        mainCamera = Camera.main;
    }

    public void OnClick()
    {
        Vector3 flatForward = camTransform.forward.normalized;
        Matrix4x4 pose = Matrix4x4.LookAt(camTransform.position, camTransform.position + flatForward, Vector3.up);
        
        var position = pose.MultiplyPoint(offset);
        
        Vector3 forward = (position - camTransform.position).normalized;

        var rotation = Quaternion.LookRotation(forward, Vector3.up);

        if (SystemManager.Inst.Portal != null)
        {
            Destroy(SystemManager.Inst.Portal);
        }
        
        SystemManager.Inst.Portal = Instantiate(portal, position, rotation * Quaternion.Euler(-110f, 0f, 180f));
        GetComponentInParent<MenuPositioner>().gameObject.SetActive(false);
    }
}
