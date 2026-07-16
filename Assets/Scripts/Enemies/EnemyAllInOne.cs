using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Idle, Chase, Attack }
    [Header("FSM 상태")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;
    private Transform playerTransform;

    [Header("기본 스탯")]
    public float movementSpeed = 6f;
    public float detectionRange = 15f;
    public float attackRange = 3.5f;

    [Header("공격 제어 및 콤보")]
    public List<string> StunList = new List<string>();
    public List<string> CooldownList = new List<string>();
    private bool isDebounce = false;
    private int aiCombo = 0;

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
        
        var player = FindObjectOfType<PlayerTemporary>();
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // 💡 셔터 내리기: 스턴 상태면 대뇌 연산을 올스톱하고 탈출시킵니다.
        if (StunList.Contains("Stun"))
        {
            currentState = EnemyState.Idle; 
            aiCombo = 0;                    
            isDebounce = false;              
            return; 
        }

        if (playerTransform == null) return;

        // 거리 비례 상태 머신 전환
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
        }

        // 공격 패턴 트리거 실행
        if (currentState == EnemyState.Attack && !isDebounce)
        {
            DecideAttackPattern();
        }
    }

    void FixedUpdate()
    {
        // 💡 물리 연산 완전 정지: 스턴 상태일 땐 제자리에 고정하고 중력만 흐르게 합니다.
        if (StunList.Contains("Stun"))
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
            if (animtr != null) animtr.SetBool("isWalking", false);
            return;
        }

        if (currentState == EnemyState.Attack || playerTransform == null) return;

        if (currentState == EnemyState.Chase)
        {
            float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
            rb2d.linearVelocity = new Vector2(direction * movementSpeed, rb2d.linearVelocity.y);
            sprdr.flipX = direction < 0;
            if (animtr != null) animtr.SetBool("isWalking", true);
        }
        else if (currentState == EnemyState.Idle)
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
            if (animtr != null) animtr.SetBool("isWalking", false);
        }
    }

    private void DecideAttackPattern()
    {
        if (CooldownList.Contains("GlobalAttackCD")) return;

        isDebounce = true;
        float dir = sprdr.flipX ? -1f : 1f;

        // 패턴 1: 유저가 공중에 떠 있을 때 어퍼컷으로 요격
        if (playerTransform.position.y - transform.position.y > 2f)
        {
            StartCoroutine(ExecuteEnemyAttack(new Vector2(1f, 2f), new Vector2(5f, 7f), 0.15f, "UpperCut"));
        }
        // 패턴 2: AI 자신이 공중에 떴고 아래 유저가 보일 때 메테오 수직강하 내려찍기
        else if (!isGrounded && playerTransform.position.y < transform.position.y)
        {
            StartCoroutine(ExecuteEnemyAttack(new Vector2(0f, -2f), new Vector2(8f, 4f), 0.2f, "DownSmash"));
        }
        // 패턴 3: 정면 평지 조우 시 3연타 기본 콤보 몰아치기
        else
        {
            aiCombo++;
            if (aiCombo >= 3)
            {
                StartCoroutine(ExecuteEnemyAttack(new Vector2(2.5f, 0.5f), new Vector2(10f, 6f), 0.2f, "HeavyFinish"));
                aiCombo = 0;
                AddDataToList("GlobalAttackCD", CooldownList, 1.5f); // 막타 적중 후 큰 후딜레이 발생
            }
            else
            {
                StartCoroutine(ExecuteEnemyAttack(new Vector2(2f, 0.5f), new Vector2(9f, 6f), 0.1f, "LightAttack"));
            }
        }
    }

    private IEnumerator ExecuteEnemyAttack(Vector2 offset, Vector2 size, float duration, string attackType)
    {
        float dir = sprdr.flipX ? -1f : 1f;
        Debug.Log($"AI Attack: {attackType}");

        // 기동력 대입
        if (attackType == "UpperCut")
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 18f);
        }
        else if (attackType == "DownSmash")
        {
            rb2d.linearVelocity = new Vector2(0, -35f);
        }
        else
        {
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            rb2d.AddForce(new Vector2(dir * 25f, 0f), ForceMode2D.Impulse);
        }

        float elapsed = 0f;
        HashSet<Collider2D> hitHistory = new HashSet<Collider2D>();

        while (elapsed < duration)
        {
            // 공격 프레임 실행 도중 유저한테 얻어맞아 스턴 상태로 전환되면 공격 루프 폭파
            if (StunList.Contains("Stun")) yield break; 

            float currentDir = sprdr.flipX ? -1f : 1f;
            Vector2 pos = (Vector2)transform.position + new Vector2(offset.x * currentDir, offset.y);

            Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0f, LayerMask.GetMask("Player"));

            foreach (var hit in hits)
            {
                if (!hitHistory.Contains(hit))
                {
                    hitHistory.Add(hit);

                    if (hit.TryGetComponent(out Rigidbody2D playerRb))
                    {
                        playerRb.linearVelocity = Vector2.zero; 

                        if (attackType == "UpperCut")
                        {
                            playerRb.AddForce(Vector2.up * 28f, ForceMode2D.Impulse);
                        }
                        else if (attackType == "DownSmash")
                        {
                            playerRb.AddForce(Vector2.down * 120f, ForceMode2D.Impulse);
                        }
                        else if (attackType == "HeavyFinish")
                        {
                            playerRb.AddForce(new Vector2(dir * 45f, 10f), ForceMode2D.Impulse);
                        }
                        else
                        {
                            playerRb.AddForce(new Vector2(dir * 12f, 0f), ForceMode2D.Impulse);
                        }

                        // 💡 유저 강제 경직(Stun) 부여
                        if (hit.TryGetComponent(out PlayerTemporary playerScript))
                        {
                            playerScript.StartCoroutine(InstantStun(playerScript));
                        }
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f); // 공격 후 빈틈 딜레이
        isDebounce = false;
    }

    private IEnumerator InstantStun(PlayerTemporary player)
    {
        player.Stunlist.Add("Stun");
        yield return new WaitForSeconds(0.4f); 
        player.Stunlist.Remove("Stun");
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