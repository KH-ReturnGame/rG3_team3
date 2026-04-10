using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    int gold;
    int exp;
    int difficulty;

    public static Manager Instance;

    public GameObject scenemanager;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(scenemanager);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Skill1()
    {

    }

    void Skill2()
    {

    }

    void Skill3()
    {

    }

     public void ChangeScene()
    {
        difficulty = Random.Range(1, 4);

        if (difficulty == 1)
        {
            SceneManager.LoadScene("StartScene");
        }
        else if (difficulty == 2)
        {
            SceneManager.LoadScene("StageScene");
        }
        else if (difficulty == 3)
        {
            SceneManager.LoadScene("BossScene");
        }
        else
        {
            return;
        }
    }
}
