using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden Çýkma Eþikleri")]
    [Tooltip("Sahnede ayný anda bulunabilecek maksimum aktif ateþ odaðý sayýsý. Bunu aþarsa yangýn kontrolden çýkar.")]
    public int maxFireCountThreshold = 15;

    [Header("Referanslar")]
    public SceneManager simulasyonYonetici; // Daha önce yazdýðýmýz yönetici

    [Header("Sesli Alarm Ayarlarý")]
    [Tooltip("Yangýn kontrolden çýktýðýnda çalacak siren veya 'Tahliye Edin' ses kaynaðý.")]
    public AudioSource sirenAudioSource;
    public AudioClip evacuationClip;

    private bool isKontroldenCikti = false;
    private float checkTimer = 0f;
    private float checkInterval = 1f; // Her saniye kontrol et (Performans dostu)

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Eðer simülasyon henüz baþlamadýysa veya yangýn zaten kontrolden çýktýysa hesaplama yapma
        if (simulasyonYonetici != null && !simulasyonYonetici.anaYanginScripti.isYanginBasladi) return;
        if (isKontroldenCikti) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            ControlEt();
        }
    }

    void ControlEt()
    {
        // 1) Sahnede o an "fire" tag'ine sahip kaç tane obje olduðunu sayýyoruz
        GameObject[] aktifAteþler = GameObject.FindGameObjectsWithTag("fire");
        int mevcutAtesSayisi = aktifAteþler.Length;

        Debug.Log($"[YANGIN DURUMU] Aktif Ateþ Odaðý Sayýsý: {mevcutAtesSayisi} / {maxFireCountThreshold}");

        // 2) Eðer mevcut ateþ sayýsý, belirlediðimiz tehlike sýnýrýný geçtiyse:
        if (mevcutAtesSayisi >= maxFireCountThreshold)
        {
            YanginKontroldenCikti();
        }
    }

    void YanginKontroldenCikti()
    {
        isKontroldenCikti = true;
        Debug.LogError("[KRÝTÝK DURUM] YANGIN KONTROLDEN ÇIKTI! DERHAL TAHLÝYE EDÝN!");

        // 1. Yeni ateþ doðmasýný engellemek için nesil limitini sýfýra çekiyoruz
        if (simulasyonYonetici != null && simulasyonYonetici.anaYanginScripti != null)
        {
            simulasyonYonetici.anaYanginScripti.maxFireGeneration = 0;
        }

        // 2. Acil Durum Sireni ve "Tahliye Edin" Sesli Uyarýsýný Tetikle
        if (sirenAudioSource != null && evacuationClip != null)
        {
            if (!sirenAudioSource.isPlaying)
            {
                sirenAudioSource.loop = true; // Sireni sürekli çal
                sirenAudioSource.clip = evacuationClip;
                sirenAudioSource.Play();
            }
        }

        // 3. VR Oyuncusuna Görev Ver
        // Burada arayüze (UI) "YANGIN KONTROLDEN ÇIKTI! GÜVENLÝ BÖLGEYE KAÇIN!" yazdýrabilirsin.
        TriggerEvacuationUI();
    }

    void TriggerEvacuationUI()
    {
        // VR ekranýnda yanýp sönen kýrmýzý bir "EVACUATE / TAHLÝYE" uyarýsý tetiklemek için harika bir yer
    }
}