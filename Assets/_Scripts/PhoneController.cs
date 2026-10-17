using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    public AudioSource audio;
    public bool isExamScene = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            bool oyunBitti = YanginDurumYonetici.Instance != null && YanginDurumYonetici.Instance.IsOyunBitti;

            // Senaryo bittiyse +20 eklenmez; tahliye sonrasi telefonu YanginDurumYonetici puanlar
            if (!oyunBitti)
            {
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.PuanEkle(20);
                else
                    Debug.LogWarning("[PhoneController] ScoreManager.Instance null!");
            }

            // Tahliye puaninda "yardim istedi mi" kontrolu icin
            if (YanginDurumYonetici.Instance != null)
                YanginDurumYonetici.Instance.TelefonEdildi();

            if (isExamScene == false && audio != null)
                audio.Play();

            gameObject.SetActive(false);
        }
    }
}