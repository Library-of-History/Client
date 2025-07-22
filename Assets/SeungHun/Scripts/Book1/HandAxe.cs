using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HandAxe : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      var grab = GetComponent<XRGrabInteractable>();
      
      GameObject attachGo = new GameObject("HandAxe");
      attachGo.transform.SetParent(transform);
      attachGo.transform.localPosition = Vector3.zero;
      attachGo.transform.localRotation = Quaternion.Euler(0,0,180);
      
      grab.attachTransform = attachGo.transform;
    }
}
