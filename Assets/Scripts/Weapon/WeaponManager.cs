using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    // 이제 더 이상 인스펙터에서 노가다로 드래그 앤 드롭할 필요가 없습니다!
    private Dictionary<string, WeaponData> weaponDictionary = new Dictionary<string, WeaponData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWeaponDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWeaponDictionary()
    {
        // 핵심: Resources/WeaponDatabase 폴더 안에 있는 모든 WeaponData 스크립터블 오브젝트를 자동으로 로드합니다.
        WeaponData[] loadedWeapons = Resources.LoadAll<WeaponData>("WeaponDatabase");

        foreach (var weapon in loadedWeapons)
        {
            if (weapon != null && !weaponDictionary.ContainsKey(weapon.weaponName))
            {
                weaponDictionary.Add(weapon.weaponName, weapon);
                Debug.Log($"[WeaponManager] 무기 자동 등록 완료: {weapon.weaponName}");
            }
        }

        Debug.Log($"[WeaponManager] 총 {weaponDictionary.Count}개의 무기가 데이터베이스에 로드되었습니다.");
    }

    public WeaponData GetWeaponData(string weaponName)
    {
        if (weaponDictionary.TryGetValue(weaponName, out WeaponData data))
        {
            return data;
        }
        Debug.LogWarning($"[WeaponManager] '{weaponName}' 에 해당하는 무기 데이터가 없습니다.");
        return null;
    }
}