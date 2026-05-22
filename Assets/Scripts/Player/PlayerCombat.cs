using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerInput plrInput;
    private PlayerManager plrManager;


    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
    }
}
