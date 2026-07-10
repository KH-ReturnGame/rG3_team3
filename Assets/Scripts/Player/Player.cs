using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D body2d;
    private Animator animtr;
    private SpriteRenderer spdr; 

    // input Relavant 
    public float xInput, yInput;

    // basic_statement Relavant
    public bool isGrounded;
    public float checkRadius = 0.2f;
    public Transform groundCheck;
    public LayerMask GroundLayer;

    // character_realTime_variables

    public float moveSpeed = 5f;
    public float jumpHeight = 30f;



    void Awake()
    {
        body2d = GetComponent<Rigidbody2D>();
        spdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();

    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        // ground Relavant
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, GroundLayer);

        // character_updater
        setSprite();

        // keyDetection
        keyInputdetection();

        if (xInput != 0)
            animtr.SetBool("isWalking", true);
        else
            animtr.SetBool("isWalking", false);




    }

    void FixedUpdate()
    {
        body2d.linearVelocity = new Vector2(xInput * moveSpeed, body2d.linearVelocity.y);
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
                body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpHeight);
            }
        }


    }

    // spriteManipulator

    public void setSprite() // 나중에 모션 발동중에 뒤집히는거 감안해서 뭐해야하는듯
    {
        // sprite x/y flip (left or right) + need to add '스킬을 쓰고 있어서 방향을 못바꾸는가?' check.
        if (xInput > 0)
            spdr.flipX = false;
        else if (xInput < 0)
            spdr.flipX = true;
    }

    // basicAnimationhandler


}
