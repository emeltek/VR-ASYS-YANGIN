using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverTimeAndSpawnFires : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject Object;
    [HideInInspector] public int myGeneration = 0; // 0 = ana ateþ
    [Header("Büyüme Ayarlarý")]
    public float scaleSpeed = 0.05f;
    public float maxScaleX = 2.0f;
    public float maxScaleZ = 2.0f;
    public float maxScaleY = 1.2f;

    [Header("VFX")]
    public ParticleSystem smokeEffect;

    [Header("Küçük Ateþ Prefab Ayarlarý")]
    public GameObject[] smallFirePrefabs;
    public int numberOfSmallFires = 3;
    public float spawnRadius = 4f;

    [Tooltip("Ateþlerin spawn olma dalgalarý arasýndaki bekleme süresi (Saniye).")]
    public float spawnDelay = 3f;

    [Header("Zamanlama Ayarlarý")]
    public float fadeInTime = 2f;
    public float growthTime = 5f;
    public float smallFireGrowthTime = 12f;

    [Header("Küçük Ateþ Büyüme Hýzý ve Maks Ölçek")]
    public float smallFireGrowthSpeed = 0.5f;
    public float smallFireMaxScaleX = 1.0f;
    public float smallFireMaxScaleY = 0.8f;
    public float smallFireMaxScaleZ = 1.0f;

    [Header("Spawn Eþiði ve Nesil Limitleri")]
    [Range(0.0f, 1.0f)] public float spawnThresholdPercentage = 0.85f;
    public int maxFireGeneration = 5;

    [Header("Rüzgar ve Yok Olma")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0.0f, 1.0f)] public float destroyProbability = 0.5f;

    [Tooltip("Ateþlerin rüzgar yönünden ne kadar sapabileceðini belirler.")]
    [Range(0f, 1f)] public float windSpreadingCone = 0.2f;

    [Header("Küçük Ateþ Collider Ayarlarý")]
    public Vector3 smallFireColliderSize = new Vector3(1f, 0.5f, 1f);

    [Header("Terrain Layer Ayarý")]
    public LayerMask terrainLayer;

    private Vector3 initialScale;
    private bool hasSpawnedFirstGeneration = false;

    [Header("Sistem Kontrolü")]
    public bool isYanginBasladi = false;
    [HideInInspector] public bool isExtinguishing = false;

    void Awake()
    {
        if (Object == null)
            Object = this.gameObject;
    }

    void Start()
    {
        if (Object != null)
            initialScale = Object.transform.localScale;
    }

    void Update()
    {
        if (Object == null || !isYanginBasladi)
        {
            Debug.Log("[DEBUG] Update atlandý — Object null: " + (Object == null) + " | isYanginBasladi: " + isYanginBasladi);
            return;
        }
        // ? isExtinguishing true ise büyütme kodunu tamamen atla
        // Flag'i burada false yapmýyoruz, FireWaterCollision yönetiyor
        if (isExtinguishing) return;

        Vector3 current = Object.transform.localScale;
        float delta = scaleSpeed * Time.deltaTime;

        current.x = Mathf.Min(current.x + delta, maxScaleX);
        current.y = Mathf.Min(current.y + delta, maxScaleY);
        current.z = Mathf.Min(current.z + delta, maxScaleZ);

        Object.transform.localScale = current;

        if (!hasSpawnedFirstGeneration && current.x >= maxScaleX * spawnThresholdPercentage)
        {
            if (myGeneration == 0)
            {
                Debug.Log("Ana ateþ büyüme eþiðine ulaþtý. Ýlk nesil küçük ateþleri doðuruyor...");
                hasSpawnedFirstGeneration = true;
                StartCoroutine(SpawnSmallFires(this, 1));
            }
            else
            {
                hasSpawnedFirstGeneration = true;
            }
        }

        if (smokeEffect != null)
        {
            var main = smokeEffect.main;
            main.startSize = Mathf.Lerp(0.5f, 2.0f, Object.transform.localScale.x / maxScaleX);
        }
    }
    public IEnumerator SpawnSmallFires(ScaleOverTimeAndSpawnFires parentFire, int generation)
    {
        if (generation > maxFireGeneration)
        {
            Debug.LogWarning($"[YANGIN IPTAL] Jenerasyon sýnýrý aþýldý! Mevcut Nesil: {generation}, Sýnýr: {maxFireGeneration}");
            yield break;
        }

        if (smallFirePrefabs == null || smallFirePrefabs.Length == 0) yield break;
        if (parentFire == null || parentFire.Object == null) yield break;

        Vector3 parentPosition = parentFire.Object.transform.position;
        float baseRadius = spawnRadius;
        List<Vector3> spawnPositions = new List<Vector3>();

        Vector3 normalizedWind = windDirection.normalized;
        normalizedWind.y = 0;

        Debug.Log($"<color=cyan>[YANGIN BAÞLADI]</color> {parentFire.gameObject.name} nesnesi, Nesil {generation} için {numberOfSmallFires} adet ateþ doðurma döngüsüne giriyor.");

        for (int i = 0; i < numberOfSmallFires; i++)
        {
            if (parentFire == null || parentFire.Object == null) yield break;

            Vector3 spawnPosition = Vector3.zero;
            bool validPosition = false;
            int attempts = 0;
            int raycastIskalamaSayisi = 0;
            int mesafeTakilmaSayisi = 0;

            do
            {
                Vector3 randomAngleOffset = new Vector3(
                    Random.Range(-windSpreadingCone, windSpreadingCone),
                    0f,
                    Random.Range(-windSpreadingCone, windSpreadingCone)
                );
                Vector3 finalSpreadDirection = (normalizedWind + randomAngleOffset).normalized;

                float distanceMultiplier = 1f + (attempts * 0.05f);
                float randomDistance = Random.Range(baseRadius * 0.5f, baseRadius * distanceMultiplier);
                Vector3 offset = finalSpreadDirection * randomDistance;

                spawnPosition = parentPosition + offset;

                RaycastHit hit;
                if (Physics.Raycast(spawnPosition + Vector3.up * 100f, Vector3.down, out hit, 200f, terrainLayer))
                {
                    // ? DÜZELTME 1: Layer kontrolü kaldýrýldý, sadece "terrain" tag kontrolü yapýlýyor.
                    // Projende terrain tag'i zaten "terrain" olarak ayarlý, bu yeterli.
                    if (hit.collider.CompareTag("terrain"))
                    {
                        spawnPosition = hit.point;
                        validPosition = true;
                    }
                    else
                    {
                        // Tag terrain deðil ama bir þeye çarptý — iskala say
                        raycastIskalamaSayisi++;
                    }
                }
                else
                {
                    raycastIskalamaSayisi++;
                }

                if (validPosition)
                {
                    foreach (Vector3 pos in spawnPositions)
                    {
                        if (Vector3.Distance(pos, spawnPosition) < 0.3f)
                        {
                            validPosition = false;
                            mesafeTakilmaSayisi++;
                            break;
                        }
                    }
                }

                attempts++;
            }
            while (!validPosition && attempts < 25);

            if (!validPosition)
            {
                Debug.LogError($"<color=red>[DOÐUM BAÞARISIZ]</color> Nesil {generation}, {i}. ateþ için 25 denemede yer BULAMADI! " +
                               $"Sebep -> Raycast Iskasý: {raycastIskalamaSayisi}, Diðer Ateþlere Yakýnlýk Engeli: {mesafeTakilmaSayisi}");
                continue;
            }

            spawnPositions.Add(spawnPosition);

            GameObject selectedPrefab = smallFirePrefabs[Random.Range(0, smallFirePrefabs.Length)];
            GameObject smallFire = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            smallFire.tag = "fire";
            smallFire.name = $"Ates_Nesil_{generation}_Id_{i}_{Random.Range(100, 999)}";

            ScaleOverTimeAndSpawnFires childScript = smallFire.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (childScript != null)
            {
                // ? DÜZELTME 2: Tüm ayarlarý parent'tan child'a kopyalýyoruz
                childScript.windDirection         = this.windDirection;
                childScript.windSpreadingCone     = this.windSpreadingCone;
                childScript.maxFireGeneration     = this.maxFireGeneration;
                childScript.smallFirePrefabs      = this.smallFirePrefabs;
                childScript.numberOfSmallFires    = this.numberOfSmallFires;
                childScript.spawnRadius           = this.spawnRadius;
                childScript.spawnDelay            = this.spawnDelay;
                childScript.terrainLayer          = this.terrainLayer;
                childScript.spawnThresholdPercentage = this.spawnThresholdPercentage;
                childScript.smallFireGrowthSpeed  = this.smallFireGrowthSpeed;
                childScript.smallFireGrowthTime   = this.smallFireGrowthTime;
                childScript.smallFireMaxScaleX    = this.smallFireMaxScaleX;
                childScript.smallFireMaxScaleY    = this.smallFireMaxScaleY;
                childScript.smallFireMaxScaleZ    = this.smallFireMaxScaleZ;
                childScript.smallFireColliderSize = this.smallFireColliderSize;
                childScript.fadeInTime            = this.fadeInTime;
                childScript.destroyProbability    = this.destroyProbability;
                childScript.isYanginBasladi       = true;
                childScript.myGeneration = generation; // ? bunu ekle
            }

            Debug.Log($"<color=green>[DOÐUM BAÞARILI]</color> {smallFire.name} baþarýyla {spawnPosition} konumunda oluþturuldu.");

            // ? DÜZELTME 3: Coroutine'i child'ýn kendi scripti üzerinde baþlatýyoruz
            if (childScript != null)
                childScript.StartCoroutine(childScript.FadeInAndScaleOverTime(smallFire, generation));
            else
                StartCoroutine(FadeInAndScaleOverTime(smallFire, generation));

            yield return new WaitForSeconds(0.4f);
        }
    }
