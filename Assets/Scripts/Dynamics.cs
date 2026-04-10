using UnityEngine;

public class Dynamics : MonoBehaviour
{
    Rigidbody2D rb2d;

    public Vector2 impulseVec;
    public float intensity;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ParabolaMovement()
    {
        rb2d.linearVelocity = impulseVec.normalized * intensity;
        //transform.position.x
    }
}
