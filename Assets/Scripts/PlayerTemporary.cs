using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool Dash = false;





    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public float look = 0f;
    public float nowchar = 1;
    //1번째 캐릭터 변수
    public int stack_char1 = 0; //적이 받는 스택
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

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isDebounce)     //�뽬(�ӽ�)
        {
            Dash = true;
            rb2d.AddForce(new Vector2(10f, 0), ForceMode2D.Impulse);
            if (!Cooldownlist.Contains("Skill1CD"))
            {   
                isDebounce = true;
                Debug.Log("Skill 1 Activated");
                AddDataToList("Dash", Stunlist, 0.2f);
                AddDataToList("Skill1CD", Cooldownlist, 3f);
                StartCoroutine(ResetDebounce(0.5f));
                StartCoroutine(ExecuteAttack(new Vector2(5f, 0f), new Vector2(6f, 3f), 0.5f, 0f, false));
            }
        }

        if (Input.GetKeyDown(KeyCode.W) && !isDebounce)   //W스킬
        {
            if(nowchar == 1)
            {
                if (!Cooldownlist.Contains("SkillWCD_1"))
                {
                isDebounce = true;
                look = sprdr.flipX ? -1f : 1f;
                Debug.Log("Skill W_1 Activated");
                StartCoroutine(Dashdown(0.25f));
                AddDataToList("SkillWCD_1", Cooldownlist, 5f, 0);
                AddDataToList("Dash", Cooldownlist, 0.6f, 0);
                rb2d.AddForce(new Vector2(look*0.13f,15f), ForceMode2D.Impulse);
                StartCoroutine(ResetDebounce(0.5f));
                StartCoroutine(ExecuteAttack(new Vector2(5f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false, 5f, 7f, 0));
                }
            }
            else if(nowchar == 2)
            {
                if (!Cooldownlist.Contains("SkillWCD_2"))
                {
                    isDebounce = true;
                    AddDataToList("SkillWCD_2", Cooldownlist, 10f, 0);
                    StartCoroutine(Defend_2(80,5));
                    Debug.Log("Skill W_2 Activated");
                    isDebounce = false;
                }
            }
            else if (nowchar == 3)
            {
                if (!Cooldownlist.Contains("SkillWCD_3"))
                {
                isDebounce = true;
                look = sprdr.flipX ? -1f : 1f;
                Debug.Log("Skill W_3 Activated");
                StartCoroutine(Dashdown(0.25f));
                AddDataToList("SkillWCD_3", Cooldownlist, 5f, 0);
                AddDataToList("Dash", Cooldownlist, 0.6f, 0);
                rb2d.AddForce(new Vector2(look*0.13f,15f), ForceMode2D.Impulse);
                StartCoroutine(ResetDebounce(0.5f));
                StartCoroutine(ExecuteAttack(new Vector2(5f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false, 5f, 7f, 0));
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.E) && !isDebounce)  //E스킬
        {
            if(nowchar == 1)
            {
                
            }
            else if(nowchar == 2)
            {
                if (!Cooldownlist.Contains("SkillQCD_2"))
                {
                isDebounce = true;
                look = sprdr.flipX ? -1f : 1f;
                Debug.Log("Skill Q Activated");
                AddDataToList("SkillQCD_2", Cooldownlist, 3f, 0);
                AddDataToList("Dash", Cooldownlist, 0.2f, 0);
                rb2d.AddForce(new Vector2(look*20f,0f), ForceMode2D.Impulse);
                StartCoroutine(ResetDebounce(0.5f));
                StartCoroutine(ExecuteAttack(new Vector2(5f, 0.5f), new Vector2(9f, 6f), 0.1f, 30f, false, 15f, 0f, 0));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isDebounce)  //R스킬
        {
            
        }
    }

    void FixedUpdate()
    {
        if (!Stunlist.Contains("Stun") && !Stunlist.Contains("Dash"))
        {
            rb2d.linearVelocity = new Vector2(moveInput * movementSpeed, rb2d.linearVelocity.y);
        }
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

    private IEnumerator ExecuteAttack(Vector2 offset, Vector2 size, float duration, float damage, bool isDownSmash, float Force_x, float Force_y, int logic)
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
                        if(nowchar == 1)
                        {
                            //enemyRb.stack_char1 += 1;
                        }
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (sprdr == null) return;
        Gizmos.color = Color.red;
        float dir = sprdr.flipX ? -1f : 1f;
        Gizmos.DrawWireCube(transform.position + new Vector3(2f * dir, 0.5f, 0), new Vector3(9f, 6f, 1f));
    }
}