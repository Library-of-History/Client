using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public static class HapticUtil
{
    public static void SendHaptic(IXRSelectInteractor interactor, float intensity, float duration)
    {
        if (interactor == null)
            return;
            
        intensity = Mathf.Clamp01(intensity);
        
        var component = interactor as Component;
        if (component == null) 
            return;
        
        Debug.Log($"[HapticUtil] Interactor component: {component.name}, Parent: {component.transform.parent?.name}");
        
        var parentName = component.transform.parent?.name ?? "";

        if (parentName.Contains("Left"))
        {
            SendHapticToDevice(InputSystem.GetDevice<XRController>(CommonUsages.LeftHand),intensity, duration);
        }
        else if (parentName.Contains("Right"))
        {
            SendHapticToDevice(InputSystem.GetDevice<XRController>(CommonUsages.RightHand),intensity, duration);
        }
    }

    private static void SendHapticToDevice(InputDevice device, float intensity, float duration)
    {
        if (device == null)
            return;

        var command = UnityEngine.InputSystem.XR.Haptics.SendHapticImpulseCommand.Create(
            0,
            intensity,
            duration
            );

        device.ExecuteCommand(ref command);
    }
}
