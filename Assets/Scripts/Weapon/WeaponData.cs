using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("무기 기본 정보")]
    public string weaponName;
    public int weaponType;           // 2: Dagger, 1: One Hand Blade, 3: Two Hand Blade, 4: Rifle, 5: Pistol, 6: Spear (123456으로 구분)   

    [Header("무기 유형별 특수 스탯")]
    // 추후 무기별 버프 생각 추가 예정

    [Header("타이밍 및 제어")]
    public float attackCooldown;

    [Header("이동 및 돌진")]
    public bool hasForwardMove;
    public float forwardForce;

    [Header("공격 및 히트박스 정보")]
    public float damage;
    public Vector2 hitboxSize;
    public Vector2 hitboxOffset;

    public float criticalMultiplier = 1f;

    [Header("특수 효과 및 이펙트")]
    public bool hasSpecialVFX;
    public string vfxName;

    [Header("피격 효과")]
    public bool hasHitEffect;
    public string HitEffect;
    public float effectDamage;
    public float effectDuration;

    [Header("피격 및 상태이상 (Stun)")]
    public float stunTime;           // 적에게 먹일 기절/스턴 시간 
    public float knockbackForce;     // 적을 밀어내는 힘

}