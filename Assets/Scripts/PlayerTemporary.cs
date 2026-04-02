using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemporary : MonoBehaviour
{
    /* < The beginning of the script >
    
    +  Stuffs you can do here : 

    Variables Setting ( mijisu setting )
    List Setting
    Seriallize Fields 

    The example form of variable definition:
    [ ACCESS MODIFIER ] / [ OTHER MODIFIER ] / [ DATA TYPE ] / [ DATA NAME ] = [ DATA VALUE ];
    
    */

    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;


    private float movementSpeed = 30;
    
    private float moveInput;


    void Awake()
    {
     /*

     < The first ever running function whenever you start the unity >

     + Stuffs you can do here: 

     [ DATA NAME ] = [ REFERENCE ];

     [ DATA NAME ] = [ INITIAL VALUE ];

     */

    rb2d = GetComponent<Rigidbody2D>();
    sprdr = GetComponent<SpriteRenderer>();

    }


    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

         if (moveInput > 0)
            sprdr.flipX = false;
        else if (moveInput < 0)
            sprdr.flipX = true;

        // runs every per frame
    }

    void FixedUpdate()
    {
        // faster than physics
        rb2d.linearVelocity = new Vector2(moveInput * movementSpeed, rb2d.linearVelocity.y);

    }

    void LateUpdate()
    {
        // The inner last Update function
    }
}
