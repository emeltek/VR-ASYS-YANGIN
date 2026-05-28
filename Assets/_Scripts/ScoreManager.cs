using UnityEngine;
using TMPro; // Eðer ekranda UI olarak göstereceksen

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Diðer scriptlerden kolayca eriþmek için

    [Header("Puan Ayarlarý")]
    public int totalScore = 100; // Senaryo baþlangýç puaný
    public int penaltyPerBurnedTree = 5; // Yanan aðaç baþýna kesilecek puan

    public TextMeshProUGUI scoreText; // VR arayüzündeki yazý (Opsiyonel)

    void Awake()
    {
        // Singleton Deseni: Kolay eriþim saðlar
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
    }

    // Aðaç yandýðýnda çaðrýlacak fonksiyon
    public void TreeBurnedPenalty()
    {
        totalScore -= penaltyPerBurnedTree;
        
        // Puanýn sýfýrýn altýna düþmesini engelle
        if (totalScore < 0) totalScore = 0; 

        Debug.Log("Bir aðaç yandý! Kalan Puan: " + totalScore);
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + totalScore;
        }
    }
}