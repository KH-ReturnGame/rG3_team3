using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[System.Serializable]
public class AttackPrefab
{
    public GameObject attack_down;
    public GameObject attack_up;
    public GameObject real_attack_down;
    public GameObject real_attack_up;
    public GameObject attack_cricle;
    public GameObject real_attack_circle;
}
public class PlayerTemporary : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;
    private GameObject attack;

    private Transform playerTransForm;

    private float movementSpeed = 10;
    private float jumpForce = 25f;
    public LayerMask Enemy;

    public AttackPrefab prefab = new AttackPrefab();


    private float moveInput;

    public List<string> Stunlist = new List<string>();
    public List<string> Cooldownlist = new List<string>();

    public string PressingKey;

    private int Combo = 0;
    private float lastComboChangeTime;

    private bool isUptilt;
    private bool isDowntilt;


    public bool isDebounce = false;

    //보스 관련
    public float Boss_HP = 1000f;
    public bool Boss_Acting = false;
    public float Boss_Who = 0;
    public float Boss_Pattern = 0;
    public bool Boss_2_1_Acting = false;
    public bool Boss_3_4_Iced = false;
    public int Boss_3_4_Iced_Stack = 0;
    public bool Boss_3_4_Acting = false;


    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public float look = 0f;
    public float nowchar = 1;
    public float strength = 100f;
    //1번째 캐릭터 변수
    private Dictionary<Collider2D, float> enemyStackRegister = new Dictionary<Collider2D, float>();
    public float stack_char1 = 0; //적이 받는 스택
    public int stack_Q_char1 = 3;
    public bool R_Activated = false;
    //2번째 캐릭터 스킬 변수
    public float punchup = 1;
    //3번째 캐릭터 스킬 변수
    public float stack_char3 = 0;
    public float stack_char3_2 = 0;

    public int punchupstack = 0;
    public float defend = 0;
    public LayerMask whatIsGround;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
        Boss_Who = Random.Range(1, 4);
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

        //보스코드
        if (Boss_HP <= 0)
        {
            Debug.Log("보스 사망");
        }
        else if (Boss_Acting == false && Boss_HP > 0)
        {
            Boss_Acting = true;
            if (Boss_Who == 1)  //보스1 패턴, 마법사
            {
                Boss_Pattern = Random.Range(1, 5);
                if (Boss_Pattern == 1)
                {
                    StartCoroutine(Boss_Pattern_1_1());
                }
                else if (Boss_Pattern == 2)
                {
                    StartCoroutine(Boss_Pattern_1_2());
                }
                else if (Boss_Pattern == 3)
                {
                    StartCoroutine(Boss_Pattern_1_3());
                }
                else if (Boss_Pattern == 4)
                {
                    StartCoroutine(Boss_Pattern_1_4());
                }
            }
            else if (Boss_Who == 2) //보스2 패턴, 검사
            {
                if (Boss_2_1_Acting == false)
                {
                    Boss_Pattern = Random.Range(1, 6);
                    if (Boss_Pattern == 1)
                    {
                        Boss_2_1_Acting = true;
                    }
                }
                else
                {
                    Boss_Pattern = Random.Range(2, 6);
                }

                if (Boss_Pattern == 1)
                {
                    StartCoroutine(Boss_Pattern_2_1());
                }
                else if (Boss_Pattern == 2)
                {
                    StartCoroutine(Boss_Pattern_2_2());
                }
                else if (Boss_Pattern == 3)
                {
                    StartCoroutine(Boss_Pattern_2_3());
                }
                else if (Boss_Pattern == 4)
                {
                    StartCoroutine(Boss_Pattern_2_4());
                }
                else if (Boss_Pattern == 5)
                {
                    StartCoroutine(Boss_Pattern_2_5());
                }
            }
            else if (Boss_Who == 3) //보스 3 패턴, 망치
            {
                if(Boss_3_4_Acting == false)
                {
                    Boss_Pattern = Random.Range(4, 5);
                }
                else
                {
                    Boss_Pattern = Random.Range(1, 4);
                }
                if(Boss_Pattern == 1)
                {
                    StartCoroutine(Boss_Pattern_3_1());
                }
                else if(Boss_Pattern == 2)
                {
                    StartCoroutine(Boss_Pattern_3_2());
                }
                else if(Boss_Pattern == 3)
                {
                    StartCoroutine(Boss_Pattern_3_3());
                }
                else if(Boss_Pattern == 4 && Boss_3_4_Acting == false)
                {
                    Boss_3_4_Acting = true;
                    Boss_3_4_Iced = true;
                    Boss_3_4_Iced_Stack = 0;
                    StartCoroutine(Boss_Pattern_3_4_1());
                    StartCoroutine(Boss_Pattern_3_4_2());
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
            {
                Boss_3_4_Iced_Stack += 1;
            }
                if(Boss_3_4_Iced_Stack == 20)
            {
                Boss_3_4_Iced = false;
                Boss_3_4_Acting = false;
                Boss_3_4_Iced_Stack = 0;
            }

        if(Boss_Pattern == 3 && Boss_Who == 3) //3번보스 3번째 스킬 끌기
        {
            GameObject player = GameObject.Find("Player");
            float player_x = player.transform.position.x;
            if(player_x > 0)
            {
                player.transform.position = new Vector2(player_x - 0.025f, player.transform.position.y);
            }
            else if(player_x < 0)
            {
                player.transform.position = new Vector2(player_x + 0.025f, player.transform.position.y);
            }
        }
        //캐릭터코드
        if (Input.GetKeyDown(KeyCode.Alpha1) && !Stunlist.Contains("Stun"))    //캐릭터 교체
        {
            Debug.Log("1변경");
            stack_char3_2 = 0;
            nowchar = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !Stunlist.Contains("Stun"))
        {
            Debug.Log("2변경");
            stack_char3_2 = 0;
            nowchar = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !Stunlist.Contains("Stun"))
        {
            Debug.Log("3변경");
            stack_char3_2 = 0;
            nowchar = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && !Stunlist.Contains("Stun"))
        {
            Debug.Log("4변경");
            stack_char3_2 = 0;
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
                    AddDataToList("SkillQCD_1", Cooldownlist, 12f, 1);
                    StartCoroutine(ResetDebounce(0.2f));
                    Debug.Log("Skill Q_1 Activated");
                    StartCoroutine(ExecuteAttack(new Vector2(1f, 0f), new Vector2(2f, 3f), 0.15f, strength / 5, false, 0f, 0f, 1f));
                    isDebounce = false;
                }
            }
            else if (nowchar == 2) //돌진펀치
            {
                if (!Cooldownlist.Contains("SkillQCD_2"))
                {
                    isDebounce = true;
                    look = sprdr.flipX ? -1f : 1f;
                    AddDataToList("SkillQCD_2", Cooldownlist, 10f, 0);
                    Debug.Log("Skill Q_2 Activated");
                    AddDataToList("Dash", Cooldownlist, 0.2f, 0);
                    rb2d.AddForce(new Vector2(look * 20f, 0f), ForceMode2D.Impulse);
                    StartCoroutine(ExecuteAttack(new Vector2(5f, 0.5f), new Vector2(9f, 6f), 0.1f, strength / 5, false, 15f, 0f, 0));
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
                    StartCoroutine(ExecuteAttack(new Vector2(1.5f, 0f), new Vector2(3f, 1f), 0.1f, strength / 10, false, 0f, 0f, 0));
                    isDebounce = false;
                }
                else if (Cooldownlist.Contains("SkillQ_2") && !Cooldownlist.Contains("SkillQCD_3_2"))
                {
                    isDebounce = true;
                    look = sprdr.flipX ? -1f : 1f;
                    Debug.Log("Skill Q_3_2 Activated");
                    AddDataToList("Dash", Cooldownlist, 0.15f, 0);
                    rb2d.AddForce(new Vector2(look * 7f, 0f), ForceMode2D.Impulse);
                    StartCoroutine(ExecuteAttack(new Vector2(3.4f, 0f), new Vector2(6.8f, 1f), 0.1f, strength / 5, false, 0f, 0f, 0));
                    isDebounce = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.W) && !isDebounce)   //W스킬
        {

            if (nowchar == 1) //텔포
            {
                if (!Cooldownlist.Contains("SkillWCD_1"))
                {
                    isDebounce = true;
                    Debug.Log("Skill W_1 Activated");
                    AddDataToList("SkillWCD_1", Cooldownlist, 10f, 0);
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
                    AddDataToList("SkillWCD_2", Cooldownlist, 7f, 0);
                    Char_2_W(0.3f);
                }
            }
            else if (nowchar == 3)  //(1타) 아래로 내려 찍기. (2타) 위로 올리기 (3타)찌르기
            {
                if (!Cooldownlist.Contains("SkillWCD_3"))
                {
                    isDebounce = true;
                    Debug.Log("Skill W_3 Activated");
                    AddDataToList("SkillWCD_3", Cooldownlist, 12f, 0);
                    StartCoroutine(Char_3_W());
                    isDebounce = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && !isDebounce)  //E스킬
        {
            if (nowchar == 1)
            {
                //본 pc에 만들었음 
            }
            else if (nowchar == 2)  //위로 펀치 올리기
            {
                if (!Cooldownlist.Contains("SkillECD_2"))
                {
                    isDebounce = true;
                    Debug.Log("Skill E_2 Activated");
                    AddDataToList("SkillECD_2", Cooldownlist, 6f, 0);
                    StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2f, 3f), 0.1f, strength / 7, false, 0f, 20f, 0));
                    isDebounce = false;
                }
            }
            else if (nowchar == 3)
            {
                if (!Cooldownlist.Contains("SkillECD_3"))
                {
                    isDebounce = true;
                    Debug.Log("Skill E_3 Activated");
                    AddDataToList("SkillECD_3", Cooldownlist, 12f, 0);
                    StartCoroutine(Char_3_E(0.3f));
                    isDebounce = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isDebounce)  //R스킬
        {
            if (nowchar == 1) //스택 추가
            {
                isDebounce = true;
                Debug.Log("Skill R_1 Activated");
                AddDataToList("SkillRCD_1", Cooldownlist, 20f, 13);
                R_Activated = true;
                isDebounce = false;
            }
            else if (nowchar == 2) //짧게 발차기
            {
                isDebounce = true;
                Debug.Log("Skill R_2 Activated");
                AddDataToList("SkillRCD_2", Cooldownlist, 20f, 0);
                StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(1f, 2f), 0.1f, strength / 2, false, 20f, 0f, 14));
                isDebounce = false;
            }
            else if (nowchar == 3)
            {
                //패시브 스킬임(구현함 5타 패시브)
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
            else if (logic == 13)
            {
                R_Activated = false;
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
        StartCoroutine(ExecuteAttack(new Vector2(0f, -1f), new Vector2(5f, 1f), 0.1f, strength / 5, false, 0f, 7f, 0));
        isDebounce = false;
    }

    private IEnumerator EnemyAttack_1(string type_1, string type_2, Vector2 position_1, Vector2 scale_1, Vector3 angle_1, Vector2 position_2, Vector2 scale_2, Vector3 angle_2, float duration_1, float duration_2, float Damage, float Logic)
    {
        GameObject targetPrefab = null;

        if (type_1 == "attack_down")
        {
            targetPrefab = prefab.attack_down;
        }
        else if (type_1 == "attack_up")
        {
            targetPrefab = prefab.attack_up;
        }
        else if (type_1 == "attack_circle")
        {
            targetPrefab = prefab.attack_cricle;
        }

        if (targetPrefab != null)
        {

            GameObject enemyAttack = Instantiate(targetPrefab, position_1, Quaternion.identity);
            enemyAttack.transform.localScale = scale_1;
            enemyAttack.transform.eulerAngles = angle_1;

            yield return new WaitForSeconds(duration_1);

            Destroy(enemyAttack);
        }


        StartCoroutine(Real_EnemyAttack_1(type_2, position_2, scale_2, angle_2, duration_2, Damage, Logic));

    }
    private IEnumerator Real_EnemyAttack_1(string type, Vector2 position, Vector2 scale, Vector3 angle, float duratiron, float Damage, float logic)
    {
        GameObject targetPrefab = null;

        if (type == "real_attack_down") targetPrefab = prefab.real_attack_down;
        else if (type == "real_attack_up") targetPrefab = prefab.real_attack_up;
        else if (type == "real_attack_circle") targetPrefab = prefab.real_attack_circle;

        if (targetPrefab != null)
        {

            GameObject enemyAttack = Instantiate(targetPrefab, position, Quaternion.identity);
            enemyAttack.transform.localScale = scale;
            enemyAttack.transform.eulerAngles = angle;

            Vector3 direction = Vector3.down;
            if (type == "real_attack_up")
            {
                direction = Vector3.up;
            }

            if (logic == 1f) //돌아가는 로직
            {
                float step = 0.1f;
                for (int i = 0; i < 900; i++)
                {
                    enemyAttack.transform.Rotate(Vector3.forward, step);
                    yield return null;
                }
                Destroy(enemyAttack);
            }

            RaycastHit[] hits = Physics.BoxCastAll(position, scale / 2f, direction, Quaternion.identity, 100f);


            foreach (var hit in hits)
            {
                if (hit.collider.name == "player")
                {
                    Debug.Log("Player hit by Laser (" + type + ") for " + Damage + " damage.");
                    break;
                }
            }

            yield return new WaitForSeconds(duratiron);
            Destroy(enemyAttack);
        }
    }

    private IEnumerator ExecuteAttack(Vector2 offset, Vector2 size, float duration, float damage, bool isDownSmash, float Force_x, float Force_y, float logic) //대미지 구현 해야함
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
                            Boss_HP -= strength / 10;
                        }
                        else if (isUptilt)
                        {
                            enemyRb.linearVelocity = Vector2.zero;
                            enemyRb.AddForce(Vector2.up * Force_y, ForceMode2D.Impulse);
                            Boss_HP -= strength / 20;
                        }
                        else
                        {
                            enemyRb.AddForce(new Vector2(dir * Force_x, Force_y), ForceMode2D.Impulse);
                            Boss_HP -= strength / 20;
                        }

                        if (logic == 1f && R_Activated == false)
                        {
                            stack_char1 = 1;
                        }
                        else if (logic == 1f && R_Activated == true)
                        {
                            stack_char1 = 2;
                        }

                        if (nowchar == 3)
                        {
                            stack_char3_2 += 1;
                        }

                        if (stack_char3_2 == 5 && nowchar == 3)
                        {
                            stack_char3_2 = 0;
                            Boss_HP -= strength / 5;
                        }
                    }

                    if (nowchar == 4)
                    {
                        stack_char3 += 1;
                    }

                    if (nowchar == 4 && stack_char3 == 3)
                    {
                        stack_char3 = 0;
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator Char_3_E(float radius) //해야함
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
            StartCoroutine(ExecuteAttack(new Vector2(0f, -1f), new Vector2(2f, 5f), 0.1f, strength * 40 / 100, false, 0f, 7f, 0));
        }
    }
    public IEnumerator Char_3_W()
    {
        StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2.3f, 3f), 0.1f, strength / 10, false, 0f, 0f, 0));
        Debug.Log("1");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("2");
        StartCoroutine(ExecuteAttack(new Vector2(1f, 0.5f), new Vector2(2.3f, 3f), 0.1f, strength / 10, false, 0f, 0f, 0));
        yield return new WaitForSeconds(0.6f);
        Debug.Log("3");
        StartCoroutine(ExecuteAttack(new Vector2(1.5f, 0f), new Vector2(3f, 1f), 0.1f, strength / 5, false, 0f, 0f, 0));
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
                Boss_HP -= strength / 5;
            }
            else
            {
                transform.position = hitenemy.transform.position;
                Boss_HP -= strength / 5;
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
    //보스코드
    private IEnumerator Boss_Pattern_1_1()
    {

        GameObject boss = GameObject.Find("Boss_1");
        boss.transform.position = new Vector2(5, -3);

        for (int i = 0; i < 10; i++)
        {
            GameObject player = GameObject.Find("Player");

            Vector2 player_position = player.transform.position;
            playerTransForm = player.transform;

            float start_x_1 = player_position.x;
            float start_y_1 = player_position.y;
            StartCoroutine(EnemyAttack_1("attack_up", "real_attack_up", new Vector2(start_x_1, start_y_1), new Vector2(1, 100), new Vector3(0, 0, 0), new Vector2(start_x_1, start_y_1), new Vector2(1, 100), new Vector3(0, 0, 0), 1f, 1f, 10, 0));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(7f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_1_2()
    {
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(0, 0), new Vector2(20, 100), new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(20, 100), new Vector3(0, 0, 0), 2f, 4f, 100f, 0));
        yield return new WaitForSeconds(6f);
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(12, 0), new Vector2(20, 100), new Vector3(0, 0, 0), new Vector2(12, 0), new Vector2(20, 100), new Vector3(0, 0, 0), 2f, 4f, 100f, 0));
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(-12, 0), new Vector2(20, 100), new Vector3(0, 0, 0), new Vector2(-12, 0), new Vector2(20, 100), new Vector3(0, 0, 0), 2f, 4f, 100f, 0));
        yield return new WaitForSeconds(6f);
        GameObject boss = GameObject.Find("Boss_1");
        boss.transform.position = new Vector2(5, -3);
        yield return new WaitForSeconds(7f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_1_3()
    {
        StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(0, 0), new Vector2(15, 15), new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(15, 15), new Vector3(0, 0, 0), 2f, 2f, 100f, 0));
        yield return new WaitForSeconds(4f);
        StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(10, 10), new Vector2(20, 20), new Vector3(0, 0, 0), new Vector2(10, 10), new Vector2(20, 20), new Vector3(0, 0, 0), 2f, 2f, 100f, 0));
        StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(10, -10), new Vector2(20, 20), new Vector3(0, 0, 0), new Vector2(10, -10), new Vector2(20, 20), new Vector3(0, 0, 0), 2f, 2f, 100f, 0));
        StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(-10, 10), new Vector2(20, 20), new Vector3(0, 0, 0), new Vector2(-10, 10), new Vector2(20, 20), new Vector3(0, 0, 0), 2f, 2f, 100f, 0));
        StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(-10, -10), new Vector2(20, 20), new Vector3(0, 0, 0), new Vector2(-10, -10), new Vector2(20, 20), new Vector3(0, 0, 0), 2f, 2f, 100f, 0));
        yield return new WaitForSeconds(4f);
        GameObject boss = GameObject.Find("Boss_1");
        boss.transform.position = new Vector2(5, -3);
        yield return new WaitForSeconds(7f);
        Boss_Acting = false;
    }
    private IEnumerator Boss_Pattern_1_4()
    {
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(0, 0), new Vector2(1, 100), new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(1, 100), new Vector3(0, 0, 0), 2f, 10f, 100f, 1f));
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(0, 0), new Vector2(100, 1), new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(100, 1), new Vector3(0, 0, 0), 2f, 10f, 100f, 1f));
        yield return new WaitForSeconds(10f);
        GameObject boss = GameObject.Find("Boss_1");
        boss.transform.position = new Vector2(5, -3);
        yield return new WaitForSeconds(7f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_2_2()
    {
        int over0_x = Random.Range(0, 35);
        int over0_y = Random.Range(-3, 35);
        int over0_z = Random.Range(-180, 0);
        int random_num = Random.Range(0, 2);
        float duration = 2f;
        for (int i = 0; i < 10; i++)
        {
            over0_x = Random.Range(0, 35);
            over0_y = Random.Range(-3, 10);
            over0_z = Random.Range(-180, 0);
            random_num = Random.Range(0, 2);
            if (random_num == 1)
            {
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(over0_x, over0_y), new Vector2(1, 200), new Vector3(0, 0, over0_z), new Vector2(over0_x, over0_y), new Vector2(1, 200), new Vector3(0, 0, over0_z), duration, 0.5f, 100f, 0f));
            }
            else
            {
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(over0_x * -1f, over0_y), new Vector2(1, 200), new Vector3(0, 0, over0_z * -1f), new Vector2(over0_x * -1f, over0_y), new Vector2(1, 200), new Vector3(0, 0, over0_z * -1f), duration, 0.5f, 100f, 0f));
            }
            duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(9f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_2_1()
    {
        StartCoroutine(EnemyAttack_1("attack_up", "real_attack_up", new Vector2(-10, 0), new Vector2(10, 100), new Vector3(0, 0, 0), new Vector2(-10, 0), new Vector2(10, 100), new Vector3(0, 0, 0), 1f, 30f, 100f, 0));
        StartCoroutine(EnemyAttack_1("attack_up", "real_attack_up", new Vector2(10, 0), new Vector2(10, 100), new Vector3(0, 0, 0), new Vector2(10, 0), new Vector2(10, 100), new Vector3(0, 0, 0), 1f, 30f, 100f, 0));
        yield return new WaitForSeconds(1f);
        GameObject boss = GameObject.Find("Boss_2");
        boss.transform.position = new Vector2(0, -3);
        yield return new WaitForSeconds(5f);
        Boss_Acting = false;
        yield return new WaitForSeconds(25f);
        Boss_2_1_Acting = false;
    }

    private IEnumerator Boss_Pattern_2_3()
    {
        GameObject player = GameObject.Find("Player");
        float angle = 1f;
        for (int i = 0; i < 3; i++)
        {
            angle = 1f;
            float player_x = player.transform.position.x;
            float player_y = player.transform.position.y;

            for (int j = 0; j < 4; j++)
            {
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(player_x, player_y), new Vector2(100, 1), new Vector3(0, 0, angle), new Vector2(player_x, player_y), new Vector2(100, 1), new Vector3(0, 0, angle), 1f, 0.5f, 100f, 0));
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(player_x, player_y), new Vector2(100, 1), new Vector3(0, 0, angle * -1f), new Vector2(player_x, player_y), new Vector2(100, 1), new Vector3(0, 0, angle * -1f), 1f, 0.5f, 100f, 0));
                angle += 6f;
            }
            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(5f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_2_4()
    {
        float start_x = -40f;
        float start_y = 0f;
        for (int i = 0; i < 20; i++)
        {
            StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(start_x, 0), new Vector2(1, 100), new Vector3(0, 0, 0), new Vector2(start_x, 0), new Vector2(1, 100), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
            start_x += 4f;
        }
        yield return new WaitForSeconds(1f);
        for (int j = 0; j < 10; j++)
        {
            StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(0, start_y), new Vector2(100, 1), new Vector3(0, 0, 0), new Vector2(0, start_y), new Vector2(100, 1), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
            start_y += 4f;
        }
        yield return new WaitForSeconds(5f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_2_5()
    {
        float start_x_1 = -30f;
        float start_x_2 = -28f;
        for (int i = 0; i < 3; i++)
        {
            start_x_1 = -30f;
            start_x_2 = -28f;
            for (int j = 0; j < 15; j++)
            {
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(start_x_1, 0), new Vector2(1, 100), new Vector3(0, 0, 0), new Vector2(start_x_1, 0), new Vector2(1, 100), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
                start_x_1 += 4f;
            }
            yield return new WaitForSeconds(1f);
            for (int j = 0; j < 15; j++)
            {
                StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(start_x_2, 0), new Vector2(1, 100), new Vector3(0, 0, 0), new Vector2(start_x_2, 0), new Vector2(1, 100), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
                start_x_2 += 4f;
            }
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(5f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_3_1()
    {
        yield return null;
        GameObject player = GameObject.Find("Player");
        float player_x = player.transform.position.x;
        int random = Random.Range(0, 2);
        GameObject boss = GameObject.Find("Boss_3");
        boss.transform.position = new Vector2(player_x, -3);
        if (random == 0)
        {
            boss.transform.position = new Vector2(player_x+10f, -3);
            StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x + 10f, -4), new Vector2(10, 10), new Vector3(0, 0, 0), new Vector2(player_x + 10f, -4), new Vector2(10, 10), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
            yield return new WaitForSeconds(0.9f);
            for (int i = 1; i<10; i++)
            {
                StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x + 10f - i * 2f, -4), new Vector2(10 - i / 2, 10 - i / 2), new Vector3(0, 0, 0), new Vector2(player_x + 10f - i * 2f, -4), new Vector2(10 - i / 2, 10 - i / 2), new Vector3(0, 0, 0), 0.2f, 0.5f, 100f, 0f));
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            boss.transform.position = new Vector2(player_x - 10f, -3);
            StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x - 10f, -4), new Vector2(10, 10), new Vector3(0, 0, 0), new Vector2(player_x - 10f, -4), new Vector2(10, 10), new Vector3(0, 0, 0), 1f, 0.5f, 100f, 0f));
            yield return new WaitForSeconds(0.9f);
            for (int i = 1; i < 10; i++)
            {
                StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x - 10f + i * 2f, -4), new Vector2(10 - i/2, 10 - i/2), new Vector3(0, 0, 0), new Vector2(player_x - 10f + i * 2f, -4), new Vector2(10 - i / 2, 10 - i / 2), new Vector3(0, 0, 0), 0.2f, 0.5f, 100f, 0f));
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(6f);
        Boss_Acting = false;
    }
    
    private IEnumerator Boss_Pattern_3_2()
    {
        GameObject player = GameObject.Find("Player");
        GameObject boss = GameObject.Find("Boss_3");
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        for (int i = 0; i < 3; i++)
        {
            float player_x = player.transform.position.x;
            boss.transform.position = new Vector2(player_x, 200);
            rb.AddForce(new Vector2(0, -70), ForceMode2D.Impulse);
            StartCoroutine(EnemyAttack_1("attack_down", "real_attack_down", new Vector2(player_x, 40), new Vector2(2, 100), new Vector3(0, 0, 0), new Vector2(player_x, 40), new Vector2(2, 100), new Vector3(0, 0, 0), 2f, 0.2f, 100f, 0f));
            yield return new WaitForSeconds(2f);
            //boss.transform.position = new Vector2(player_x, -3);
            for(int j = 0; j < 7; j++)
            {
                StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x + j * 2f, -4), new Vector2(7 - j/2, 7 - j/2), new Vector3(0, 0, 0), new Vector2(player_x + j * 2f, -4), new Vector2(7 - j / 2, 7 - j / 2), new Vector3(0, 0, 0), 0.2f, 0.5f, 100f, 0f));
                StartCoroutine(EnemyAttack_1("attack_circle", "real_attack_circle", new Vector2(player_x - j * 2f, -4), new Vector2(7 - j / 2, 7 - j / 2), new Vector3(0, 0, 0), new Vector2(player_x - j * 2f, -4), new Vector2(7 - j / 2, 7 - j / 2), new Vector3(0, 0, 0), 0.2f, 0.5f, 100f, 0f));
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(6f);
        Boss_Acting = false;
    }

    private IEnumerator Boss_Pattern_3_3()
    {
        GameObject boss = GameObject.Find("Boss_3");
        GameObject player = GameObject.Find("Player");
        boss.transform.position = new Vector2(0, -3);
        StartCoroutine(EnemyAttack_1("attack_down", "real_attack_circle", new Vector2(0, -4), new Vector2(40, 5), new Vector3(0, 0, 0), new Vector2(0, -4), new Vector2(5, 5), new Vector3(0, 0, 0), 2f, 0.5f, 100f, 0));
        yield return new WaitForSeconds(2f);
        Boss_Pattern = 10;
        for (int i = 1; i < 10; i++)
        {
            StartCoroutine(EnemyAttack_1("attack_down", "real_attack_circle", new Vector2(100, -4), new Vector2(100, 5), new Vector3(0, 0, 0), new Vector2(0 + i * 2f, -4), new Vector2(5 -  i / 5 , 5 - i / 5), new Vector3(0, 0, 0), 0.1f, 0.5f, 100f, 0f));
            StartCoroutine(EnemyAttack_1("attack_down", "real_attack_circle", new Vector2(100, -4), new Vector2(100, 5), new Vector3(0, 0, 0), new Vector2(0 - i * 2f, -4), new Vector2(5 - i / 5, 5 - i / 5), new Vector3(0, 0, 0), 0.1f, 0.5f, 100f, 0f));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(6f);
        Boss_Acting = false;
    }
    private IEnumerator Boss_Pattern_3_4_1()
    {
        GameObject player = GameObject.Find("Player");
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;
        while(Boss_3_4_Iced == true)
        {
            player.transform.position = new Vector2(player_x,player_y);
            if (Boss_3_4_Iced == false)
            {
                yield break;
            }
            yield return null;
        }
    }
    private IEnumerator Boss_Pattern_3_4_2()
    {
        yield return new WaitForSeconds(2f);
        Boss_Acting = false;
        yield return new WaitForSeconds(6f);
        Boss_3_4_Acting = false;
        Boss_3_4_Iced = false;
    }
}