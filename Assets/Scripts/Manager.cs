using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    public float health;
    
    public string currentScene;
    public int difficulty;
    
    public float exp;
    public float maxExp;
    public int level;

    public float timer;

    public GameObject playerPrefab;
    GameObject player;

    void Awake()
    {
        if(Instance == null)
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
        health = 100;
        exp = 0;
        difficulty = 1;
        level = 1;

        maxExp = CalculateMaxExp();

        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Instantiate(playerPrefab);
            DontDestroyOnLoad(player);

            DamageManager.Instance.SetPlayer(player);
        }

        currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "TestScene_Start" || currentScene == "TestScene_Setting" || currentScene == "TestScene_Menu" || health <= 0)
        {
            player.SetActive(false);
        }
        else
        {
            player.SetActive(true);
            timer += Time.deltaTime; // health <= 0일 때도 정지됨
        }

        //LevelUp(exp);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        player.transform.position = new Vector3(0, 0, 0);
    }

    public void GetExp(float e)
    {
        exp += e;
        LevelUp();
    }

    public void LoseExp(float e)
    {
        exp -= e;
        LevelUp();
    }

    public void LevelUp()
    {
        while (exp >= maxExp)
        {
            exp -= maxExp;
            level++;
            maxExp = CalculateMaxExp();
        }
    }

    public float CalculateMaxExp()
    {
        return (100f * (1f + 0.5f * (level - 1))) * (1f + 0.2f * (difficulty - 1));
    }
}
