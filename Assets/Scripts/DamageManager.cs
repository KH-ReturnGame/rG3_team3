using System.Collections;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance;

    GameObject player;
    SpriteRenderer spRend;

    float damage;
    float totalDamage;
    float recover;

    Coroutine burningCoroutine;

    public void SetPlayer(GameObject p)
    {
        player = p;
        spRend = player.GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Manager.Instance.health <= 0)
        //{
        //    player.SetActive(false);
        //}
    }

    public void GetDamage(float d)
    {
        if (Manager.Instance.health > 0)
        {
            damage = d;
            Manager.Instance.health -= damage;
            totalDamage += damage;
            //Debug.Log("damage: " + damage + ", total damage: " + totalDamage + ", current health is " + Manager.Instance.health);
        }
        else
        {
            Debug.Log("can't damage");
        }
    }

    public void Recover(float r)
    {
        if (Manager.Instance.health > 0 && Manager.Instance.health < 100)
        {
            recover = r;
            Manager.Instance.health += recover;
            //Debug.Log(recover + " health recoverd. current health is " + Manager.Instance.health);
        }
        else if (Manager.Instance.health <= 0)
        {
            Debug.Log("can't recover");
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
            // 시각 효과
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
        player.SetActive(true);
        spRend.color = Color.white;
    }
}
