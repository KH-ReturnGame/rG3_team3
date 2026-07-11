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

 
}
