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

        if (isDownSmash)
        {
            rb2d.linearVelocity = new Vector2(0, -40f); 
        }
        else
        {
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            rb2d.AddForce(new Vector2(dir * 40f, 0), ForceMode2D.Impulse);
        }

        float elapsed = 0f;
        HashSet<Collider2D> hitHistory = new HashSet<Collider2D>();

        while (elapsed < duration)
        {
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
    }

    private IEnumerator PerformDash()
    {
        isDebounce = true;
        
        float x = Input.GetAxisRaw("Horizontal"); // A(-1), D(1)
        float y = Input.GetAxisRaw("Vertical");   // S(-1), W(1)
        
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
        
        AddDataToList("DashCD", Cooldownlist, 2f);
        
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

        // 1. 다이렉트
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

        // 2. 키를 누르고 있는 중
        if (Input.GetKey(key) && castType == SkillCastType.Charging && isCharging)
        {
            chargeTimer += Time.deltaTime;
        }

        // 3. 키를 뗀 순간
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

    // 다이렉트 시전
    private void ExecuteDirectSkill(KeyCode inputKey)
    {
        isDebounce = true;
        Debug.Log($"[다이렉트] {inputKey} 스킬 발동");
        
        // 기본 범위 공격 실행 (오프셋, 크기, 지속시간, 데미지, 다운스매시여부)
        StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.15f, 30f, false));

        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 3f);
        StartCoroutine(ResetDebounce(0.3f));
    }

    // 차징 완료 후 시전 (시간 비례 데미지 반영)
    private void ExecuteChargingSkill(KeyCode inputKey, float duration)
    {
        isDebounce = true;

        // 1. 차징 비율 계산 (0.0 ~ 1.0 사이로 제한)
        float chargeRatio = Mathf.Clamp01(duration / maxChargeTime);

        // 2. 시간에 비례한 데미지 계산
        float minDamage = 20f;
        float maxDamage = 80f;
        float finalDamage = minDamage + (maxDamage - minDamage) * chargeRatio;

        // 3. 차징 정도에 따른 이펙트 및 판정 차별화
        if (chargeRatio >= 1.0f)
        {
            Debug.Log($"[풀차징] {inputKey} 스킬 발동, 데미지: {finalDamage:F1} (최대 데미지)");
            // 풀차징 시에는 공격 범위(size)를 더 크게 설정 (예: 14f, 8f)
            StartCoroutine(ExecuteAttack(new Vector2(3f, 0.5f), new Vector2(14f, 8f), 0.3f, finalDamage, false));
        }
        else
        {
            Debug.Log($"[차징 미달] {inputKey} 스킬 발동, 차징시간: {duration:F1}초, 데미지: {finalDamage:F1}");
            // 일반 차징 범위
            StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.15f, finalDamage, false));
        }

        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 4f);
        StartCoroutine(ResetDebounce(0.4f));
    }

    // 홀딩(누르는 중) 시전 로직
    private IEnumerator ExecuteHoldingSkill(KeyCode inputKey)
    {
        isDebounce = true;
        Debug.Log($"[홀딩 시작] {inputKey} 스킬");

        float holdingTimer = 0f;
        float maxHoldingTime = 5.0f; // 홀딩 최대 제한 시간
        float damagePerTick = 10f;   // 틱당 데미지

        // 키를 누르고 있고, 스턴이 아니며, 최대 제한 시간을 넘지 않을 때만 반복
        while (isHolding && !isStun && holdingTimer < maxHoldingTime)
        {
            Debug.Log($"공격 중, 현재 유지 시간: {holdingTimer:F1}초");
            
            // 0.2초마다 주변 적에게 10의 데미지를 주는 짧은 공격 판정 생성
            StartCoroutine(ExecuteAttack(new Vector2(2f, 0.5f), new Vector2(8f, 5f), 0.05f, damagePerTick, false));

            yield return new WaitForSeconds(0.2f); // 틱 간격
            holdingTimer += 0.2f;
        }

        // 제한 시간을 초과해서 탈출한 경우를 위해 상태 강제 해제
        if (holdingTimer >= maxHoldingTime)
        {
            Debug.Log($"[최대 시간 초과] {inputKey} 스킬 종료");
            isHolding = false; // Update문과의 동기화를 위해 변수 꺼줌
        }
        else
        {
            Debug.Log($"[홀딩 종료] {inputKey} 스킬 종료");
        }

        AddDataToList(inputKey.ToString() + "CD", Cooldownlist, 5f);
        isDebounce = false;
    }
}