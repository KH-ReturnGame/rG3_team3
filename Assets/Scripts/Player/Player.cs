using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body2d;
    [SerializeField] private Animator animtr;
    [SerializeField] private SpriteRenderer spdr;

    Transform spriteRenderOBJ;

    // input Relavant 
    public float xInput, yInput;

    // direction properties
    public bool isfacingRight = true;

    // basic_statement Relavant
    public bool isGrounded;
    public float checkRadius = 0.2f;
    public Transform groundCheck;
    public LayerMask GroundLayer;

    // character_realTime_variables
    public float moveSpeed = 5f;
    public float jumpHeight = 30f;

    [Header("평타 전진 설정")]
    [SerializeField] private float attackDashForce = 15f;
    [SerializeField] private float DashForce = 15f;

    // combo 
    public int currentComboStack = 0;
    private float lastComboChangeTime;
    private float comboStackResetLimit = 1.5f;
    private int maxComboStack = 4;

    public float attackCooldown = 0.36f;



    private AudioSource audioSource;

    [SerializeField] private AudioClip[] slashSounds; 




    // 공격 히트박스 & 이펙트 설정
    [Header("공격 히트박스 & 이펙트")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackHitboxSize = new Vector2(1.5f, 1.2f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damageAmount = 10;

    [Header("상태리스트")]
    public List<string> Stunlist = new List<string>();
    public List<string> FrameList = new List<string>();
    public List<string> DebounceList = new List<string>();

    [Header("내부상태리스트")]
    public List<string> Cooldownlist = new List<string>();

    void Awake()
    {
        spriteRenderOBJ = transform.Find("_SpriteRenderer");
        audioSource = GetComponent<AudioSource>();

        body2d = GetComponent<Rigidbody2D>();
        spdr = spriteRenderOBJ.GetComponent<SpriteRenderer>();
        animtr = spriteRenderOBJ.GetComponent<Animator>();
    }

    void Update()
    {
        if (!DebounceList.Contains("BasicAttackDash"))
        {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");
        }
        else
        {
            xInput = 0;
            yInput = 0;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, GroundLayer);

        animtr.SetInteger("Combo", currentComboStack);

        setSprite();

        keyInputdetection();

        if (xInput != 0)
        {
            animtr.SetBool("isWalking", true);
        }
        else
        {
            animtr.SetBool("isWalking", false);
        }

        if (currentComboStack > 0 && Time.time - lastComboChangeTime >= comboStackResetLimit)
        {
            currentComboStack = 0;
            Debug.Log("시간 초과 (스택 변화 없음): 콤보 스택이 0으로 초기화되었습니다.");
        }
    }

    void FixedUpdate()
    {
        launchMovement();

        animtr.SetFloat("yVelocity", body2d.linearVelocityY);

        directionInspection();
    }

    void launchMovement()
    {
        if (DebounceList.Contains("BasicAttackDash") || DebounceList.Contains("isDash"))
        {
            // 이동 코드를 실행하지 않고 바로 함수를 나감 (이동 차단)
            return;
        }

        body2d.linearVelocity = new Vector2(xInput * moveSpeed, body2d.linearVelocity.y);

    }

    void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackHitboxSize);
        }
    }

    private void keyInputdetection()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded == true)
            {
                StartCoroutine(Jump());
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Dash());
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.L))
        {
            BasicAttack();
        }
    }

    private void directionInspection()
    {
        if (Mathf.Abs(transform.localEulerAngles.y) > 0.1f)
            isfacingRight = false;
        else
            isfacingRight = true;
    }

    public void setSprite()
    {
        if (!DebounceList.Contains("BasicAttackDash"))
        {
            if (xInput > 0)
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0f, transform.eulerAngles.z);
            else if (xInput < 0)
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180f, transform.eulerAngles.z);
        }
    }

    private IEnumerator Jump()
    {
        if (Stunlist.Count > 0) yield break;
        if (DebounceList.Contains("isUsingMovement")) yield break;
        animtr.SetTrigger("isJumpTrigger");
        yield return new WaitForSeconds(.1f);
        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpHeight);
    }

    private IEnumerator Dash()
    {
        if (Cooldownlist.Contains("Dash")) yield break;
        if (DebounceList.Contains("isUsingMovement")) yield break;
        if (Stunlist.Count > 0) yield break;
        float dir;
        if (isfacingRight == true)
            dir = 1f;
        else
            dir = -1f;


        animtr.SetTrigger("isDash");
        yield return new WaitForSeconds(0.2f);

        Utility.DataManagement.ListManagement.AddData("Dash", Cooldownlist, .5f);

        Utility.DataManagement.ListManagement.AddData("isDash", DebounceList, 0.3f);


        body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
        body2d.AddForce(new Vector2(dir * DashForce, 0f), ForceMode2D.Impulse);
    }

    // ★ [수정] 평타 공격 함수
    private void BasicAttack()
    {
        // 1. 스턴 상태이거나 이미 공격 중(데바운스) 또는 쿨다운 중이면 공격 금지!
        if (Stunlist.Count > 0) return;
        if (DebounceList.Contains("BasicAttackDash")) return;
        if (DebounceList.Contains("isUsingMovement")) return;
        if (Cooldownlist.Contains("Attack")) return;

        // 2. 콤보 스택 증가
        currentComboStack++;

        // 3. 콤보 판단 및 쿨다운 적용
        float currentCooldown = attackCooldown;

        if (currentComboStack >= maxComboStack)
        {
            // 막타(4타) 시 긴 쿨다운 적용 후 스택 초기화
            currentCooldown = attackCooldown * 2.3f;
            currentComboStack = 0;
        }

        // 쿨다운 등록 (한 번만 적용)
        Utility.DataManagement.ListManagement.AddData("Attack", Cooldownlist, currentCooldown);
        Utility.DataManagement.ListManagement.AddData("isUsingMovement", DebounceList, currentCooldown - currentCooldown/10);


        lastComboChangeTime = Time.time;
        Debug.Log("Combo : " + currentComboStack);

        audioSource.pitch = Random.Range(0.7f, 1.2f);
        PlayRandomSlashSound();
        audioSource.pitch = 1.0f;

        // 애니메이터 설정
        animtr.SetInteger("Combo", currentComboStack == 0 ? maxComboStack : currentComboStack);
        animtr.SetTrigger("isAttackTrigger");

        // 공격 대쉬 연출 중 상태 등록 (0.5초간 추가 클릭/이동 제한)
        Utility.DataManagement.ListManagement.AddData("BasicAttackDash", DebounceList, 0.5f);

        // 평타 전진 코루틴 실행
        StartCoroutine(PerformAttackDash());
    }

    // ★ [수정] 대쉬 후 타격 처리 코루틴

    private void PlayRandomSlashSound()
    {
        if (slashSounds != null && slashSounds.Length > 0)
        {
            // 1. 배열 크기 안에서 랜덤 인덱스 뽑기
            int randomIndex = Random.Range(0, slashSounds.Length);
            AudioClip selectedClip = slashSounds[randomIndex];

            if (selectedClip != null)
            {
                // 2. 피치 변주 (0.85 ~ 1.15)
             

                // 3. 랜덤 오디오 원샷 재생
                audioSource.PlayOneShot(selectedClip);
            }
        }
    }


    private IEnumerator PerformAttackDash()
    {
        float dir = isfacingRight ? 1f : -1f;

        // 공격 시 선선모션/대기 시간
        yield return new WaitForSeconds(0.1f);

        // 스턴 체크 한번 더 수행
        if (Stunlist.Count > 0) yield break;

        // 대쉬 순간 관성 제거 후 툭 밀어주기
        body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
        body2d.AddForce(new Vector2(dir * attackDashForce, 0f), ForceMode2D.Impulse);

        // 대쉬 힘이 가해진 직후 1프레임 대기 (위치 좌표 물리 반영)
        yield return new WaitForFixedUpdate();

        // 이동이 반영된 '최신 위치'에서 히트박스 체크!
        CheckHitbox();
    }

    // ★ [수정] 히트박스 즉시 판정 메서드 (코루틴 제거로 더 간결해짐)
    private void CheckHitbox()
    {
        Vector3 originPos = attackPoint != null ? attackPoint.position : transform.position;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(originPos, attackHitboxSize, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[Hit!] {enemy.name} 적 타격 성공!");

            if (VFXHandler.Instance != null)
            {
                VFXHandler.Instance.PlayEffect("playerAttackHit", enemy.transform.position, 0.8f);
            }

            if (SoundHandler.Instance != null)
            {
                SoundHandler.Instance.PlayHitSoundAtPosition(enemy.transform.position);
            }
        }
    }

    private IEnumerator AttackFunction()
    {
        yield return new WaitForSeconds(0.2f);
    }
}