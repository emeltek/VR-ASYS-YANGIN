using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden Çýkma Eþiði")]
    public int maxFireCountThreshold = 15;

    [Header("Referanslar")]
    public SahneYonetici sahneYonetici; // SahneYonetici'ye eriþtik

    [Header("Oyun Sonu Paneli")]
    public GameObject oyunSonuPaneli;

    [Header("Sesli Alarm & Uyarý Ayarlarý")]
    public AudioSource sirenAudioSource;
    public AudioClip evacuationClip;
    
    // ? YENÝ EKLEME: Tüm yangýnlar bittiðinde çalacak baþarý ses kaydý
    [Tooltip("Tebrikler yangýný söndürdünüz, sýnav modülüne geçebilirsiniz ses klibi.")]
    public AudioClip fireSuccessClip; 

    private bool isKontroldenCikti = false;
    private bool isOyunBitti = false;
    private float checkTimer = 0f;
    private float checkInterval = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (sahneYonetici != null && !SahneYonetici.isFireStart) return;
        if (isOyunBitti) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            ControlEt();
        }
    }
    void ControlEt()
    {
        GameObject[] aktifAtesler = GameObject.FindGameObjectsWithTag("fire");
        int mevcutAtesSayisi = aktifAtesler.Length;

        // ? HATA BULMA (DEBUG): Eðer hala ateþ var diyorsa, o ateþlerin isimlerini Console'a yazdýr
        if (mevcutAtesSayisi > 0)
        {
            foreach (GameObject ates in aktifAtesler)
            {
                // Console'da hangi objenin "fire" tag'i yüzünden oyunu bitirmediðini göreceðiz
                Debug.Log($"[YANGIN KONTROL] Sahnede hala sönmemiþ ateþ var! Objeni Adý: {ates.name} | Mevcut Ölçeði: {ates.transform.localScale.x}");
            }
        }

        // --- DURUM 1: TÜM YANGINLAR BAÞARIYLA SÖNDÜRÜLDÜ ---
        if (mevcutAtesSayisi == 0)
        {
            SimulasyonuBitir(true, "Tebrikler! Yangýný tamamen söndürdünüz. Sýnav modülüne geçebilirsiniz.", true);
            return;
        }

        // --- DURUM 2: KONTROLDEN ÇIKMA BAÞLANGICI ---
        if (!isKontroldenCikti && mevcutAtesSayisi >= maxFireCountThreshold)
        {
            YanginKontroldenCikti();
        }
    }

    void YanginKontroldenCikti()
    {
        isKontroldenCikti = true;
        if (sirenAudioSource != null && evacuationClip != null)
        {
            if (!sirenAudioSource.isPlaying)
            {
                sirenAudioSource.loop = true;
                sirenAudioSource.clip = evacuationClip;
                sirenAudioSource.Play();
            }
        }
    }

    // ? Fonksiyon parametresine 'isSonduruldu' kontrolü ekledik
    public void SimulasyonuBitir(bool iyiBitisMi, string bitisMesaji, bool isSonduruldu = false)
    {
        if (isOyunBitti) return;
        isOyunBitti = true;

        Debug.Log($"[SAHNE BÝTTÝ] {bitisMesaji}");

        if (oyunSonuPaneli != null)
        {
            oyunSonuPaneli.SetActive(true);
        }

        if (sirenAudioSource != null && sirenAudioSource.isPlaying)
        {
            sirenAudioSource.Stop();
        }

        // ? YENÝ: Eðer yangýn baþarýyla söndürülerek bittiyse baþarý anonsunu çal
        if (isSonduruldu && sirenAudioSource != null && fireSuccessClip != null)
        {
            sirenAudioSource.PlayOneShot(fireSuccessClip);
        }
    }
}