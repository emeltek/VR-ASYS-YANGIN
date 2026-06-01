using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Puan Ayarlarý")]
    public int totalScore = 80;      // Baþlangýç puaný
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
        totalScore = 80; // Sahne baþýnda her zaman 80'den baþla
        UpdateScoreUI();
    }

    // Puan eklemek için
    public void PuanEkle(int miktar)
    {
        totalScore = Mathf.Clamp(totalScore + miktar, minScore, maxScore);
        UpdateScoreUI();
    }

    // Puan düþürmek için
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