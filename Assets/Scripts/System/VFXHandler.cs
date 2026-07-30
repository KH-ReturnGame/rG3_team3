using UnityEngine;
using System.Collections.Generic;

public class VFXHandler : MonoBehaviour
{
    public static VFXHandler Instance { get; private set; }

    // 자주 쓰는 이펙트 캐싱용 (매번 로드하면 렉걸리니까)
    private Dictionary<string, GameObject> vfxCache = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 이펙트 이름, 생성 위치, 지워질 시간(초)
    public void PlayEffect(string vfxName, Vector3 spawnPosition, float destroyTime = 1.0f)
    {
        GameObject prefab = null;

        // 1. 이미 불러온 적 있는지 확인
        if (vfxCache.ContainsKey(vfxName))
        {
            prefab = vfxCache[vfxName];
        }
        else
        {
            // 2. 없으면 Resources/VFX/ 폴더에서 찾아서 로드
            prefab = Resources.Load<GameObject>("Particles/" + vfxName);

            if (prefab != null)
            {
                vfxCache.Add(vfxName, prefab);
            }
            else
            {
                Debug.LogError($"Particles/{vfxName} 프리팹을 찾을 수 없음!");
                return;
            }
        }

        // 3. 생성하고 지정한 시간 뒤 자동 삭제
        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);
        Destroy(obj, destroyTime);
    }
}