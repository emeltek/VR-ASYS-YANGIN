using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Gelen nesne oyuncu mu? (VR Kamerasının Rig'i veya elleri)
        // Eğer OVRCameraRig'e "Player" tag'i verdiysen:
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Rig"))
        {
            // Yangın Durum Yöneticisine bakıyoruz
            if (YanginDurumYonetici.Instance != null)
            {
                // EĞER yangın kontrolden çıkmışsa ve oyuncu buraya kaçtıysa: İYİ/DOĞRU DAVRANIŞ BİTİŞİ
                // EĞER yangın hala ufaksa ve bodoslama kaçtıysa: KÖTÜ/KORKAK DAVRANIŞ BİTİŞİ
                
                // Kod durum yöneticisindeki isKontroldenCikti değişkenine göre kararı otomatik verir:
                if (GameObject.FindGameObjectsWithTag("fire").Length >= YanginDurumYonetici.Instance.maxFireCountThreshold)
                {
                    YanginDurumYonetici.Instance.SimulasyonuBitir(true, "Doğru Karar! Kontrolden çıkan yangından başarıyla tahliye oldunuz.");
                }
                else
                {
                    YanginDurumYonetici.Instance.SimulasyonuBitir(false, "Başarısız! Yangın hala söndürülebilirdi, alanı erken terk ettiniz.");
                }
            }
        }
    }
}