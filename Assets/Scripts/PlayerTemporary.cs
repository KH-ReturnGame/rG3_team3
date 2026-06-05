using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemporary : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    private Transform playerTransForm;

    private float movementSpeed = 10;
    private float jumpForce = 25f;
    public LayerMask Enemy;



    private float moveInput;

    public List<string> Stunlist = new List<string>();
    public List<string> Cooldownlist = new List<string>();

    public string PressingKey;

    private int Combo = 0;
    private float lastComboChangeTime;

    private bool isUptilt;
    private bool isDowntilt;


    public bool isDebounce = false;





    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public float look = 0f;
    public float nowchar = 1;
    //1번째 캐릭터 변수
    private Dictionary<Collider2D, float> enemyStackRegister = new Dictionary<Collider2D, float>();
    public float stack_char1 = 0; //적이 받는 스택
    public int stack_Q_char1 = 3;
    //2번째 캐릭터 스킬 변수
    public float punchup = 1;
    public int punchupstack = 0;
    public float defend = 0;
    public LayerMask whatIsGround;

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

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
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
            AddDataToList("LightCooldown", Cooldownlist, 0.7f, 0);
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
                    StartCoroutine(ExecuteAttack(new Vector2(0f, -2f), new Vector2(8f, 4f), 0.15f, 40f, true, 0f, 140f, 0));
                }
                else
                {
                    StartCoroutine(ExecuteAttack(new Vector2(0f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false, 0f, 0f, 0));
                }

                AddDataToList("LightCooldown", Cooldownlist, 0.26f, 0);
                StartCoroutine(ResetDebounce(0.26f));
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !Stunlist.Contains("Stun"))    //캐릭터 교체
        {
            nowchar = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !Stunlist.Contains("Stun"))
        {
            nowchar = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !Stunlist.Contains("Stun"))
        {
            nowchar = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && !Stunlist.Contains("Stun"))
        {
            nowchar = 4;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isDebounce)  //Q스킬 
        {
            if (nowchar == 1)   //찌르기 
            {
                if (stack_Q_char1 != 0)
                {
                    isDebounce = true;
                    stack_Q_char1 -= 1;
                    AddDataToList("SkillQCD_1", Cooldownlist, 7f, 1);
                    StartCoroutine(ResetDebounce(0.2f));
                    Debug.Log("Skill Q_1 Activated");
                    StartCoroutine(ExecuteAttack(new Vector2(1f, 0f), new Vector2(2f, 3f), 0.15f, 100f, false, 0f, 0f, 1f));
                    isDebounce = false;
                }
                else if (nowchar == 2) //돌진펀치
                {
                    if (!Cooldownlist.Contains("SkillQCD_2"))
                    {
                        isDebounce = true;
                        look = sprdr.flipX ? -1f : 1f;
                        Debug.Log("Skill Q Activated");
                        AddDataToList("SkillQCD_2", Cooldownlist, 3f, 0);
                        Debug.Log("Skill Q_2 Activated");
                        AddDataToList("SkillQCD_2", Cooldownlist, 5f, 0);
                        AddDataToList("Dash", Cooldownlist, 0.2f, 0);
                        rb2d.AddForce(new Vector2(look * 20f, 0f), ForceMode2D.Impulse);
                        StartCoroutine(ExecuteAttack(new Vector2(5f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false, 15f, 0f, 0));
                        rb2d.AddForce(new Vector2(look * 10f, 0f), ForceMode2D.Impulse);
                        StartCoroutine(ExecuteAttack(new Vector2(5f, 0f), new Vector2(10f, 2f), 0.1f, 30f, false, 15f, 0f, 0));
                        isDebounce = false;
                    }
                }
                else if (nowchar == 3) //(1타)찌르기 (2타)찌르며 대쉬
                {
                    if (!Cooldownlist.Contains("SkillQCD_3"))
                    {
                        isDebounce = true;
                        Debug.Log("Skill Q_3_1 Activated");
                        AddDataToList("SkillQCD_3", Cooldownlist, 8f, 0);
                        AddDataToList("SkillQCD_3_2", Cooldownlist, 3f, 0);
                        AddDataToList("SkillQ_2", Cooldownlist, 15f, 0);
                        StartCoroutine(ExecuteAttack(new Vector2(1.5f, 0f), new Vector2(3f, 1f), 0.1f, 30f, false, 0f, 0f, 0));
                        isDebounce = false;
                    }
                    else if (Cooldownlist.Contains("SkillQ_2") && !Cooldownlist.Contains("SkillQCD_3_2"))
                    {
                        isDebounce = true;
                        look = sprdr.flipX ? -1f : 1f;
                        Debug.Log("Skill Q_3_2 Activated");
                        AddDataToList("Dash", Cooldownlist, 0.15f, 0);
                        rb2d.AddForce(new Vector2(look * 7f, 0f), ForceMode2D.Impulse);
                        StartCoroutine(ExecuteAttack(new Vector2(3.4f, 0f), new Vector2(6.8f, 1f), 0.1f, 30f, false, 0f, 0f, 0));
                        isDebounce = false;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.W) && !isDebounce)   //W스킬
            {
                if (nowchar == 1)
                    if (nowchar == 1) //텔포
                    {
                        if (!Cooldownlist.Contains("SkillWCD_1"))
                        {
                            isDebounce = true;
                            Debug.Log("Skill W_1 Activated");
                            AddDataToList("SkillWCD_1", Cooldownlist, 5f, 0);
                            TeleportLogic(0.3f, Enemy);
                            isDebounce = false;
                        }
                    }
                    else if (nowchar == 2)  //날라가 착지
                    {
                        if (!Cooldownlist.Contains("SkillWCD_2"))
                        {
                            isDebounce = true;
                            Debug.Log("Skill W_2 Activated");
                            AddDataToList("SkillWCD_2", Cooldownlist, 5f, 0);
                            Char_2_W(0.3f);
                        }
                    }
                    else if (nowchar == 3)  //(1타) 아래로 내려 찍기. (2타) 위로 올리기 (3타)찌르기
                    {
                        if (!Cooldownlist.Contains("SkillWCD_3"))
                        {
                            isDebounce = true;
                            Debug.Log("Skill W_3 Activated");
                            AddDataToList("SkillWCD_3", Cooldownlist, 6f, 0);
                            StartCoroutine(Char_3_W());
                            isDebounce = false;
                        }
                    }
            }

            if (Input.GetKeyDown(KeyCode.E) && !isDebounce)  //E스킬
            {
                if (nowchar == 1)
                {

                }
                else if (nowchar == 2)  //위로 펀치 올리기
                {
                    if (!Cooldownlist.Contains("SkillECD_2"))
                    {
                        isDebounce = true;
                        Debug.Log("Skill E_2 Activated");
                        AddDataToList("SkillECD_2", Cooldownlist, 6f, 0);
                        StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2f, 3f), 0.1f, 30f, false, 0f, 20f, 0));
                        isDebounce = false;
                    }
                }
                else if (nowchar == 3)
                {
                    if (!Cooldownlist.Contains("SkillECD_3"))
                    {
                        isDebounce = true;
                        Debug.Log("Skill E_3 Activated");
                        AddDataToList("SkillECD_3", Cooldownlist, 6f, 0);
                        StartCoroutine(Char_3_E(0.3f)); 
                        isDebounce = false;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R) && !isDebounce)  //R스킬
            {

            }
        }
    }

    void FixedUpdate()
    {
        if (!Stunlist.Contains("Stun") && !Cooldownlist.Contains("Dash"))
            rb2d.linearVelocity = new Vector2(moveInput * movementSpeed, rb2d.linearVelocity.y);
    }

    private IEnumerator ResetDebounce(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDebounce = false;
    }

    private void AddDataToList(string dataName, List<string> dataList, float duration, float logic)
    {
        StartCoroutine(Activate());
        IEnumerator Activate()
        {
            dataList.Add(dataName);
            yield return new WaitForSeconds(duration);
            dataList.Remove(dataName);
            if (logic == 1)
            {
                stack_Q_char1 += 1;
            }
        }
    }
    private IEnumerator Dashdown(float duration)
    {
        {
            yield return new WaitForSeconds(duration);
            rb2d.AddForce(new Vector2(0f, -20f), ForceMode2D.Impulse);
        }
    }

    private IEnumerator Defend_2(float Defend, float duration)
    {
        {
            defend = Defend;
            yield return new WaitForSeconds(duration);
            defend = 0;
        }
    }
    private IEnumerator Bezier_Curves(float base_x, float base_y, float finish_x, float finish_y, float duration)
    {
        float t = 0f;
        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogError("Player를 찾을 수 없습니다!");
            yield break;
        }

        Vector2 player_position = player.transform.position;
        playerTransForm = player.transform;

        float start_x = player_position.x;
        float start_y = player_position.y;

        while (t <= duration)
        {
            t += Time.deltaTime;
            float devide = t / duration;

            if (devide > 1f) devide = 1f;

            float main_1_x = start_x + (base_x - start_x) * devide;
            float main_1_y = start_y + (base_y - start_y) * devide;

            float main_2_x = base_x + (finish_x - base_x) * devide;
            float main_2_y = base_y + (finish_y - base_y) * devide;

            float moving_x = (1 - devide) * (main_1_x) + devide * (main_2_x);
            float moving_y = (1 - devide) * (main_1_y) + devide * (main_2_y);

            playerTransForm.position = new Vector2(moving_x, moving_y);
            yield return null;
        }
        playerTransForm.position = new Vector2(finish_x, finish_y);
        StartCoroutine(ExecuteAttack(new Vector2(0f, -1f), new Vector2(5f, 1f), 0.1f, 30f, false, 0f, 7f, 0));
        isDebounce = false;
    }
    private IEnumerator ExecuteAttack(Vector2 offset, Vector2 size, float duration, float damage, bool isDownSmash, float Force_x, float Force_y, float logic)
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
                            enemyRb.AddForce(Vector2.down * Force_y, ForceMode2D.Impulse);
                            Debug.Log("DOWN SMASH!");
                        }
                        else if (isUptilt)
                        {
                            enemyRb.linearVelocity = Vector2.zero;
                            enemyRb.AddForce(Vector2.up * Force_y, ForceMode2D.Impulse);
                        }
                        else
                        {
                            enemyRb.AddForce(new Vector2(dir * Force_x, Force_y), ForceMode2D.Impulse);
                        }
                        if (logic == 1f)
                        {
                            stack_char1 += 1;
                        }
                    }

                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator Char_3_E(float radius)
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 finalMousePos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        int realEnemyLayerMask = LayerMask.GetMask("Enemy");
        Collider2D hitenemy = Physics2D.OverlapCircle(finalMousePos, radius, realEnemyLayerMask);
        if (hitenemy != null)
        {
            rb2d.AddForce(new Vector2(0f, 15f), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.1f);
            transform.position = hitenemy.transform.position + Vector3.up * 100f;
            rb2d.AddForce(new Vector2(0f, -30f), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ExecuteAttack(new Vector2(0f, -1f), new Vector2(2f, 5f), 0.1f, 30f, false, 0f, 7f, 0));
        }
    }
    public IEnumerator Char_3_W()
    {
        StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2.3f, 3f), 0.1f, 30f, false, 0f, 0f, 0));
        Debug.Log("1");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("2");
        StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2.3f, 3f), 0.1f, 30f, false, 0f, 0f, 0));
        yield return new WaitForSeconds(0.6f);
        Debug.Log("3");
        StartCoroutine(ExecuteAttack(new Vector2(1.5f, 0f), new Vector2(3f, 1f), 0.1f, 50f, false, 0f, 0f, 0));
    }

    private void Char_2_W(float radius)
    {

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 finalMousePos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        float x = finalMousePos.x;
        float y = finalMousePos.y;

        int realEnemyLayerMask = LayerMask.GetMask("Enemy");
        Collider2D hitenemy = Physics2D.OverlapCircle(finalMousePos, radius, realEnemyLayerMask);

        look = sprdr.flipX ? -1f : 1f;

        if (hitenemy != null)
        {

            StartCoroutine(Bezier_Curves(x - 1f * look, y + 10f, x, y, 0.7f));
            StartCoroutine(Bezier_Curves(x - 1f * look, y + 7f, x, y, 0.6f));

        }
        else
        {
            isDebounce = false;
            Cooldownlist.Remove("SkillWCD_2");
        }

    }

    private void TeleportLogic(float radius, LayerMask Layer)
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 finalMousePos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);


        int realEnemyLayerMask = LayerMask.GetMask("Enemy");

        Collider2D hitenemy = Physics2D.OverlapCircle(finalMousePos, radius, realEnemyLayerMask);

        if (hitenemy != null)
        {
            if (stack_char1 >= 1)
            {
                transform.position = hitenemy.transform.position;
                Cooldownlist.Remove("SkillWCD_1");
                stack_char1 -= 1;
            }
            else
            {
                transform.position = hitenemy.transform.position;
            }
        }
        else
        {
            Cooldownlist.Remove("SkillWCD_1");
        }
    }

    private void OnDrawGizmos()
    {
        if (sprdr == null) return;
        Gizmos.color = Color.red;
        float dir = sprdr.flipX ? -1f : 1f;
        Gizmos.DrawWireCube(transform.position + new Vector3(1f * dir, 0f, 0), new Vector3(2f, 3f, 1f));
        Gizmos.DrawWireCube(transform.position + new Vector3(0f * dir, 0f, 0), new Vector3(5f, 1f, 1f));
    }
}
    