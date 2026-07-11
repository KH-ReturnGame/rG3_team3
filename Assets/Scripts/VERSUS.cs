using UnityEngine;
using Unity.Cinemachine; // <-- 최신 버전은 네임스페이스가 이렇게 바뀜!

public class VersusCameraController : MonoBehaviour
{
    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Camera Components")]
    public CinemachineCamera virtualCamera; // v3에서는 CinemachineCamera를 사용

    [Header("Zoom Settings")]
    public float minZoom = 4f;
    public float maxZoom = 10f;
    public float zoomFactor = 1.5f;
    public float zoomSpeed = 5f;

    private void LateUpdate()
    {
        if (player1 == null || player2 == null || virtualCamera == null)
            return;

        // 1. 두 플레이어의 평균 위치(중심점) 계산
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        transform.position = centerPoint;

        // 2. 두 플레이어 사이의 거리 계산
        float distance = Vector3.Distance(player1.position, player2.position);

        // 3. 거리에 비례하는 목표 줌(Lens.OrthographicSize) 값 계산
        float targetZoom = Mathf.Clamp(distance / zoomFactor, minZoom, maxZoom);

        // 4. [수정됨] v3에서는 Lens.OrthographicSize로 직접 접근함 (m_Lens 아님)
        virtualCamera.Lens.OrthographicSize = Mathf.Lerp(
            virtualCamera.Lens.OrthographicSize,
            targetZoom,
            Time.deltaTime * zoomSpeed
        );
    }
}