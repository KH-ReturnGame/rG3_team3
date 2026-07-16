using UnityEngine;
using System.Collections;

public class FrenchMan : MonoBehaviour
{
    [Header("이동 및 대시")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 7f;

    [Header("패링 기본 설정")]
    [SerializeField] private float parryCooldown = 5f;
    [SerializeField] private float parryActiveDuration = 0.2f;
    [SerializeField] private Vector2 parryBoxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 parryOffset = new Vector2(1f, 0f);
    [SerializeField] private LayerMask targetLayer; // 투사체 레이어를 지정하세요.

    [Header("패링 성공 시 투사체 물리 설정")]
    [SerializeField] private float projectileBounceSpeed = 15f; // 튕겨 나가는 속도
    [SerializeField] private float minSpinSpeed = 360f;          // 최소 회전 속도 (도/초)
    [SerializeField] private float maxSpinSpeed = 720f;          // 최대 회전 속도 (도/초)

    public Rigidbody2D rb;

    private ParticleSystem ps;
    private float horizontalInput;

    private bool isDashing = false;
    private bool canDash = true;
    private float lastDashDirection = 1f;

    private bool canParry = true;

    private float particlesstartpos;

    public Animator SWINGanimtr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            lastDashDirection = Mathf.Sign(horizontalInput);
            FlipCharacter(lastDashDirection);
        }

        if (Input.GetKeyDown(KeyCode.Q) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.E) && canParry)
        {
            StartCoroutine(Parry());
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void FlipCharacter(float direction)
    {
        if (direction < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (direction > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(lastDashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator Parry()
    {
        canParry = false;
        SWINGanimtr.SetTrigger("Parry");

        float timer = 0f;
        while (timer < parryActiveDuration)
        {
            Vector2 detectionPosition = transform.TransformPoint(parryOffset);
            Collider2D[] hits = Physics2D.OverlapBoxAll(detectionPosition, parryBoxSize, 0f, targetLayer);

            if (hits.Length > 0)
            {
                foreach (Collider2D hit in hits)
                {
                    // 1. 맞춘 대상(투사체)의 Rigidbody2D 컴포넌트 가져오기
                    Rigidbody2D projectileRb = hit.GetComponent<Rigidbody2D>();

                    if (projectileRb != null)
                    {
                        // 2. 360도 전 방향 랜덤 방향 벡터 생성
                        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

                        // 3. 투사체의 속도 재설정 (전 방향 튕기기)
                        projectileRb.linearVelocity = randomDirection * projectileBounceSpeed;
                        // ※ 구버전 유니티(2022.3 미만) 사용 중이라면 아래 코드의 주석을 풀고 위 코드를 지우세요.
                        // projectileRb.velocity = randomDirection * projectileBounceSpeed;

                        // 4. 무작위 회전 방향 및 속도 적용
                        float spinDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
                        float randomSpin = Random.Range(minSpinSpeed, maxSpinSpeed) * spinDirection;
                        projectileRb.angularVelocity = randomSpin;
                    }
                }
                break; // 한 번 패링 성공하면 루프 종료
            }

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector2 detectionPosition = transform.TransformPoint(parryOffset);
        Gizmos.DrawWireCube(detectionPosition, parryBoxSize);
    }
}