using UnityEngine;

public class NPCDetectionZone : MonoBehaviour
{
    private NPCTriggerDetection parentNPC;

    public void Initialize(NPCTriggerDetection parent)
    {
        parentNPC = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject))
            return;

        if (!other.CompareTag("Player"))
            return;

        parentNPC.HandlePlayerEnter(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject))
            return;

        if (!other.CompareTag("Player"))
            return;
        
        parentNPC.HandlePlayerExit(other.transform);
    }

    private bool IsPlayerLayer(GameObject obj)
    {
        return (parentNPC.PlayerLayerMask & (1 << obj.layer)) != 0;
    }
}
