using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden ??kma E?i?i")] public int maxFireCountThreshold = 15;

    [Header("Referanslar")] public SahneYonetici sahneYonetici; // SahneYonetici'ye eri?tik

    [Header("Oyun Sonu Paneli")] public GameObject oyunSonuPaneli;

    [Header("Sesli Alarm & Uyar? Ayarlar?")]
    public AudioSource sirenAudioSource;

    public AudioClip evacuationClip;

    // ? YEN? EKLEME: T?m yang?nlar bitti?inde ?alacak ba?ar? ses kayd?
    [Tooltip("Tebrikler yang?n? s?nd?rd?n?z, s?nav mod?l?ne ge?ebilirsiniz ses klibi.")]
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
        Debug.Log("[TEST] Awake ?al??t?, Instance null mu: " + (Instance == null));

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
            // Sahnede 'fire' tag'ine sahip t?m nesneleri say?yoruz
            GameObject[] aktifAtesler = GameObject.FindGameObjectsWithTag("fire");
            int mevcutAtesSayisi = aktifAtesler.Length;

            Debug.Log($"[YANGIN KONTROL] Sahnede kalan toplam ate? nesnesi say?s?: {mevcutAtesSayisi}");

            // ? MUHTE?EM TRICK: Normalde 0 olmas? gerekirdi ama sahnede 1 tane orijinal/ana nesne 
            // her zaman kalaca?? i?in, s?n?r de?erimizi 1 (veya daha k???k) yap?yoruz!
            if (mevcutAtesSayisi <= 2 && SahneYonetici.isFireStart == true)
            {
                // Debug.Log("[YANGIN KONTROL] Sadece 1 (Ana) ate? kald?, yani oyuncu t?m k???k ate?leri s?nd?rd?! Ba?ar? tetikleniyor...");
                Debug.LogWarning("sahne bitti;;;;;");
                // Tebrikler sesini bas ve paneli a?an fonksiyonunu ?a??r
                isSceneSuccessful = true;
                SimulasyonuBitir(true, "Tebrikler! Yang?n? tamamen s?nd?rd?n?z. S?nav mod?l?ne ge?ebilirsiniz.", true);
                return;
            }

            // --- KONTROLDEN ?IKMA BA?LANGICI (Eski mant???n aynen korunsun) ---
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

            // ? NULL KONTROL? EKLEND?
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
                // ? NULL KONTROL? EKLEND?
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.PuanEkle(80);

                if (isExamScene == false)
                    sirenAudioSource.PlayOneShot(fireSuccessClip);
            }
        }
    }
