using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Puan Ayarlar?")]
    public int totalScore = 0;      // Ba?lang?? puan?
    public int maxScore = 100;       // Maksimum puan
    public int minScore = 0;         // Minimum puan

    public TextMeshProUGUI scoreText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalScore = 0; // Sahne başında her zaman 0'dan başla
        UpdateScoreUI();
    }

    // Puan eklemek i?in
    public void PuanEkle(int miktar)
    {
        totalScore = Mathf.Clamp(totalScore + miktar, minScore, maxScore);
        UpdateScoreUI();
    }

    // Puan d???rmek i?in
    public void PuanDus(int miktar)
    {
        totalScore = Mathf.Clamp(totalScore - miktar, minScore, maxScore);
        UpdateScoreUI();
    }

     public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + totalScore;
        }
    }
}