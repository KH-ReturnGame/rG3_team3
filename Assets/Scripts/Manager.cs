using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    public float health;
    public int score;
    public string currentScene;

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
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Instantiate(playerPrefab);
            DontDestroyOnLoad(player);
        }

        currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "TestScene_Start" || currentScene == "TestScene_Setting" || currentScene == "TestScene_Menu")
        {
            player.SetActive(false);
        }
        else
        {
            player.SetActive(true);
        }
    }

     public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
