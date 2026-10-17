using UnityEngine;

public class YanginDurumYonetici : MonoBehaviour
{
    public static YanginDurumYonetici Instance;

    [Header("Kontrolden ??kma E?i?i")] public int maxFireCountThreshold = 15;

    [Header("Tahliye Puani (mudahale + telefona gore bant)")]
    [Tooltip("Sondurmeye calisti VE telefonla yardim istedi: Orta Riskli (x = en dusuk, y = en yuksek).")]
    public Vector2Int tahliyeTamBandi = new Vector2Int(60, 79);
    [Tooltip("Ikisinden sadece birini yapti (sondurmeye calisti YA DA telefon etti): Riskli.")]
    public Vector2Int tahliyeKismiBandi = new Vector2Int(40, 59);
    [Tooltip("Hicbir sey yapmadi, izleyip kacti: Cok Riskli.")]
    public Vector2Int tahliyePasifBandi = new Vector2Int(0, 39);

    [Header("Mudahale Kontrolu")]
    [Tooltip("En az bu kadar ates sondurduyse mudahale etti sayilir.")]
    public int mudahaleIcinMinSondurulenAtes = 1;
    [Tooltip("Ya da toplam en az bu kadar saniye sondurucu sikti ise mudahale etti sayilir.")]
    public float mudahaleIcinMinSikmaSuresi = 3f;

    // Mudahale takibi (FireWaterCollision, VRExtinguisherSpray ve PhoneController bildirir)
    private readonly System.Collections.Generic.HashSet<int> sondurulenAtesler = new System.Collections.Generic.HashSet<int>();
    public int SondurulenAtesSayisi => sondurulenAtesler.Count;
    public float ToplamSikmaSuresi { get; private set; }
    public bool IsTelefonEdildi { get; private set; }
    public bool IsMudahaleEtti => SondurulenAtesSayisi >= mudahaleIcinMinSondurulenAtes
                                  || ToplamSikmaSuresi >= mudahaleIcinMinSikmaSuresi;

    public void AtesSonduruldu(GameObject ates)
    {
        if (ates != null && sondurulenAtesler.Add(ates.GetInstanceID()))
            Debug.Log($"[MUDAHALE] Ates sonduruldu. Toplam: {SondurulenAtesSayisi}");
    }

    public void SikmaSuresiEkle(float sure) => ToplamSikmaSuresi += sure;

    private bool isTahliyeIleBitti = false;

    public void TelefonEdildi()
    {
        if (IsTelefonEdildi) return;
        IsTelefonEdildi = true;

        // Tahliyeden sonra guvenli alandan yardim istemek de dogru davranis: puani yeniden hesapla
        if (isOyunBitti && isTahliyeIleBitti)
        {
            Debug.Log("[TAHLIYE] Tahliye sonrasi telefonla yardim istendi, puan yeniden hesaplaniyor.");
            TahliyePuaniniHesapla();
        }
    }

    /// <summary>Senaryo bittiyse true (sonuc ekrani acildi, puan kesinlesti).</summary>
    public bool IsOyunBitti => isOyunBitti;

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
                      if(isExamScene == false)
                                        {
                                        sirenAudioSource.Play();
                                            
                                        }
                }
            }
        }

        public void SimulasyonuBitir(bool iyiBitisMi, string bitisMesaji, bool isSonduruldu = false, bool isTahliye = false)
        {
            if (isOyunBitti) return;
            isOyunBitti = true;

            if (scoreUI != null)
                scoreUI.SetActive(true);

            // ? NULL KONTROL? EKLEND?
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ShowFinalScore();
            }

            if (oyunSonuPaneli != null)
                oyunSonuPaneli.SetActive(true);

            if (sirenAudioSource != null && sirenAudioSource.isPlaying)
                sirenAudioSource.Stop();

            if (isSonduruldu)
            {
                // Puan ses ayarlarindan bagimsiz verilir
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.PuanEkle(80);

                if (isExamScene == false && sirenAudioSource != null && fireSuccessClip != null)
                    sirenAudioSource.PlayOneShot(fireSuccessClip);
            }

            KnnDangerZoneSystem knn = KnnDangerZoneSystem.Instance;
            if (knn != null)
                knn.enabled = false; // Senaryo bitti, artik ceza islemesin

            if (ScoreManager.Instance == null) return;

            if (isTahliye)
            {
                isTahliyeIleBitti = true;
                TahliyePuaniniHesapla();
            }
            else if (knn != null)
            {
                // KNN konum cezasi (en fazla maxPenalty) senaryo sonunda bir kere dusulur.
                // +80 eklendikten sonra dusulur ki ScoreManager'in 0 siniri cezayi yutmasin.
                int konumCezasi = Mathf.RoundToInt(knn.TotalPenalty);
                Debug.Log($"[KNN] Konum cezasi: -{konumCezasi} (sinir: {knn.maxPenalty})");
                if (konumCezasi > 0)
                    ScoreManager.Instance.PuanDus(konumCezasi);
            }
        }

        // Tahliye puani: bant, yapilanlara gore secilir; bant icindeki yeri konum davranisi belirler.
        //   sondurmeye calisti + telefon -> Orta Riskli
        //   sadece biri               -> Riskli
        //   hicbiri                   -> Cok Riskli
        // Telefon tahliyeden sonra edilirse TelefonEdildi() bunu tekrar cagirir.
        void TahliyePuaniniHesapla()
        {
            if (ScoreManager.Instance == null) return;

            Vector2Int bant;
            string durum;
            if (IsMudahaleEtti && IsTelefonEdildi)       { bant = tahliyeTamBandi;   durum = "mudahale + telefon"; }
            else if (IsMudahaleEtti || IsTelefonEdildi)  { bant = tahliyeKismiBandi; durum = IsMudahaleEtti ? "sadece mudahale" : "sadece telefon"; }
            else                                         { bant = tahliyePasifBandi; durum = "pasif"; }

            KnnDangerZoneSystem knn = KnnDangerZoneSystem.Instance;
            float konumPuani = knn != null ? knn.LocationScore : 0f;
            float konumMax = knn != null ? knn.maxPenalty : 0f;
            float basari = konumMax > 0f ? Mathf.Clamp01(konumPuani / konumMax) : 1f;
            int tahliyePuani = Mathf.RoundToInt(Mathf.Lerp(bant.x, bant.y, basari));

            Debug.Log($"[TAHLIYE] Durum: {durum} (sondurulen: {SondurulenAtesSayisi}, sikma: {ToplamSikmaSuresi:F1} sn, telefon: {IsTelefonEdildi}) " +
                      $"| konum: {konumPuani:F0}/{konumMax:F0} | basari: {basari:F2} -> puan: {tahliyePuani}");
            ScoreManager.Instance.PuanAyarla(tahliyePuani);
        }
    }
