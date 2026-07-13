using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        //if (Manager.Instance.currentScene == "TestScene_Start" || Manager.Instance.currentScene == "TestScene_Setting" || Manager.Instance.currentScene == "TestScene_Menu")
        //{
        //    gameObject.SetActive(false);
        //}
        //else
        //{
        //    gameObject.SetActive(true);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //if (Manager.Instance.currentScene == "TestScene_Start" || Manager.Instance.currentScene == "TestScene_Setting" || Manager.Instance.currentScene == "TestScene_Menu")
        //{
        //    gameObject.SetActive(false);
        //}
        //else
        //{
        //    gameObject.SetActive(true);
        //}
    }
}
