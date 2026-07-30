using UnityEngine;
using System.Collections.Generic;

public class SoundHandler : MonoBehaviour
{
    public static SoundHandler Instance { get; private set; }

    [Header("타격 사운드 리스트")]
    [SerializeField] private List<AudioClip> hitSounds = new List<AudioClip>();

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

    // 적 위치에서 피격 사운드 랜덤 재생 및 자동 삭제
    public void PlayHitSoundAtPosition(Vector3 position, float volume = 1.0f)
    {
        if (hitSounds == null || hitSounds.Count == 0)
        {
            Debug.LogWarning("[SoundHandler] hitSounds 리스트가 비어있습니다!");
            return;
        }

        // 1. 리스트에서 랜덤 사운드 뽑기
        int randomIndex = Random.Range(0, hitSounds.Count);
        AudioClip selectedClip = hitSounds[randomIndex];

        if (selectedClip == null) return;

        // 2. 적 위치에 임시 오디오 오브젝트 생성
        GameObject soundObj = new GameObject("TempHitSound");
        soundObj.transform.position = position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = selectedClip;
        audioSource.volume = volume;

        // 3. 피치 변주 (0.85 ~ 1.15)
        audioSource.pitch = Random.Range(0.85f, 1.15f);

        // 4. 재생
        audioSource.Play();

        // 5. 재생이 끝나면 오디오 오브젝트 자동 삭제
        float destroyDelay = selectedClip.length / Mathf.Max(0.1f, audioSource.pitch);
        Destroy(soundObj, destroyDelay);
    }
}