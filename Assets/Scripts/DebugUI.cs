using UnityEngine;
using TMPro;

public class DebugUI : MonoBehaviour
{
    public TMP_Text debugText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        debugText.text =
            "Current Scene: " + Manager.Instance.currentScene +
            "\nHP: " + Manager.Instance.health +
            "\nDifficulty: " + Manager.Instance.difficulty +
            "\nTimer: " + Manager.Instance.timer +
            "\nLevel: " + Manager.Instance.level +
            "\nCurrent Exp: " + Manager.Instance.exp +
            "\nCurrent MaxExp: " + Manager.Instance.maxExp;
    }
}
