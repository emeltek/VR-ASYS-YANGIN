using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

            // Puan kesme artýk KnnDangerZoneSystem'de ("Yangýn" bölgesi).
            // Ateþe yaklaþma cezasý orada hesaplanýr ve senaryo sonunda bir kere düþülür.
        }
    }
}