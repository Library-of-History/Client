using UnityEngine;
// System.Collections는 Coroutine에 필요하지만,
// 이 버전은 Coroutine을 사용하지 않으므로 더 이상 필요하지 않습니다.

public class DirectVRTeleportNoFade : MonoBehaviour
{
    // 인스펙터에서 드래그 앤 드롭으로 할당
    public Transform vrCameraRig; // 씬의 XR Origin 또는 OVRCameraRig의 Transform
    public Transform teleportPointGround; // 지상 순간이동 지점 (Empty GameObject)
    public Transform teleportPointAir;    // 공중 순간이동 지점 (Empty GameObject)

    // 페이드 효과 관련 변수들은 더 이상 필요 없으므로 제거됩니다.
    // public CanvasGroup fadeScreen;
    // public float fadeDuration = 0.5f;

    // --- 지상으로 순간이동하는 함수 ---
    public void TeleportToGround()
    {
        DoTeleport(teleportPointGround.position, teleportPointGround.rotation);
    }

    // --- 공중으로 순간이동하는 함수 ---
    public void TeleportToAir()
    {
        DoTeleport(teleportPointAir.position, teleportPointAir.rotation);
    }

    // --- 실제 순간이동 로직 (페이드 효과 없음) ---
    private void DoTeleport(Vector3 targetPosition, Quaternion targetRotation)
    {
        // 1. 페이드 아웃 로직 제거

        // 2. VR Rig 위치/회전 변경
        if (vrCameraRig != null)
        {
            vrCameraRig.position = targetPosition;
            vrCameraRig.rotation = targetRotation;
            // 참고: VR Rig의 Rotation은 헤드셋의 초기 방향을 설정합니다.
            // 플레이어의 실제 헤드 트래킹은 카메라가 독립적으로 처리합니다.
        }

        // 3. 페이드 인 로직 제거
    }
}