using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateBossAI : MonoBehaviour
{
    private enum BossState { Idle, Chase, Attack, Dead }

    [Header("FSM 상태")]
    [SerializeField] private BossState currentState = BossState.Idle;

    [Header("보스 스탯")]
    public float maxHP = 1000f; // [cite: 1]
    public float currentHP;
    public float movementSpeed = 6f;
    public float detectionRange = 15f;
    public float attackRange = 3.5f;

    [Header("컴포넌트 및 타겟")]
    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;
    private Transform playerTransform;

    [Header("공격 제어 및 쿨타임")]
    public List<string> StunList = new List<string>();
    public List<string> CooldownList = new List<string>();
    private bool isActing = false; // 행동 중첩 방지 (기존 Boss_Acting 역할) [cite: 1]

    [Header("프리팹 세팅 (Inspector에서 할당)")]
    public GameObject warningDownPrefab;   // 경고용 기둥 [cite: 55]
    public GameObject realDownPrefab;      // 실제 데미지용 기둥 [cite: 75]
    public GameObject warningCirclePrefab; // 경고용 원형 장판 [cite: 57]
    public GameObject realCirclePrefab;    // 실제 데미지 원형 장판 [cite: 76]

    [Header("땅 밟음 체크")]
    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();

        // 플레이어 찾기 (이름이나 태그 기반으로 수정 가능)
        GameObject player = GameObject.Find("_PlayerTransform"); // [cite: 62]
        if (player != null) playerTransform = player.transform;

        currentHP = maxHP;
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // 스턴 상태면 대뇌 연산 올스톱
        if (StunList.Contains("Stun"))
        {
            currentState = BossState.Idle;
            isActing = false;
            return;
        }

        if (playerTransform == null) return;

        // 행동 중(isActing)이 아닐 때만 거리 비례 상태 전환
        if (!isActing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
                currentState = BossState.Attack;
            else if (distanceToPlayer <= detectionRange)
                currentState = BossState.Chase;
            else
                currentState = BossState.Idle;
        }

        // 공격 범위에 들어왔고, 공격 쿨타임이 돌고 있지 않다면 패턴 실행
        if (currentState == BossState.Attack && !isActing)
        {
            DecideAttackPattern();
        }
    }

    void FixedUpdate()
    {
        if (currentState == BossState.Dead) return;

        // 스턴 상태 물리 연산 완전 정지
        if (StunList.Contains("Stun"))
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
            if (animtr != null) animtr.SetBool("run", false);
            return;
        }

        // 공격 중이거나 플레이어가 없으면 이동 정지
        if (currentState == BossState.Attack || playerTransform == null || isActing) return;

        if (currentState == BossState.Chase)
        {
            float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
            rb2d.linearVelocity = new Vector2(direction * movementSpeed, rb2d.linearVelocity.y);
            sprdr.flipX = direction < 0;
            if (animtr != null) animtr.SetBool("run", true);
        }
        else if (currentState == BossState.Idle)
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
            if (animtr != null) animtr.SetBool("run", false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == BossState.Dead) return;

        currentHP -= damage;
        Debug.Log("보스 피격! 남은 체력: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentState = BossState.Dead;
        isActing = true;
        rb2d.linearVelocity = Vector2.zero;
        Debug.Log("보스 사망"); // [cite: 7]
        // 사망 애니메이션 Trigger 호출 등 추가 가능
    }

    // ===================================================================
    // 최강 보스 패턴 결정 로직 (스파게티 코드를 FSM에 맞게 압축)
    // ===================================================================
    private void DecideAttackPattern()
    {
        if (CooldownList.Contains("GlobalAttackCD")) return;

        isActing = true;
        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
        sprdr.flipX = playerTransform.position.x < transform.position.x;

        // 패턴 랜덤 돌리기 (예시로 근접 돌진과 마법 장판 2가지를 융합)
        int randomPattern = Random.Range(1, 3);

        if (randomPattern == 1)
        {
            StartCoroutine(MeleeRushPattern());
        }
        else
        {
            StartCoroutine(MagicAreaPattern());
        }
    }

    // 1번 패턴: 무식하게 플레이어에게 돌진하는 근접 공격 (보스 4 기반)
    private IEnumerator MeleeRushPattern()
    {
        if (animtr != null) animtr.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f); // 선딜레이

        if (!StunList.Contains("Stun"))
        {
            float dir = sprdr.flipX ? -1f : 1f;
            rb2d.AddForce(new Vector2(dir * 25f, 0f), ForceMode2D.Impulse); // 돌진

            // 돌진 타격 판정
            Vector2 attackPos = (Vector2)transform.position + new Vector2(dir * 2f, 0.5f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, new Vector2(3f, 2f), 0f, LayerMask.GetMask("Player"));

            foreach (var hit in hits)
            {
                // 플레이어가 맞았을 때 데미지 처리
                Debug.Log("돌진 공격 적중!");
            }
        }

        yield return new WaitForSeconds(0.6f); // 후딜레이

        AddDataToList("GlobalAttackCD", CooldownList, 1.5f);
        isActing = false;
    }

    // 2번 패턴: 플레이어 발밑에 경고 장판 생성 후 폭발 (보스 1, 3 마법 기반)
    private IEnumerator MagicAreaPattern()
    {
        Vector2 targetPos = playerTransform.position;

        // 1. 경고 프리팹 생성 (스파게티의 attack_down, attack_circle 역할) [cite: 55, 57]
        GameObject warningObj = null;
        if (warningCirclePrefab != null)
        {
            warningObj = Instantiate(warningCirclePrefab, targetPos, Quaternion.identity);
            warningObj.transform.localScale = new Vector2(3f, 3f);
        }

        
        yield return new WaitForSeconds(1.5f);

        
        if (warningObj != null) Destroy(warningObj);

        if (!StunList.Contains("Stun") && realCirclePrefab != null)
        {
            GameObject realObj = Instantiate(realCirclePrefab, targetPos, Quaternion.identity);
            realObj.transform.localScale = new Vector2(3f, 3f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 3f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") || hit.name == "Player")
                {
                    Debug.Log("마법 공격 적중! 플레이어 데미지 처리");
                    // hit.GetComponent<PlayerScript>().TakeDamage(데미지);
                }
            }

            Destroy(realObj, 0.5f);
        }

        AddDataToList("GlobalAttackCD", CooldownList, 2.0f);
        isActing = false;
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

    private void OnDrawGizmos()
    {
        if (sprdr == null) return;
        Gizmos.color = Color.yellow;
        float dir = sprdr.flipX ? -1f : 1f;
        Gizmos.DrawWireCube(transform.position + new Vector3(attackRange * dir, 0.5f, 0), new Vector3(attackRange * 2, 2f, 1f));
    }
}