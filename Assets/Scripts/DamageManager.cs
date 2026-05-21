using System.Collections;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public GameObject circle;
    SpriteRenderer spRend;
    
    float health;
    float damage;

    Coroutine burningCoroutine;

    void Awake()
    {
        spRend = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            circle.SetActive(false);
        }
    }

    public void GetDamage(float d)
    {
        damage = d;
        health -= damage;
        Debug.Log("damage: " + damage);
    }

    IEnumerator BurningDamage(float d, int i)
    {
        for (int j = 0; j < i; j++)
        {
            GetDamage(10);
            spRend.color = Color.red;
            yield return new WaitForSeconds(0.25f);
            spRend.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void TakeWater()
    {
        StopCoroutine(burningCoroutine);
        health += 5;
    }

    public void Burning()
    {
        burningCoroutine = StartCoroutine(BurningDamage(5, 5));
    }
}
