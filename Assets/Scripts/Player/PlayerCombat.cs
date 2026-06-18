using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCombat : MonoBehaviour
{
    private PlayerManager plrManager;
    private PlayerInput plrInput;

    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    // 활성 변수

    public bool isDebounce = false;

    // Attack - 평타 변수 

    public int currentComboStack = 0;
    private int previousComboStack;
    private float lastComboChangeTime;
    private float comboStackResetLimit = 1f; // 이후 Update에서 Data에서 정의 ( 평타 쿨타임 간격보다 약간 더 길게 잡으면 됨.)

    private int maxComboStack = 4; // 이게 문제네 이걸 ㅅㅂ 어케하지 그니깐 Weapon 데이터를 적용 시키는게 지금으로써는 
    /// <summary>
    /// 아래에 그 뭐냐 평타할떄 '사후적으로' 데이터를 인식시켜서 바꾸는건데, 아니면 차라리
    /// 
    /// 씬이 이니셜 라이즈 될때 데이터 세팅해주는 매니저에서 
    /// 
    /// 플레이어가 착용한 무기를 감지해서 Max Combo Stack 이런거를 바꾸게 할까? 
    /// </summary>


    void Awake()
    {
        plrInput = GetComponent<PlayerInput>();
        plrManager = GetComponent<PlayerManager>();

        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
    }

    void Update()
    {
         if (currentComboStack > 0 && Time.time - lastComboChangeTime >= comboStackResetLimit)
         {
             currentComboStack = 0;
             Debug.Log("시간 초과 (스택 변화 없음): 콤보 스택이 0으로 초기화되었습니다.");
         }
    }

    void FixedUpdate()
    {

    }

    public void RequestAttack()
    {
        if (plrManager.Stunlist.Count > 0) return;
        if (!plrManager.Cooldownlist.Contains("Attack"))
        {

            WeaponData weapon = WeaponManager.Instance.GetWeaponData(plrManager.currentWeapon);
            if (weapon == null) return; // 무기리스트에서 반환돼지 않으면 불가.

            currentComboStack++;
            lastComboChangeTime = Time.time;

            comboStackResetLimit = weapon.attackCooldown * 1.4f;

            Debug.Log("Combo : " + currentComboStack);
            Utility.DataManagement.ListManagement.AddData("Attack", plrManager.Cooldownlist, weapon.attackCooldown); // 1.5f는 임시. 

            if (weapon.hasForwardMove == true && !(plrInput.yInput == 1) )
            {
                Debug.Log("no W, Frontdash");
            }
            else if(plrInput.yInput == 1 && !(plrManager.isGrounded == true))
            {
                Debug.Log("Aerial Cleaves");
            }
            // 여따 애니메이션 관련 / 쿨타임 데이터 관련 코드 작성 예정


            // 애니메이션 타임 맞춰서 히트 박스 관리



            if (currentComboStack >= maxComboStack)
            {
                currentComboStack = 0;
                Debug.Log(" 초과로 인해 0으로 초기화진행.");
                Utility.DataManagement.ListManagement.AddData("Attack", plrManager.Cooldownlist, weapon.attackCooldown * 2.3f);
            }
        }

    }

    public void Skill1()
    {

    }

    void LateUpdate()
    {

    }

    /* 

   plrInput (평타 호출) -> 

   + plrCombat 작동순서 


    BasicAttack void 만들기
    - StunCheck 
    - CoolTimeCheck
    - DataCheck
    -> isSpecialAttack 체크

   < 만약 FALSE > 일때

   콤보 스택 1+ 
   콤보 n이상 이면 1로 초기화 
   Data에서 매 콤보스택마다의 타이밍 불러와서쓰기 

   Data에서 앞으로 전진하는 값이 주어졌는지 확인 

   애니메이션 이벤트 
    -> 히트박스 호출 및 매개변수로 데이터에서 추출한 값 대입
    -> 이펙트 모듈 호출

   -> 특수 데미지 Check 
    참이면 상대방 데미지 호출때 SpecialDamage키고 능력치 매개변수로 ㄱㄱ
    

    참고로 데미지 받는거에서 도트 데미지와 함께 딸려오는 이펛트는 데미지 모듈 안에서 처리.



    시간 초과하면 콤보 스택 초기화 



    + 공중인 상태로 W누른 상태로 클릭 누르면 붕뜬 상태로 평타 

   




  







     */
}