public IEnumerator FadeInAndScaleOverTime(GameObject smallFire, int generation)
{
    if (smallFire == null) yield break;

    float elapsedTime = 0f;
    smallFire.transform.localScale = Vector3.zero;

    // Fade-in bekleme süresi
    while (elapsedTime < fadeInTime)
    {
        if (smallFire == null) yield break;
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    elapsedTime = 0f;
    Vector3 targetScale = new Vector3(smallFireMaxScaleX, smallFireMaxScaleY, smallFireMaxScaleZ);
    bool hasSpawnedNewFires = false;

    if (smallFire == null) yield break;

    // Collider ekle
    BoxCollider boxCollider = smallFire.GetComponent<BoxCollider>();
    if (boxCollider == null)
    {
        boxCollider = smallFire.AddComponent<BoxCollider>();
    }
    boxCollider.isTrigger = true;
    boxCollider.size = smallFireColliderSize;
    boxCollider.center = new Vector3(0.09f, 0.3f, 0f);

    // Rigidbody ekle
    Rigidbody rb = smallFire.GetComponent<Rigidbody>();
    if (rb == null)
    {
        rb = smallFire.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    ParticleSystem localSmokeEffect = smallFire.GetComponentInChildren<ParticleSystem>();
    if (localSmokeEffect != null)
    {
        var main = localSmokeEffect.main;
        main.startSize = 0.5f;
    }

    while (elapsedTime < smallFireGrowthTime)
    {
        if (smallFire == null) yield break;

        // ? SÖNDÜRME KONTROLÜ: Su deðiyorsa bu kareyi atla, büyütme
        ScaleOverTimeAndSpawnFires selfScript = smallFire.GetComponent<ScaleOverTimeAndSpawnFires>();
        if (selfScript != null && selfScript.isExtinguishing)
        {
            yield return null;
            continue;
        }

        elapsedTime += Time.deltaTime * smallFireGrowthSpeed;
        float progress = Mathf.Clamp01(elapsedTime / smallFireGrowthTime);
        smallFire.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

        if (localSmokeEffect != null)
        {
            var main = localSmokeEffect.main;
            main.startSize = Mathf.Lerp(0.5f, 1.5f, progress);
        }

        if (!hasSpawnedNewFires && progress >= spawnThresholdPercentage)
        {
            hasSpawnedNewFires = true;

            ScaleOverTimeAndSpawnFires childScript = smallFire != null
                ? smallFire.GetComponent<ScaleOverTimeAndSpawnFires>()
                : null;

            if (childScript != null)
            {
                yield return new WaitForSeconds(spawnDelay);

                if (smallFire != null && childScript != null)
                {
                    Debug.Log($"<color=orange>[NESÝL TETIKLENDI]</color> {smallFire.name} nesnesi Nesil {generation + 1} ateþlerini doðuruyor.");
                    childScript.StartCoroutine(childScript.SpawnSmallFires(childScript, generation + 1));
                }
            }
        }

        yield return null;
    }

        if (smallFire == null) yield break;
        smallFire.transform.localScale = targetScale;

        // Belirli bir olasýlýkla ateþi yok et
        if (Random.value < destroyProbability)
        {
            float fireLifetime = 10f + (generation * 2f);
            yield return new WaitForSeconds(fireLifetime);

            if (smallFire != null)
                Destroy(smallFire);
        }
    }
}