using UnityEngine;

public class VRExtinguisherPin : MonoBehaviour
{
    [Header("Mesafe Ayarlarý")]
    [Tooltip("Pimin yuvadan çýkýp yok olmasý için gereken kesin çekme mesafesi (Artý deðer yazýn).")]
    public float targetPullDistance = 0.025f; // ? Ýdeal hassasiyet sýnýrý

    [Header("Referanslar")]
    public VRExtinguisherSpray sprayScript;

    private Vector3 initialLocalPosition;
    private bool isPinPulled = false;

    void Start()
    {
        // Pimin oyun baþýndaki orijinal yerel konumunu kaydediyoruz
        initialLocalPosition = transform.localPosition;
        isPinPulled = false;
    }

    void Update()
    {
        if (isPinPulled) return;

        // ? Hareketi mutlak deðerden çýkarýp net yön analizine döküyoruz:
        // Orijinal X konumundan ne kadar eksiye (-X yönüne) doðru kaydýðýný ölçüyoruz
        float currentPullX = initialLocalPosition.x - transform.localPosition.x;

        // Oyun baþýnda burasý 0 çýkacaktýr. Ne zaman ki oyuncu pimi tutup eksi yöne çekerse:
        if (currentPullX >= targetPullDistance)
        {
            PinCikart();
        }
    }

    void PinCikart()
    {
        isPinPulled = true;
        Debug.Log("[YANGIN TÜPÜ] Pim baþarýyla çekildi ve yuvadan fýrladý.");

        if (sprayScript != null)
        {
            sprayScript.isPinPulledSuccessfully = true;
        }

        // Pimin kendisini sahnede kapatýyoruz
        gameObject.SetActive(false);
    }
}