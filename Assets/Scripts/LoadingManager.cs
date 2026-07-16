using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(Manager.Instance.nextScene);

        while (!op.isDone)
        {
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
