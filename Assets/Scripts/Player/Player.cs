using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private float attackDashForce = 15f; // 평타 시 앞으로 전진하는 힘의 크기 (조절 가능)

    // combo 
    public int currentComboStack = 0;
    private float lastComboChangeTime;
    private float comboStackResetLimit = 1f;
    private int maxComboStack = 5;

    private float attackCooldown = 0.45f;

    [Header("상태리스트")]
    public List<string> Stunlist = new List<string>();
    public List<string> FrameList = new List<string>();
    public List<string> DebounceList = new List<string>();

    [Header("내부상태리스트")]
    public List<string> Cooldownlist = new List<string>();

    void Awake()
    {
        spriteRenderOBJ = transform.Find("_SpriteRenderer");

        body2d = GetComponent<Rigidbody2D>();
        // IN SPRITE RENDERER
        spdr = spriteRenderOBJ.GetComponent<SpriteRenderer>();
        animtr = spriteRenderOBJ.GetComponent<Animator>();
    }

    void Update()
    {
        // 공격 중(BasicAttackDash 상태)일 때는 방향 키 입력을 받지 않음 (좌우 뒤집힘 및 입력 방지)
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

        // ground Relavant
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, GroundLayer);

        // combo
        animtr.SetInteger("Combo", currentComboStack);

        // character_updater
        setSprite();

        // keyDetection
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
        // ★ 핵심: 공격 중(BasicAttackDash 상태)이 아닐 때만 키보드 입력으로 좌우 이동이 가능합니다.
        if (!DebounceList.Contains("BasicAttackDash"))
        {
            body2d.linearVelocity = new Vector2(xInput * moveSpeed, body2d.linearVelocity.y);
        }

        animtr.SetFloat("yVelocity", body2d.linearVelocityY);

        directionInspection();
    }

    void OnDrawGizmos()
    {
    }

    // keyInputsHandler
    private void keyInputdetection()
    {
        if (Input.GetButtonDown("Jump")) // Jump 
        {
            if (isGrounded == true)
            {
                StartCoroutine(Jump());
            }
        }
        if (Input.GetKeyDown(KeyCode.Q)) // Dash
        {
            StartCoroutine(Dash());
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.L))
        {
            BasicAttack();
        }
    }

    // directionInspection
    private void directionInspection()
    {
        // Y 회전값이 0에 가까우면 오른쪽 바라보는 상태, 아니면 왼쪽 바라보는 상태
        if (Mathf.Abs(transform.localEulerAngles.y) > 0.1f)
            isfacingRight = false;
        else
            isfacingRight = true;
    }

    // spriteManipulator
    public void setSprite()
    {
        // 공격 모션 발동 중에는 방향이 뒤집히지 않도록 방지
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
        animtr.SetTrigger("isJumpTrigger");
        yield return new WaitForSeconds(.1f);
        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpHeight);
    }

    private IEnumerator Dash()
    {
        // 바라보는 방향 결정
        float dir = spdr.flipX ? -1f : 1f;

        animtr.SetTrigger("isDash");
        yield return new WaitForSeconds(0.2f);

        // 대쉬 전 순간적으로 속도를 0으로 초기화 (더 깔끔한 대쉬를 위해)
        body2d.linearVelocity = new Vector2(0, body2d.linearVelocity.y);

        // 대쉬 힘 가하기
        body2d.AddForce(new Vector2(dir * 80f, 0), ForceMode2D.Impulse);
    }

    private void BasicAttack()
    {
        if (Stunlist.Count > 0) return;

        if (!Cooldownlist.Contains("Attack"))
        {
            // 1. 공격을 누른 즉시 콤보 스택 증가
            currentComboStack++;

            // 최대 콤보(4타)를 넘어가면 다시 1타로 순환 구문
            if (currentComboStack > 4)
            {
                Utility.DataManagement.ListManagement.AddData("Attack", Cooldownlist, attackCooldown * 1.5f);
                currentComboStack = 0;
            }

            lastComboChangeTime = Time.time;
            comboStackResetLimit = 1.5f;
            Debug.Log("Combo : " + currentComboStack);

            // 애니메이터의 Combo 값 즉시 주입
            animtr.SetInteger("Combo", currentComboStack);

            // 공격 트리거 발동
            animtr.SetTrigger("isAttackTrigger");

            // ★ 공격 중 상태 등록 (0.5초간 움직임/뒤집기 제한)
            Utility.DataManagement.ListManagement.AddData("BasicAttackDash", DebounceList, .5f);

            // 쿨다운 등록
            Utility.DataManagement.ListManagement.AddData("Attack", Cooldownlist, attackCooldown);

            // ★ 바라보고 있는 방향으로 평타 돌진(대쉬) 적용
            StartCoroutine(PerformAttackDash());
        }
    }

    // ★ 평타 전진 물리 연출 코루틴
    private IEnumerator PerformAttackDash()
    {
        // 바라보고 있는 방향 판단 (isfacingRight 사용)
        float dir = isfacingRight ? 1f : -1f;

        // 순간적으로 수평 속도를 멈춰 물리 관성 충돌 방지
        body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);

        // 보는 방향으로 짧고 정교하게 툭 쳐주는 전진 힘 가하기
        body2d.AddForce(new Vector2(dir * attackDashForce, 0f), ForceMode2D.Impulse);

        yield return null;
    }

    private IEnumerator AttackFunction()
    {
        yield return new WaitForSeconds(0.2f);
    }
}