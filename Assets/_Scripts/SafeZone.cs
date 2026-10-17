using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private bool isPlayerExit = false;
    public AudioSource audio;
    public AudioClip charaterRunFire;
    public AudioClip failEarlyFire;
    public bool isExamScene = false;
    private bool isAudioStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Rig")) 
            && isPlayerExit == true 
            && SahneYonetici.isFireStart == true)
        {
            if (YanginDurumYonetici.Instance == null) 
            {
                Debug.LogWarning("[SafeZone] YanginDurumYonetici.Instance null!");
                return;
            }

            if (GameObject.FindGameObjectsWithTag("fire").Length >= YanginDurumYonetici.Instance.maxFireCountThreshold)
            {
                if (isExamScene == false && isAudioStarted == false)
                {
                    isAudioStarted = true;
                    audio.PlayOneShot(charaterRunFire);
                }

                // Puan SimulasyonuBitir içinde Orta Riskli bandına (60-79) göre verilir
                YanginDurumYonetici.Instance.SimulasyonuBitir(true, "Doğru Karar! Kontrolden çıkan yangından başarıyla tahliye oldunuz.", false, true);
            }
            else
            {
                // Eğer sahne zaten başarıyla bittiyse SafeZone'u tamamen yoksay
                if (YanginDurumYonetici.isSceneSuccessful == true)
                    return;

                if (isExamScene == false && isAudioStarted == false)
                {
                    isAudioStarted = true;
                    audio.PlayOneShot(failEarlyFire);
                }

                YanginDurumYonetici.Instance.SimulasyonuBitir(false, "Başarısız! Yangın hala söndürülebilirdi, alanı erken terk ettiniz.");
    
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.PuanDus(80);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerExit = true;
        }
    }
}