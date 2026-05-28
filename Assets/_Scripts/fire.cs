using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverTimeAndSpawnFires : MonoBehaviour
{
    [Header("Referanslar")]
    // Eðer eren atandýysa onun transform'u kullanýlacak, atanmazsa bu script'in baðlý olduðu GameObject kullanýlacak.
    public GameObject Object;

    [Header("Büyüme Ayarlarý")]
    public float scaleSpeed = 0.1f;
    public float maxScaleX = 2.0f;
    public float maxScaleZ = 2.0f;
    public float maxScaleY = 1.2f;

    [Header("VFX")]
    public ParticleSystem smokeEffect;

    [Header("Küçük Ateþ Prefab Ayarlarý")]
    public GameObject[] smallFirePrefabs;
    public int numberOfSmallFires = 5;
    public float spawnRadius = 5f;
    public float spawnDelay = 1f;

    [Header("Zamanlama Ayarlarý")]
    public float fadeInTime = 2f;
    public float growthTime = 5f;
    public float smallFireGrowthTime = 10f;

    [Header("Küçük Ateþ Büyüme Hýzý ve Maks Ölçek")]
    public float smallFireGrowthSpeed = 1.0f;
    public float smallFireMaxScaleX = 1.0f;
    public float smallFireMaxScaleY = 0.8f;
    public float smallFireMaxScaleZ = 1.0f;

    [Header("Spawn Eþiði ve Nesil Limitleri")]
    [Range(0.0f, 1.0f)] public float spawnThresholdPercentage = 0.8f;
    public int maxFireGeneration = 3;

    [Header("Rüzgar ve Yok Olma")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0.0f, 1.0f)] public float destroyProbability = 0.5f;

    [Header("Küçük Ateþ Collider Ayarlarý")]
    public Vector3 smallFireColliderSize = new Vector3(1f, 0.5f, 1f);

    [Header("Terrain Layer Ayarý")]
    public LayerMask terrainLayer;  // Terrain katmaný için LayerMask

    private Vector3 initialScale;
    private bool hasSpawnedFirstGeneration = false;

    // Eðer eren atanmamýþsa, bu script'in baðlý olduðu GameObject kullanýlýr.
    void Awake()
    {
        if (Object == null)
        {
            Object = this.gameObject;
        }
    }

    void Start()
    {
        initialScale = Object.transform.localScale;
    }

    void Update()
    {
        // Ana ateþin (eren) yavaþ yavaþ büyümesi
        Vector3 current = Object.transform.localScale;
        float delta = scaleSpeed * Time.deltaTime;

        // Her ekseni kendi max deðerine göre güncelle ve aþmayý engelle
        current.x = Mathf.Min(current.x + delta, maxScaleX);
        current.y = Mathf.Min(current.y + delta, maxScaleY);
        current.z = Mathf.Min(current.z + delta, maxScaleZ);

        Object.transform.localScale = current;

        // Ana ateþ belirli bir eþiðe ulaþtýðýnda küçük ateþleri spawn etmeye baþla
        if (!hasSpawnedFirstGeneration && current.x >= maxScaleX * spawnThresholdPercentage)
        {
            Debug.Log("Ana ateþ büyüme eþiðine ulaþtý. Küçük ateþleri spawn ediyor...");
            hasSpawnedFirstGeneration = true;
            StartCoroutine(SpawnSmallFires(this, 1));
        }


        if (smokeEffect != null)
        {
            var main = smokeEffect.main;
            main.startSize = Mathf.Lerp(0.5f, 2.0f, Object.transform.localScale.x / maxScaleX);
           
        }
    }

    /// <summary>
    /// Belirli sayýda küçük ateþ oluþturur. Yalnýzca terrainLayer'a ait yüzeye raycast yapar.
    /// </summary>
    public IEnumerator SpawnSmallFires(ScaleOverTimeAndSpawnFires parentFire, int generation)
    {
        // Maks nesil kontrolü
        if (generation > maxFireGeneration) yield break;

        // Küçük ateþ prefab’leri var mý kontrolü
        if (smallFirePrefabs == null || smallFirePrefabs.Length == 0)
        {
            Debug.LogError("smallFirePrefabs dizisi boþ! Inspector üzerinden prefab eklediðinizden emin olun.");
            yield break;
        }

        // Ebeveyn ateþin pozisyonu (eren kullanýlýyor)
        Vector3 parentPosition = parentFire != null ? parentFire.Object.transform.position : Object.transform.position;

        // Nesile göre spawn yarýçapýný biraz artýrabilirsiniz
        float adjustedSpawnRadius = spawnRadius * (1 + generation * 0.3f);

        // Ayný pozisyona yakýn tekrar spawn olmamasý için pozisyon listesi
        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < numberOfSmallFires; i++)
        {
            Vector3 spawnPosition;
            bool validPosition = false;
            int attempts = 0;

            // Raycast ile terrain yüzeyine denk gelene kadar dene
            do
            {
                Vector3 randomOffset = Random.insideUnitSphere * adjustedSpawnRadius;
                randomOffset.y = 0; // Yükseklik deðiþikliði yok, yatay düzlemde rastgele
                spawnPosition = parentPosition + randomOffset;

                RaycastHit hit;
                // terrainLayer ile raycast, sadece bu layer'a çarpar
                if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, terrainLayer))
                {
                    // Eðer hit edilen collider'ýn tag'i "terrain" ise
                    if (hit.collider.CompareTag("terrain"))
                    {
                        spawnPosition = hit.point;
                        validPosition = true;
                    }
                }

                // Daha önceki konumlarla çok yakýnsa yine geçersiz kýl
                if (validPosition)
                {
                    foreach (Vector3 pos in spawnPositions)
                    {
                        if (Vector3.Distance(pos, spawnPosition) < spawnRadius / 2)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }

                attempts++;
            }
            while (!validPosition && attempts < 10);

            // 10 denemede de geçerli pozisyon bulunamadýysa bu ateþi atla
            if (!validPosition)
            {
                Debug.LogWarning("Geçerli terrain pozisyonu bulunamadý, bu ateþ atlanýyor.");
                continue;
            }

            // Onaylanan pozisyonu listeye ekle
            spawnPositions.Add(spawnPosition);

            // Rastgele bir küçük ateþ prefab'ý seç ve instantiate et
            GameObject selectedPrefab = smallFirePrefabs[Random.Range(0, smallFirePrefabs.Length)];
            GameObject smallFire = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            smallFire.tag = "fire"; // Tag olarak fire ayarladýk

            // Fade-in ve büyüme coroutine’ini baþlat
            StartCoroutine(FadeInAndScaleOverTime(smallFire, generation));

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Küçük ateþin fade-in yapmasý ve zamanla büyümesi, ayrýca belli eþiðe ulaþtýðýnda yeni ateþleri doðurmasý.
    /// </summary>
    private IEnumerator FadeInAndScaleOverTime(GameObject smallFire, int generation)
    {
        if (smallFire == null)
            yield break;

        float elapsedTime = 0f;
        smallFire.transform.localScale = Vector3.zero;

        // Fade-in süreci (görsel efekt amaçlý)
        while (elapsedTime < fadeInTime)
        {
            if (smallFire == null)
                yield break;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ateþin büyüme süreci
        elapsedTime = 0f;
        Vector3 targetScale = new Vector3(smallFireMaxScaleX, smallFireMaxScaleY, smallFireMaxScaleZ);
        bool hasSpawnedNewFires = false;

        // Collider ve Rigidbody'nin ekli olduðunu varsayýyoruz. Yoksa ekleyebilirsiniz.
        BoxCollider boxCollider = smallFire.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = smallFire.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = smallFireColliderSize;
        }

        Rigidbody rb = smallFire.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = smallFire.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Eðer prefab içerisinde bir duman efekti varsa onun boyutunu ayarlýyoruz.
        ParticleSystem localSmokeEffect = smallFire.GetComponentInChildren<ParticleSystem>();
        if (localSmokeEffect != null)
        {
            var main = localSmokeEffect.main;
            main.startSize = 0.5f;
        }

        // Büyüme döngüsü
        while (elapsedTime < smallFireGrowthTime)
        {
            if (smallFire == null)
                yield break;

            elapsedTime += Time.deltaTime * smallFireGrowthSpeed;
            float progress = Mathf.Clamp01(elapsedTime / smallFireGrowthTime);
            smallFire.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

            // Eðer varsa duman efektinin boyutunu ayarla
            if (localSmokeEffect != null)
            {
                var main = localSmokeEffect.main;
                main.startSize = Mathf.Lerp(0.5f, 1.5f, smallFire.transform.localScale.x / targetScale.x);
            }

            // Belirli bir ölçeðe ulaþtýðýnda yeni ateþ spawn et
            if (!hasSpawnedNewFires && smallFire.transform.localScale.x >= (targetScale.x * spawnThresholdPercentage))
            {
                hasSpawnedNewFires = true;
                StartCoroutine(SpawnSmallFires(this, generation + 1));
            }

            yield return null;
        }

        // Eðer smallFire hala var ise hedef ölçeðe ayarla
        if (smallFire != null)
            smallFire.transform.localScale = targetScale;

        // Yok olma olasýlýðý için kontrol
        if (smallFire != null && Random.value < destroyProbability)
        {
            float fireLifetime = 10f + (generation * 2f);
            yield return new WaitForSeconds(fireLifetime);

            // Yeniden null kontrolü yapýyoruz
            if (smallFire != null)
                Destroy(smallFire);
        }
    }
}