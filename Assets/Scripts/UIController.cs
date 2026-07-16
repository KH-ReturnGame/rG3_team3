using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text sliderText;
    public Slider slider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (slider != null)
        {
            slider.value = Manager.Instance.difficulty;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (slider != null && sliderText != null)
        {
            sliderText.text = "" + Manager.Instance.difficulty;

            SetDifficulty((int)slider.value);
            slider.value = Manager.Instance.difficulty;
            Manager.Instance.maxExp = Manager.Instance.CalculateMaxExp();
        }
    }

    public void ChangeScene(string sceneName)
    {
        Manager.Instance.ChangeScene(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }


    public void SetDifficulty(int d)
    {
        Manager.Instance.difficulty = d;
    }

    // References for DamageManager
    public void Burn()
    {
        DamageManager.Instance.Burning();
    }

    public void TakeWater()
    {
        DamageManager.Instance.TakeWater();
    }

    public void Revive()
    {
        DamageManager.Instance.Revive();
    }

    public void GetExp(float e)
    {
        Manager.Instance.GetExp(e);
    }

    public void LoseExp(float e)
    {
        Manager.Instance.LoseExp(e);
    }
}
