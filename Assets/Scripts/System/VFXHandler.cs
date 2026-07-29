using UnityEngine;

public class VFXHandler : MonoBehaviour
{
    public static VFXHandler Instance { get; private set; }

    [SerializeField] private GameObject defaultHitVFX;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            // 필요하다면 Scene 전환 시에도 유지
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayEffect(Vector3 spawnPosition, GameObject vfxPrefab = null)
    {
        GameObject targetPrefab = vfxPrefab != null ? vfxPrefab : defaultHitVFX;

        if (targetPrefab == null)
        {
            Debug.LogWarning("[VFXHandler] 재생할 VFX 프리팹이 지정되지 않았습니다.");
            return;
        }

        // 프리팹을 위치에 스폰 (회전값 기본)
        Debug.Log("DASDASDASDA");
        Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
    }
}