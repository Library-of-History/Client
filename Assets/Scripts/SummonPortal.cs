using Anaglyph.XRTemplate;
using UnityEngine;

public class SummonPortal : MonoBehaviour
{
    [SerializeField] private GameObject portal;
    private Vector3 offset = new Vector3(0f, 0.7f, 1f);

    public void OnClick()
    {
        Instantiate(portal, MainXROrigin.Transform.position + offset, Quaternion.Euler(0f, 90f, 0f));
    }
}
