using UnityEngine;
using Unity.Cinemachine;

public class FightingGameCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform enemy;
    public CinemachineVirtualCamera vcam;

    [Header("Camera Settings")]
    public float minZoom = 5f;       // 최소 카메라 줌 (가까울 때)
    public float maxZoom = 12f;      // 최대 카메라 줌 (멀어질 때)
    public float zoomFactor = 1.5f;  // 거리에 따른 줌 민감도
    public float zoomSpeed = 5f;     // 줌 변경 속도 (부드러운 전환)

    public float yOffset = 2f;       // 화면 중앙을 살짝 위로 올리기 위한 오프셋

    void Update()
    {
        if (player == null || enemy == null || vcam == null)
            return;

        MoveCameraTarget();
        AdjustCameraZoom();
    }

    private void MoveCameraTarget()
    {
        // 1. 두 캐릭터의 정확한 중간 지점 계산
        Vector3 middlePoint = (player.position + enemy.position) / 2f;

        // 격투 게임 특성상 발밑보다는 상체를 비추는 것이 좋으므로 Y축 오프셋 추가
        middlePoint.y += yOffset;

        // 이 스크립트가 붙어있는 오브젝트를 중간 지점으로 이동시킴
        transform.position = middlePoint;
    }

    private void AdjustCameraZoom()
    {
        // 2. 플레이어와 적 사이의 X축 거리 계산 (2D이므로 주로 X축 거리가 중요함)
        float distance = Mathf.Abs(player.position.x - enemy.position.x);

        // 3. 거리에 비례하여 목표 줌 크기(Orthographic Size) 계산
        float targetZoom = Mathf.Clamp(distance * zoomFactor, minZoom, maxZoom);

        // 4. Mathf.Lerp를 사용해 현재 줌에서 목표 줌으로 부드럽게 변경
        vcam.m_Lens.OrthographicSize = Mathf.Lerp(
            vcam.m_Lens.OrthographicSize,
            targetZoom,
            Time.deltaTime * zoomSpeed
        );
    }
}