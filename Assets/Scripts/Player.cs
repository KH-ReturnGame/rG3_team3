using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spRend;

    public float speed;
    Vector2 dirVec;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //rigid = GetComponent<Rigidbody2D>();
        //spRend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //dirVec.x = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        //rigid.MovePosition(rigid.position + dirVec.normalized * speed * Time.fixedDeltaTime);
        //rigid.linearVelocityX = dirVec.normalized.x * speed * Time.fixedDeltaTime;
        //rigid.AddForce(rigid.position + dirVec.normalized * speed * Time.fixedDeltaTime);
    }
}
