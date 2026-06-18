using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerManager : MonoBehaviour
{
    private PlayerInput plrInput; 
    private PlayerCombat plrCombat;

    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    [Header("기본 스탯 요소")]
    public float Health = 100f;
    public float Mana = 0f;
    public float MoveSpeed = 0f;
    public float JumpPower = 0f;

    public string currentWeapon = "testSword"; //임시


    [Header("캐릭터 제어 요소")]
    public bool CanMove = true;

    [Header("땅 밟음 체크")]
    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask GroundLayer;


    [Header("상태리스트")]
    public List<string> Stunlist = new List<string>();
    public List<string> FrameList = new List<string>();

    [Header("내부상태리스트")]
    public List<string> Cooldownlist = new List<string>();
    public List<string> Frameslist = new List<string>(); // Armor, Hyperarmor 보관 



    void Awake()
    {
        plrInput = GetComponent<PlayerInput>();
        plrCombat = GetComponent<PlayerCombat>();
        // Initialization -- 초기화 작업: DefaultData라는 게임 '첫' 시작에 있는 디폴트 플레이어 능력치를 참고하여 시작할때의 능력치를 초기화한다.
        Health = PlayerDefaultData.Health;
        Mana = PlayerDefaultData.Mana;
        MoveSpeed = PlayerDefaultData.MoveSpeed;
        JumpPower = PlayerDefaultData.JumpPower;
        //주요 컴포넌트 설정
        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 땅 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, GroundLayer);
        // 
        FlipSprite();
    }

    /// <summary>
    /// 좌우이동에 스프라이트 방향 수정도 체크. 근데 공격 도중에 바꾸는거 여부는 나중에 검토.
    /// </summary>

    void FixedUpdate()
    {
          if (CanMove == true)
        {
            // 좌우 이동에 필요한 힘 조절기
             rb2d.linearVelocity = new Vector2(plrInput.xInput * MoveSpeed, rb2d.linearVelocity.y);
        }
    }

    public void Jump()
    {
          if(isGrounded == true)
        {
            // 점프 기능, 아직은 불안정 해보임
             rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, JumpPower);
        }
    }

    public void FlipSprite() // 나중에 모션 발동중에 뒤집히는거 감안해서 뭐해야하는듯
    {
        if (plrInput.xInput > 0)
            sprdr.flipX = false;
        else if (plrInput.xInput < 0)
            sprdr.flipX = true;

    }
}
