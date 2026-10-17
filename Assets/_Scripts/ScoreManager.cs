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

    // Puanı doğrudan belirli bir değere ayarlar (örn. tahliye bandı)
    public void PuanAyarla(int deger)
    {
        totalScore = Mathf.Clamp(deger, minScore, maxScore);
        UpdateScoreUI();
    }

    private bool isFinal = false; // Senaryo bittiyse risk etiketi de gösterilir

    // Senaryo sonunda çağrılır: bundan sonraki her puan değişiminde etiket de güncellenir
    public void ShowFinalScore()
    {
        isFinal = true;
        UpdateScoreUI();
    }

     public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + totalScore;
            if (isFinal)
                scoreText.text += "\nRisk Düzeyi: " + GetRiskLabel(totalScore);
        }
    }

    // Puana göre renkli risk etiketi döner
    private string GetRiskLabel(int value)
    {
        if (value >= 80) return "<color=#2ECC71>Güvenli</color>";
        if (value >= 60) return "<color=#F1C40F>Orta Riskli</color>";
        if (value >= 40) return "<color=#E67E22>Riskli</color>";
        return "<color=#E74C3C>Çok Riskli</color>";
    }
}