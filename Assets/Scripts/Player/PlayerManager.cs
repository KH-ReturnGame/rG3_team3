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


    [Header("캐릭터 제어 요소")]
    public bool CanMove = true;

    [Header("땅 밟음 체크")]
    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask GroundLayer;


    [Header("상태리스트")]
    public List<string> Stunlist = new List<string>();
    public List<string> Cooldownlist = new List<string>();



    void Awake()
    {
        plrInput = GetComponent<PlayerInput>();
        plrCombat = GetComponent<PlayerCombat>();
        // Initialization -- 초기화 작업
        Health = PlayerDefaultData.Health;
        Mana = PlayerDefaultData.Mana;
        MoveSpeed = PlayerDefaultData.MoveSpeed;
        JumpPower = PlayerDefaultData.JumpPower;
        //
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
    }

    void FixedUpdate()
    {
          if (CanMove == true)
        {
             rb2d.linearVelocity = new Vector2(plrInput.xInput * MoveSpeed, rb2d.linearVelocity.y);
        }
    }

    public void Jump()
    {
          if(isGrounded == true)
        {
             rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, JumpPower);
        }
    }
}
