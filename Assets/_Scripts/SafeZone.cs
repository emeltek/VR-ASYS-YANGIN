using System;
using Unity.VisualScripting;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private bool isPlayerExit = false;
    public AudioSource audio;
    public AudioClip charaterRunFire;
    public AudioClip failEarlyFire;
    private void OnTriggerEnter(Collider other)
    {
        // Gelen nesne oyuncu mu? (VR Kamerasının Rig'i veya elleri)
        // Eğer OVRCameraRig'e "Player" tag'i verdiysen:
        if ((other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Rig")) && isPlayerExit == true && SahneYonetici.isFireStart  ==true)
        {
            // Yangın Durum Yöneticisine bakıyoruz
            if (YanginDurumYonetici.Instance != null)
            {
                // EĞER yangın kontrolden çıkmışsa ve oyuncu buraya kaçtıysa: İYİ/DOĞRU DAVRANIŞ BİTİŞİ
                // EĞER yangın hala ufaksa ve bodoslama kaçtıysa: KÖTÜ/KORKAK DAVRANIŞ BİTİŞİ
                
                // Kod durum yöneticisindeki isKontroldenCikti değişkenine göre kararı otomatik verir:
                if (GameObject.FindGameObjectsWithTag("fire").Length >= YanginDurumYonetici.Instance.maxFireCountThreshold)
                {
                    audio.PlayOneShot(charaterRunFire);

                    YanginDurumYonetici.Instance.SimulasyonuBitir(true, "Doğru Karar! Kontrolden çıkan yangından başarıyla tahliye oldunuz.");

                }
                else
                {
                    if (YanginDurumYonetici.isSceneSuccessful == false)
                    {                    audio.PlayOneShot(failEarlyFire);

                        
                    }
    
                    YanginDurumYonetici.Instance.SimulasyonuBitir(false, "Başarısız! Yangın hala söndürülebilirdi, alanı erken terk ettiniz.");
                }
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