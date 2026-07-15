using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Player : MonoBehaviour
{
    [SerializeField]  private Rigidbody2D body2d;
    [SerializeField]  private Animator animtr;
    [SerializeField]  private SpriteRenderer spdr;

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

    // combo 


    public int currentComboStack = 0;
    private float lastComboChangeTime;
    private float comboStackResetLimit = 1f;
    private int maxComboStack = 5;

    //

    private float attackCooldown = 0.45f;

    //

    [Header("상태리스트")]
    public List<string> Stunlist = new List<string>();
    public List<string> FrameList = new List<string>();

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
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

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
        body2d.linearVelocity = new Vector2(xInput * moveSpeed, body2d.linearVelocity.y);

        animtr.SetFloat("yVelocity", body2d.linearVelocityY);



        directionInspection();
    }

    void OnDrawGizmos()
    {

    }




    // keyInputsHandler

    private void keyInputdetection()
    {
        if (Input.GetButtonDown("Jump")) // Jump + 추후 수정 예정 + 공중 대쉬 + 더블 점프 + 내려찍기 등 
        {
            if (isGrounded == true)
            {
                StartCoroutine(Jump());
            }
        }
        if (Input.GetKeyDown(KeyCode.Q)) // Jump + 추후 수정 예정 + 공중 대쉬 + 더블 점프 + 내려찍기 등 
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
        if (transform.rotation.y != 0f)
            isfacingRight = false;
        else
            isfacingRight = true;
    }

    // spriteManipulator



    public void setSprite() // 나중에 모션 발동중에 뒤집히는거 감안해서 뭐해야하는듯
    {
        // sprite x/y flip (left or right) + need to add '스킬을 쓰고 있어서 방향을 못바꾸는가?' check.
        if (xInput > 0)
           transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0f, transform.eulerAngles.z);
           
        else if (xInput < 0)
           transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180f, transform.eulerAngles.z);

    }

    // basicAnimationhandler



    // ======================================================================================================================================================
    // ======================================================================================================================================================
    // ======================================================================================================================================================

    private IEnumerator Jump()
    {
        animtr.SetTrigger("isJumpTrigger");
        yield return new WaitForSeconds(.1f);
        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpHeight);

    }

    private IEnumerator Dash()
    {
        //isDashing = true;

        // 바라보는 방향 결정
        float dir = spdr.flipX ? -1f : 1f;


        animtr.SetTrigger("isDash");

        yield return new WaitForSeconds(0.2f);

        // 대쉬 전 순간적으로 속도를 0으로 초기화 (더 깔끔한 대쉬를 위해)
        body2d.linearVelocity = new Vector2(0, body2d.linearVelocity.y);

        // 대쉬 힘 가하기
        body2d.AddForce(new Vector2(dir * 80f, 0), ForceMode2D.Impulse);



//
    }


    private void BasicAttack()
    {
        if (Stunlist.Count > 0) return;

        if (!Cooldownlist.Contains("Attack"))
        {
            // 1. 공격을 누른 즉시 콤보 스택 증가 (0 -> 1 -> 2 -> 3 -> 4)
            currentComboStack++;

            // 최대 콤보(4타)를 넘어가면 다시 1타로 순환 구문
            if (currentComboStack > 4)
            {
                currentComboStack = 1;
            }

            lastComboChangeTime = Time.time;
            comboStackResetLimit = 1.5f;
            Debug.Log("Combo : " + currentComboStack);

            // ★ 핵심: 트리거를 터뜨리기 직전에 애니메이터의 Combo 정수값을 즉시 주입합니다.
            animtr.SetInteger("Combo", currentComboStack);
   

            // 2. 공격 트리거 발동 (이제 애니메이터는 변경된 Combo 값을 기준으로 트랜지션을 체크합니다)
            animtr.SetTrigger("isAttackTrigger");
          


            // 쿨다운 등록
            Utility.DataManagement.ListManagement.AddData("Attack", Cooldownlist, attackCooldown);
        }
    }


    /*
    private void BasicAttack()
    {
        if (Stunlist.Count > 0) return;
        if (!Cooldownlist.Contains("Attack"))
        {


            currentComboStack++;
            lastComboChangeTime = Time.time;
            comboStackResetLimit = 1.5f;
            Debug.Log("Combo : " + currentComboStack);

            Utility.DataManagement.ListManagement.AddData("Attack", Cooldownlist, attackCooldown);


            /*
            if (!plrManager.isGrounded && plrInput.yInput > 0)
            {
                Debug.Log("Aerial Cleaves: 체공");

                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0f);
                ExecuteHitbox(1.5f, 10f, 0f);
            }
           

            else if (plrManager.isGrounded)
            {
                if (weapon.hasForwardMove && plrInput.yInput != 1)
                {
                    Debug.Log("Frontdash: 지상 전진 평타");
                    float facingDirection = sprdr.flipX ? -1f : 1f;
                    rb2d.AddForce(new Vector2(facingDirection * forwardThrustForce, 0f));
                }
                ExecuteHitbox(1.5f, 10f, 0f);
            }
           
       animtr.SetTrigger("isAttackTrigger");


     



        }
        }
    */
}
