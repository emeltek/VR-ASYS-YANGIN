using UnityEngine;

public class VRExtinguisherSpray : MonoBehaviour
{
    [Header("Referanslar")]
    public ParticleSystem sprayParticles; 
    public AudioSource sprayAudio;        

    [Header("Durum Kontrolleri (Editor Izleme Icýn)")]
    public bool isExtinguisherHeld = false;
    public bool isHoseHeld = false;
    public bool isSpraying = false;

    void Start()
    {
        if (sprayParticles != null)
        {
            sprayParticles.Stop();
            sprayParticles.gameObject.tag = "water";
        }

        if (sprayAudio != null)
        {
            sprayAudio.loop = true;
            sprayAudio.playOnAwake = false;
        }
    }

    void Update()
    {
        // ÞART 1: Ýki elin de nesneleri sýkýca tutuyor olmasý gerekiyor
        if (isExtinguisherHeld && isHoseHeld)
        {
            bool shouldSpray = false;

            // 1) Meta SDK Canlý Tuþ Kontrolü (OVRInput.Get active durumunu her karede sorgular)
            if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch) || 
                OVRInput.Get(OVRInput.RawButton.Y))
            {
                shouldSpray = true;
            }

            // 2) Yedek PC Editör Testi (Sol Shift veya Boþluk tuþuna basýlý tutuluyorsa)
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space))
            {
                shouldSpray = true;
            }

            // AKSIYON ZAMANI: Canlý sorgu sonucuna göre aç veya kapat
            if (shouldSpray)
            {
                if (!isSpraying) StartSpray();
            }
            else
            {
                if (isSpraying) StopSpray();
            }
        }
        else
        {
            // Ellerden biri bile býrakýlýrsa anýnda fýþkýrtmayý kes ve emniyete al
            if (isSpraying) StopSpray();
        }
    }

    void StartSpray()
    {
        isSpraying = true;
        if (sprayParticles != null) sprayParticles.Play();
        if (sprayAudio != null) sprayAudio.Play();
        Debug.Log("[Sprey Sistemi] Püskürtme BAÞLADI!");
    }

    void StopSpray()
    {
        isSpraying = false;
        if (sprayParticles != null) sprayParticles.Stop();
        if (sprayAudio != null) sprayAudio.Stop();
        Debug.Log("[Sprey Sistemi] Püskürtme DURDURULDU.");
    }

    public void SetExtinguisherHeld(bool state) => isExtinguisherHeld = state;
    public void SetHoseHeld(bool state) => isHoseHeld = state;
}