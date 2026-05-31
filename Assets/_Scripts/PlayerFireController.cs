using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFireController : MonoBehaviour
{
    [Header("Ses Ayarlarý")]
    public AudioSource audioSource;
   
    [Header("Mod Ayarý")]
    public bool isExamScene = false;

    [Header("Süre Kontrolü (? YENÝ)")]
    [Tooltip("Sesli uyarýnýn kaç saniyede bir tekrarlanabileceðini belirler.")]
    public float warningCooldown = 10f; // 10 saniyelik kesin fren süresi
    private float nextWarningTime = 0f; // Bir sonraki uyarýnýn çalabileceði oyun zamaný

    private void OnTriggerEnter(Collider other)
    {
        // Eðer girdiðimiz nesnenin tag'i "fire" ise
        if (other.CompareTag("fire"))
        {
            // 1) ? 10 SANÝYELÝK SÜRE KONTROLÜ
            // Eðer oyunun o anki süresi (Time.time) izin verilen zamana ulaþtýysa veya geçtiyse ses çalsýn
            if (Time.time >= nextWarningTime)
            {
                // Bir sonraki çalma zamanýný tam 10 saniye sonrasýna kilitliyoruz
                nextWarningTime = Time.time + warningCooldown;

                // Sadece eðitim sahnesindeysek ses çalsýn (Sýnav sahnesinde çalmaz)
                if (isExamScene == false)
                {
                    if (audioSource != null)
                    {
                        audioSource.Play();
                        Debug.Log("[ÝSG UYARI] Oyuncu ateþe girdi, ses çalýndý. 10 saniye kilit aktif.");
                    }
                }
            }

            // 2) PUAN KESME MANTIÐI
            // Puan kesme iþlemi süre kilidine takýlmaz; oyuncu her ateþe deðdiðinde -35 puaný anýnda yer.
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.totalScore -= 35;
                Debug.Log("[CEZA] Ateþe çok yaklaþýldý! -35 Puan.");
            }
        }
    }
}