using UnityEngine;

public class VRExtinguisherPin : MonoBehaviour
{
    [Header("Mesafe Ayarlarý")]
    [Tooltip("Pimin yuvadan çýkmasý için local eksende ne kadar çekilmesi gerektiðini belirler.")]
    public float targetPullDistance = 0.2f;

    [Header("Referanslar")]
    [Tooltip("Sahnede duran sprey scriptini buraya baðlayýn.")]
    public VRExtinguisherSpray sprayScript;

    private Vector3 initialLocalPosition;
    private bool isPinPulled = false;

    void Start()
    {
        // Pimin ilk takýlý olduðu orijinal pozisyonu kaydediyoruz
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isPinPulled) return;

        // Pimin ilk konumundan ne kadar uzaklaþtýðýný (çekildiðini) local olarak hesaplýyoruz
        float currentDistance = Vector3.Distance(transform.localPosition, initialLocalPosition);

        // Eðer oyuncu pimi tutup 0.2 birim veya daha fazla çektiyse:
        if (currentDistance >= targetPullDistance)
        {
            PinCikart();
        }
    }

    void PinCikart()
    {
        isPinPulled = true;
        Debug.Log("[YANGIN TÜPÜ] Pim baþarýyla çekildi! Emniyet kilidi açýldý.");

        // 1) Sprey scriptine pimin çekildiðini haber veriyoruz
        if (sprayScript != null)
        {
            sprayScript.isPinPulledSuccessfully = true;
        }

        // 2) Pim objesini tamamen ortadan kaldýrýyoruz (yuvadan çýktý ve kayboldu)
        gameObject.SetActive(false);
    }
}