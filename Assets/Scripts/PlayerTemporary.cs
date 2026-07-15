using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillCastType
{
    None,
    Direct,      // 다이렉트 즉시 시전
    Charging,    // 차징 후 시전
    Holding      // 키를 누르는 동안 지속
}

[System.Serializable]
public class WeaponConfig
{
    public string weaponName;
    public SkillCastType zSkillType;
    public SkillCastType xSkillType;
    public SkillCastType cSkillType;
}

// --- [ 카드 시스템을 위한 데이터 구조들 ] ---
public enum CardType
{
    Skill,          // 1. 스킬
    Stat,           // 2. 능력치 상승
    Passive,        // 3. 패시브 능력
    Weapon          // 4. 무기
}

[System.Serializable]
public class CardData
{
    public string cardID;           // 시스템 내부 구분을 위한 ID (예: "Skill_1", "Stat_2")
    public string cardName;         // 화면에 표시될 이름
    public CardType cardType;       // 카드의 유형
    public int maxUpgradeLevel = 3; // 스킬, 패시브용 최대 강화 상한 수치

    [Header("무기 설정 (무기 유형일 때만 입력)")]
    public WeaponConfig weaponConfig;

    [TextArea(2, 3)]
    public string description;      // 카드 간단 설명
}

public class PlayerTemporary : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    private float movementSpeed = 10;
    private float jumpForce = 25f;

    private float moveInput;

    public List<string> Stunlist = new List<string>();
    public List<string> Cooldownlist = new List<string>();

    public string PressingKey;

    private int Combo = 0;
    private float lastComboChangeTime;

    private bool isUptilt;
    private bool isDowntilt;

    public bool isDebounce = false;

    private int jumpCount = 0;
    public int maxJumpCount = 2;

    public bool isStun = false;
    private bool isDashing = false;
    private float dashCooldown = 2f;
    private float dashCancelCooldown = 10f;
    private float dashForce = 30f;

    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    [Header("무기 및 스킬 시스템")]
    public List<WeaponConfig> myWeapons = new List<WeaponConfig>();
    public int currentWeaponIndex = 0;

    private float chargeTimer = 0f;
    private float maxChargeTime = 1.5f; // 최대 차징 시간
    private bool isCharging = false;
    private bool isHolding = false;

    // =========================================================================
    // [카드 시스템을 위해 추가된 내부 변수들]
    // =========================================================================
    [Header("--- 카드 보상 시스템 데이터 ---")]
    [Tooltip("에디터 인스펙터에서 카드 종류(스킬1~4, 능력치1~4 등)를 자유롭게 빼거나 더할 수 있습니다.")]
    public List<CardData> rewardCardPool = new List<CardData>();

    [Header("유형별 출현 확률 (합계 100)")]
    public float skillChance = 35f;
    public float statChance = 40f;
    public float passiveChance = 20f;
    public float weaponChance = 5f; // 매우 희귀하게 설정

    // 플레이어가 획득한 실시간 누적 상태 기록용 (Dictionary는 인스펙터에 안 보이므로 확인용 List 병행 가능)
    private Dictionary<string, int> acquiredSkills = new Dictionary<string, int>();
    private Dictionary<string, int> acquiredPassives = new Dictionary<string, int>();
    private Dictionary<string, int> acquiredStats = new Dictionary<string, int>();

    private List<CardData> currentChoices = new List<CardData>(); // 현재 화면에 뜬 카드 4개
    private int selectedCount = 0;                               // 현재 스테이지에서 선택한 개수 (최대 2개)

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    PressingKey = key.ToString();
                }
            }
        }

        isUptilt = Input.GetKey(KeyCode.W);
        isDowntilt = Input.GetKey(KeyCode.S);

         if (!Stunlist.Contains("Stun"))
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            
            if (isGrounded)
            {
                jumpCount = 0; 
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded || jumpCount < maxJumpCount)
                {
                    rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
                    jumpCount++;
                }
            }
        }

        if (moveInput > 0)
            sprdr.flipX = false;
        else if (moveInput < 0)
            sprdr.flipX = true;

        if (moveInput != 0)
            animtr.SetBool("isWalking", true);
        else
            animtr.SetBool("isWalking", false);

        if (Combo != 0 && Time.time - lastComboChangeTime >= 1f)
        {
            Combo = 0;
            Debug.Log("COMBO RESET DUE TO TIMER");
        }

        if (Combo == 3)
        {
            AddDataToList("LightCooldown", Cooldownlist, 0.7f);
            Combo = 0;
            Debug.Log("COMBO RESET DUE TO END OF STACK");
        }

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.L)) && !isDebounce)
        {
            if (!Cooldownlist.Contains("LightCooldown"))
            {
                isDebounce = true;
                Combo++;
                lastComboChangeTime = Time.time;

                Debug.Log(Combo);

                if (!isGrounded && isDowntilt)
                {
                    StartCoroutine(ExecuteAttack(new Vector2(0f, -2f), new Vector2(8f, 4f), 0.15f, 40f, true));
                }
                else
                {
                    StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false));
                }

                AddDataToList("LightCooldown", Cooldownlist, 0.26f);
                StartCoroutine(ResetDebounce(0.26f));
            }
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isDebounce && !isStun)
        {
            if (!Cooldownlist.Contains("DashCD"))
            {
                StartCoroutine(PerformDash());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!Cooldownlist.Contains("DashCancelCD"))
            {
                rb2d.linearVelocity = Vector2.zero;
                AddDataToList("DashCancelCD", Cooldownlist, 10f);
                Debug.Log("대쉬 캔슬 발동");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isDebounce)
        {
            if (!Cooldownlist.Contains("Skill1CD"))
            {
                isDebounce = true;
                Debug.Log("Skill 1 Activated");
                AddDataToList("Skill1CD", Cooldownlist, 3f);
                StartCoroutine(ResetDebounce(0.5f));
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isDebounce)
        {
            if (!Cooldownlist.Contains("Skill2CD"))
            {
                isDebounce = true;
                Debug.Log("Skill 2 Activated");
                AddDataToList("Skill2CD", Cooldownlist, 5f);
                StartCoroutine(ResetDebounce(0.5f));
            }
        }
        if (myWeapons.Count > 0 && currentWeaponIndex < myWeapons.Count)
        {
            WeaponConfig currentWeapon = myWeapons[currentWeaponIndex];

            HandleSkillInput(KeyCode.Z, currentWeapon.zSkillType);
            HandleSkillInput(KeyCode.X, currentWeapon.xSkillType);
            HandleSkillInput(KeyCode.C, currentWeapon.cSkillType);
            HandleSkillInput(KeyCode.V, currentWeapon.cSkillType);
        }

        // 테스팅용 코드: 스페이스바 옆의 G 키를 누르면 스테이지 클리어 상황을 시뮬레이션합니다.
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnStageClear();
        }
    }

    void FixedUpdate()
    {
        if (!Stunlist.Contains("Stun") && !isStun && !isDebounce)
        {
            rb2d.linearVelocity = new Vector2(moveInput * movementSpeed, rb2d.linearVelocity.y);
        }
    }

    private IEnumerator ResetDebounce(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDebounce = false;
    }

    private void AddDataToList(string dataName, List<string> dataList, float duration)
    {
        StartCoroutine(Activate());
        IEnumerator Activate()
        {
            dataList.Add(dataName);
            yield return new WaitForSeconds(duration);
            dataList.Remove(dataName);
        }
    }

    private IEnumerator ExecuteAttack(Vector2 offset, Vector2 size, float duration, float damage, bool isDownSmash)
    {
        float dir = sprdr.flipX ? -1f : 1f;
        float originalGravity = rb2d.gravityScale; 
        bool isAirAttack = !isGrounded && !isDownSmash; 

        if (isDownSmash)
        {
            rb2d.linearVelocity = new Vector2(0, -40f); 
        }
        else
        {
            if (isAirAttack)
            {
                rb2d.linearVelocity = new Vector2(0, Mathf.Max(0f, rb2d.linearVelocity.y * 0.2f)); 
                rb2d.gravityScale = originalGravity * 0.15f; 
            }
            else
            {
                rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
                rb2d.AddForce(new Vector2(dir * 40f, 0), ForceMode2D.Impulse);
            }
        }

        float elapsed = 0f;
        HashSet<Collider2D> hitHistory = new HashSet<Collider2D>();

        while (elapsed < duration)
        {
            if (isAirAttack && rb2d.linearVelocity.y < -1f)
            {
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, -1f);
            }

            float currentDir = sprdr.flipX ? -1f : 1f;
            Vector2 pos = (Vector2)transform.position + new Vector2(offset.x * currentDir, offset.y);

            Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0f, LayerMask.GetMask("Enemy"));

            foreach (var hit in hits)
            {
                if (!hitHistory.Contains(hit))
                {
                    Debug.Log("JUST HIT: " + hit.name);
                    hitHistory.Add(hit);

                    if (hit.TryGetComponent(out Rigidbody2D enemyRb))
                    {
                        if (isDownSmash)
                        {
                            enemyRb.linearVelocity = Vector2.zero;
                            enemyRb.AddForce(Vector2.down * 140f, ForceMode2D.Impulse);
                            Debug.Log("DOWN SMASH!");
                        }
                        else if (isUptilt)
                        {
                            enemyRb.linearVelocity = Vector2.zero;
                            enemyRb.AddForce(Vector2.up * 25f, ForceMode2D.Impulse);
                        }
                        else
                        {
                            enemyRb.AddForce(new Vector2(dir * 10f, 0), ForceMode2D.Impulse);
                        }
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isAirAttack)
        {
            rb2d.gravityScale = originalGravity;
        }
    }

    private IEnumerator PerformDash()
    {
        isDebounce = true;
        
        float x = Input.GetAxisRaw("Horizontal"); 
        float y = Input.GetAxisRaw("Vertical");   
        
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Space))
        {
            x = 1f;
            y = 1f;
        }
        
        Vector2 dashDir = new Vector2(x, y).normalized;
        
        if (dashDir == Vector2.zero)
        {
            dashDir = new Vector2(sprdr.flipX ? -1f : 1f, 0);
        }
        
        rb2d.linearVelocity = dashDir * dashForce;
        
        AddDataToList("DashCD", Cooldownlist, dashCooldown); // 가변 dashCooldown 변수로 연동
        
        yield return new WaitForSeconds(0.2f);

        isDebounce = false;
    }
    
    private void OnDrawGizmos()
    {
        if (sprdr == null) return;
        Gizmos.color = Color.red;
        float dir = sprdr.flipX ? -1f : 1f;
        Gizmos.DrawWireCube(transform.position + new Vector3(2f * dir, 0.5f, 0), new Vector3(9f, 6f, 1f));
    }

    private void HandleSkillInput(KeyCode key, SkillCastType castType)
    {
        if (castType == SkillCastType.None || Cooldownlist.Contains(key.ToString() + "CD")) return;

        if (Input.GetKeyDown(key) && !isDebounce && !isStun)
        {
            switch (castType)
            {
                case SkillCastType.Direct:
                    ExecuteDirectSkill(key);
                    break;
                case SkillCastType.Charging:
                    isCharging = true;
                    chargeTimer = 0f;
                    Debug.Log($"{key} 차징 시작");
                    break;
                case SkillCastType.Holding:
                    isHolding = true;
                    StartCoroutine(ExecuteHoldingSkill(key));
                    break;
            }
        }

        if (Input.GetKey(key) && castType == SkillCastType.Charging && isCharging)
        {
            chargeTimer += Time.deltaTime;
        }

        if (Input.GetKeyUp(key))
        {
            switch (castType)
            {
                case SkillCastType.Charging:
                    if (isCharging)
                    {
                        isCharging = false;
                        ExecuteChargingSkill(key, chargeTimer);
                    }
                    break;
                case SkillCastType.Holding:
                    if (isHolding)
                    {
                        isHolding = false;
                    }
                    break;
            }
        }
    }

    private void ExecuteDirectSkill(KeyCode inputKey)
    {
        isDebounce = true;
        Debug.Log($"[다이렉트] {inputKey} 스킬 발동");
        StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.15f, 30f, false));
        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 3f);
        StartCoroutine(ResetDebounce(0.3f));
    }

    private void ExecuteChargingSkill(KeyCode inputKey, float duration)
    {
        isDebounce = true;
        float chargeRatio = Mathf.Clamp01(duration / maxChargeTime);

        float minDamage = 20f;
        float maxDamage = 80f;
        float finalDamage = minDamage + (maxDamage - minDamage) * chargeRatio;

        if (chargeRatio >= 1.0f)
        {
            Debug.Log($"[풀차징] {inputKey} 스킬 발동, 데미지: {finalDamage:F1}");
            StartCoroutine(ExecuteAttack(new Vector2(3f, 0.5f), new Vector2(14f, 8f), 0.3f, finalDamage, false));
        }
        else
        {
            Debug.Log($"[차징 미달] {inputKey} 스킬 발동, 데미지: {finalDamage:F1}");
            StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.15f, finalDamage, false));
        }

        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 4f);
        StartCoroutine(ResetDebounce(0.4f));
    }

    private IEnumerator ExecuteHoldingSkill(KeyCode inputKey)
    {
        isDebounce = true;
        Debug.Log($"[홀딩 시작] {inputKey} 스킬");

        float holdingTimer = 0f;
        float maxHoldingTime = 8.0f;
        float damagePerTick = 10f;

        while (isHolding && !isStun && holdingTimer < maxHoldingTime)
        {
            StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(8f, 5f), 0.05f, damagePerTick, false));
            yield return new WaitForSeconds(0.2f);
            holdingTimer += 0.2f;
        }

        if (holdingTimer >= maxHoldingTime)
        {
            isHolding = false;
        }

        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 5f);
        isDebounce = false;
    }
    
    // [카드 시스템 핵심 구현 함수군]
    public void OnStageClear()
    {
        Debug.Log("<color=yellow>스테이지의 모든 적 처치 완료! 보상 카드 4개를 출력합니다.</color>");
        selectedCount = 0;
        currentChoices.Clear();

        // 1. 가중치 확률 기반으로 랜덤 카드 4개 뽑기
        for (int i = 0; i < 4; i++)
        {
            CardData rolledCard = GetRandomCardByWeight();
            if (rolledCard != null) currentChoices.Add(rolledCard);
        }

        // 2. 화면 콘솔 및 UI 시뮬레이션 출력
        DisplayChoicesUI();
    }

    // 설정된 확률 가중치 기반 랜덤 카드 추출 함수
    private CardData GetRandomCardByWeight()
    {
        if (rewardCardPool.Count == 0) return null;

        float totalValue = Random.Range(0f, 100f);
        CardType selectedType;

        if (totalValue < weaponChance)
            selectedType = CardType.Weapon;
        else if (totalValue < weaponChance + passiveChance)
            selectedType = CardType.Passive;
        else if (totalValue < weaponChance + passiveChance + skillChance)
            selectedType = CardType.Skill;
        else
            selectedType = CardType.Stat;

        // 인펙터 리스트에서 해당 타입의 카드를 골라냄
        List<CardData> matchedCards = rewardCardPool.FindAll(c => c.cardType == selectedType);

        // 혹시 에디터에 해당 타입 카드가 아예 등록되지 않았다면 예외 처리로 풀 전체에서 랜덤 추출
        if (matchedCards.Count == 0)
        {
            return rewardCardPool[Random.Range(0, rewardCardPool.Count)];
        }

        return matchedCards[Random.Range(0, matchedCards.Count)];
    }

    // 임시 UI 텍스트 출력 로직
    private void DisplayChoicesUI()
    {
        Debug.Log("------ [ 보상 선택: 아래 4개 중 2개를 고르세요 ] ------");
        for (int i = 0; i < currentChoices.Count; i++)
        {
            CardData card = currentChoices[i];
            Debug.Log($"[버튼 인덱스: {i}] 유형: {card.cardType} | 이름: {card.cardName} | 설명: {card.description}");
        }
        Debug.Log("선택하려면 컴포넌트나 UI 버튼을 통해 'SelectCardByIndex(인덱스)' 함수를 호출하세요.");
    }

    /// <summary>
    /// UI 카드 버튼 클릭 시 연결될 이벤트용 함수 (인덱스: 0 ~ 3)
    /// </summary>
    public void SelectCardByIndex(int choiceIndex)
    {
        if (selectedCount >= 2)
        {
            Debug.LogWarning("이미 이번 스테이지에서 2개의 카드를 모두 선택했습니다.");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count) return;

        CardData chosenCard = currentChoices[choiceIndex];
        ApplyCardEffect(chosenCard);

        selectedCount++;
        Debug.Log($"카드가 선택되었습니다 ({selectedCount} / 2)");

        if (selectedCount >= 2)
        {
            Debug.Log("<color=cyan>선택 완료! 다음 스테이지로 진입할 수 있습니다.</color>");
            // 필요 시 이곳에 다음 스테이지 맵 이동 로직 배치
        }
    }

    // 선택된 카드의 효과 처리 로직 (중복 조건 계산 포함)
    private void ApplyCardEffect(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.Weapon:
                // 중복 체크: 이미 무기 이름이 리스트에 있다면 무기 유지 (동작 없음), 없으면 추가
                bool exists = myWeapons.Exists(w => w.weaponName == card.weaponConfig.weaponName);
                if (exists)
                {
                    Debug.Log($"이미 장착 중인 무기 [{card.weaponConfig.weaponName}] 입니다. 무기가 유지됩니다.");
                }
                else
                {
                    myWeapons.Add(card.weaponConfig);
                    currentWeaponIndex = myWeapons.Count - 1; // 획득 즉시 새로 장착
                    Debug.Log($"새로운 무기를 획득하여 장착했습니다: {card.weaponConfig.weaponName}");
                }
                break;

            case CardType.Stat:
                // 제약 조건 없는 중복 중첩(스택) 처리
                if (!acquiredStats.ContainsKey(card.cardID)) acquiredStats[card.cardID] = 0;
                acquiredStats[card.cardID]++;
                
                Debug.Log($"능력치 상승 처리됨 -> ID: {card.cardID} (누적 스택: {acquiredStats[card.cardID]})");

                // 플레이어 기존 변수에 실시간 연동 예시
                if (card.cardID == "Stat_1") // 예: 이동 속도 카드
                {
                    movementSpeed += 1.5f;
                    Debug.Log($"[스탯 반영] 플레이어 이동 속도가 {movementSpeed}로 증가했습니다.");
                }
                else if (card.cardID == "Stat_2") // 예: 대쉬 쿨다운 감소 카드
                {
                    dashCooldown = Mathf.Max(0.4f, dashCooldown - 0.2f);
                    Debug.Log($"[스탯 반영] 대쉬 쿨타임이 {dashCooldown}초로 단축되었습니다.");
                }
                break;

            case CardType.Skill:
                // 중복 획득 시 '강화' 성격 (한도 설정 적용)
                HandleUpgradableReward(card, acquiredSkills, "스킬");
                break;

            case CardType.Passive:
                // 중복 획득 시 '강화' 성격 (한도 설정 적용)
                HandleUpgradableReward(card, acquiredPassives, "패시브");
                break;
        }
    }

    // 스킬 및 패시브 공용 강화 한도 체크 로직
    private void HandleUpgradableReward(CardData card, Dictionary<string, int> registry, string typeLabel)
    {
        if (!registry.ContainsKey(card.cardID))
        {
            registry[card.cardID] = 1;
            Debug.Log($"새로운 {typeLabel} 획득: {card.cardName} (최초 1레벨)");
        }
        else
        {
            if (registry[card.cardID] < card.maxUpgradeLevel)
            {
                registry[card.cardID]++;
                Debug.Log($"{typeLabel} 강화 완료: {card.cardName} (현재 {registry[card.cardID]}레벨)");
            }
            else
            {
                Debug.Log($"{typeLabel} [{card.cardName}]은 이미 마스터 단계({card.maxUpgradeLevel}레벨)입니다. 강화 한도 초과.");
            }
        }
    }
}