using System.Collections;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    SpriteRenderer spRend;

    float damage;
    float totalDamage;
    float recover;

    Coroutine burningCoroutine;

    void Awake()
    {
        spRend = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.Instance.health <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void GetDamage(float d)
    {
        damage = d;
        Manager.Instance.health -= damage;
        totalDamage += damage;
        //Debug.Log("damage: " + damage + ", total damage: " + totalDamage + ", current health is " + Manager.Instance.health);
    }

    public void Recover(float r)
    {
        if (Manager.Instance.health < 100)
        {
            recover = r;
            Manager.Instance.health += recover;
            Debug.Log(recover + " health recoverd. current health is " + Manager.Instance.health);
        }
        else
        {
            Debug.Log("the health is max");
        }
    }

    IEnumerator BurningDamage(float d, int i)
    {
        totalDamage = 0;
        
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
        spRend.color = Color.white;
        Recover(5);
    }

    public void Burning()
    {
        burningCoroutine = StartCoroutine(BurningDamage(5, 5));
    }

    public void Revive()
    {
        Manager.Instance.health = 100;
        gameObject.SetActive(true);
        spRend.color = Color.white;
    }
}
