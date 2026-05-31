using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden Çýkma Eþiði")]
    public int maxFireCountThreshold = 15;

    [Header("Referanslar")]
    public SahneYonetici simulasyonYonetici;

    [Header("Oyun Sonu Ayarlarý (? YENÝ)")]
    [Tooltip("Simülasyon iyi veya kötü bittiðinde AKTÝF (True) yapýlacak olan Game Object (Örn: Sonuç Paneli).")]
    public GameObject oyunSonuPaneli;

    [Header("Sesli Alarm Ayarlarý")]
    public AudioSource sirenAudioSource;
    public AudioClip evacuationClip;

    private bool isKontroldenCikti = false;
    private bool isOyunBitti = false; // ? Çift tetiklenmeyi önlemek için kilit
    private float checkTimer = 0f;
    private float checkInterval = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Simülasyon baþlamadýysa veya zaten bittiyse kontrol etme
        if (simulasyonYonetici != null && !simulasyonYonetici.anaYanginScripti.isYanginBasladi) return;
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

        // --- ? DURUM 1: ÝYÝ BÝTÝÞ (Tüm yangýnlar söndürüldü) ---
        // Eðer ilk nesil doðduktan sonra sahnede hiç ateþ kalmadýysa oyuncu kazanmýþtýr
        if (mevcutAtesSayisi == 0)
        {
            SimulasyonuBitir(true, "Tebrikler! Yangýný tamamen söndürdünüz.");
            return;
        }

        // --- ? DURUM 2: KONTROLDEN ÇIKMA BAÞLANGICI ---
        if (!isKontroldenCikti && mevcutAtesSayisi >= maxFireCountThreshold)
        {
            YanginKontroldenCikti();
        }
    }

    void YanginKontroldenCikti()
    {
        isKontroldenCikti = true;
        Debug.LogError("[KRÝTÝK] Yangýn kontrolden çýktý!");

        if (simulasyonYonetici != null && simulasyonYonetici.anaYanginScripti != null)
        {
            simulasyonYonetici.anaYanginScripti.maxFireGeneration = 0;
        }

        if (sirenAudioSource != null && evacuationClip != null)
        {
            if (!sirenAudioSource.isPlaying)
            {
                sirenAudioSource.loop = true;
                sirenAudioSource.clip = evacuationClip;
                sirenAudioSource.Play();
            }
        }
        
        // Not: Burada oyun hemen BÝTMÝYOR. Oyuncunun güvenli bölgeye kaçmasýný bekliyoruz!
    }

    /// <summary>
    /// ? ÝYÝ veya KÖTÜ senaryo bittiðinde çaðrýlan MERKEZÝ FONKSÝYON
    /// </summary>
    public void SimulasyonuBitir(bool iyiBitisMi, string bitisMesaji)
    {
        if (isOyunBitti) return; // Eðer zaten bittiyse bir daha çalýþma
        isOyunBitti = true;

        Debug.Log($"[SÝMÜLASYON BÝTTÝ] Sonuç: {(iyiBitisMi ? "BAÞARILI" : "BAÞARISIZ")} | Mesaj: {bitisMesaji}");

        // 1) ? ÝSTEDÝÐÝN NESNEYÝ TRUE YAPMA ANI:
        if (oyunSonuPaneli != null)
        {
            oyunSonuPaneli.SetActive(true);
        }

        // 2) Eðer siren çalýyorsa durdur
        if (sirenAudioSource != null && sirenAudioSource.isPlaying)
        {
            sirenAudioSource.Stop();
        }

        // Opsiyonel: Buraya skor hesaplama veya VR ellerini kilitleme kodlarý gelebilir.
    }
}