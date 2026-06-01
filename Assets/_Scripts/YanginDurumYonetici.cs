using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden Çýkma Eþiði")] public int maxFireCountThreshold = 15;

    [Header("Referanslar")] public SahneYonetici sahneYonetici; // SahneYonetici'ye eriþtik

    [Header("Oyun Sonu Paneli")] public GameObject oyunSonuPaneli;

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
    public static bool isSceneSuccessful = false;
    public bool isExamScene = false;
    public GameObject scoreUI;
    private bool firebiggestAuidoPlay = false;

    void Awake()
    {
        Debug.Log("[TEST] Awake çalýþtý, Instance null mu: " + (Instance == null));

        if (Instance == null)
        {
            Instance = this;
            isSceneSuccessful = false;
            isOyunBitti = false;
            isKontroldenCikti = false;
            firebiggestAuidoPlay = false;
            if (scoreUI != null) scoreUI.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
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
            // Sahnede 'fire' tag'ine sahip tüm nesneleri sayýyoruz
            GameObject[] aktifAtesler = GameObject.FindGameObjectsWithTag("fire");
            int mevcutAtesSayisi = aktifAtesler.Length;

            Debug.Log($"[YANGIN KONTROL] Sahnede kalan toplam ateþ nesnesi sayýsý: {mevcutAtesSayisi}");

            // ? MUHTEÞEM TRICK: Normalde 0 olmasý gerekirdi ama sahnede 1 tane orijinal/ana nesne 
            // her zaman kalacaðý için, sýnýr deðerimizi 1 (veya daha küçük) yapýyoruz!
            if (mevcutAtesSayisi <= 2 && SahneYonetici.isFireStart == true)
            {
                // Debug.Log("[YANGIN KONTROL] Sadece 1 (Ana) ateþ kaldý, yani oyuncu tüm küçük ateþleri söndürdü! Baþarý tetikleniyor...");
                Debug.LogWarning("sahne bitti;;;;;");
                // Tebrikler sesini bas ve paneli açan fonksiyonunu çaðýr
                isSceneSuccessful = true;
                SimulasyonuBitir(true, "Tebrikler! Yangýný tamamen söndürdünüz. Sýnav modülüne geçebilirsiniz.", true);
                return;
            }

            // --- KONTROLDEN ÇIKMA BAÞLANGICI (Eski mantýðýn aynen korunsun) ---
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
                if (!sirenAudioSource.isPlaying && firebiggestAuidoPlay == false)
                {

                    firebiggestAuidoPlay = true;
                    sirenAudioSource.clip = evacuationClip;
                    sirenAudioSource.Play();
                }
            }
        }

        public void SimulasyonuBitir(bool iyiBitisMi, string bitisMesaji, bool isSonduruldu = false)
        {
            if (isOyunBitti) return;
            isOyunBitti = true;

            if (scoreUI != null)
                scoreUI.SetActive(true);

            // ? NULL KONTROLÜ EKLENDÝ
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.UpdateScoreUI();
            }

            if (oyunSonuPaneli != null)
                oyunSonuPaneli.SetActive(true);

            if (sirenAudioSource != null && sirenAudioSource.isPlaying)
                sirenAudioSource.Stop();

            if (isSonduruldu && sirenAudioSource != null && fireSuccessClip != null)
            {
                // ? NULL KONTROLÜ EKLENDÝ
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.PuanEkle(80);

                if (isExamScene == false)
                    sirenAudioSource.PlayOneShot(fireSuccessClip);
            }
        }
    }
