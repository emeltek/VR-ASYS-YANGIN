using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverTimeAndSpawnFires : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject Object;

    [Header("Büyüme Ayarlarý")]
    public float scaleSpeed = 0.05f; // Hýzý biraz düþürmek kontrolü artýrýr
    public float maxScaleX = 2.0f;
    public float maxScaleZ = 2.0f;
    public float maxScaleY = 1.2f;

    [Header("VFX")]
    public ParticleSystem smokeEffect;

    [Header("Küçük Ateþ Prefab Ayarlarý")]
    public GameObject[] smallFirePrefabs;
    public int numberOfSmallFires = 3; // Tek seferde çok fazla doðup kasmamasý için 3'e düþürüldü
    public float spawnRadius = 4f;
    
    [Tooltip("Ateþlerin spawn olma dalgalarý arasýndaki bekleme süresi (Saniye). Kontrol için bunu artýrýn.")]
    public float spawnDelay = 3f; // 1'den 3 saniyeye çýkarýldý (Yavaþlatma Etkisi 1)

    [Header("Zamanlama Ayarlarý")]
    public float fadeInTime = 2f;
    public float growthTime = 5f;
    public float smallFireGrowthTime = 12f; // Küçük ateþlerin olgunlaþma süresi uzatýldý (Yavaþlatma Etkisi 2)

    [Header("Küçük Ateþ Büyüme Hýzý ve Maks Ölçek")]
    public float smallFireGrowthSpeed = 0.5f; // Büyüme hýzý yarýya düþürüldü
    public float smallFireMaxScaleX = 1.0f;
    public float smallFireMaxScaleY = 0.8f;
    public float smallFireMaxScaleZ = 1.0f;

    [Header("Spawn Eþiði ve Nesil Limitleri")]
    [Range(0.0f, 1.0f)] public float spawnThresholdPercentage = 0.85f; // Daha geç doðum yapmasý için eþik yükseltildi
    public int maxFireGeneration = 3;

    [Header("Rüzgar ve Yok Olma")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0.0f, 1.0f)] public float destroyProbability = 0.5f;

    [Tooltip("Ateþlerin rüzgar yönünden ne kadar sapabileceðini belirler. (0 = Tam rüzgar oku yönü, 1 = Tamamen serbest)")]
    [Range(0f, 1f)] public float windSpreadingCone = 0.2f; // Sadece rüzgar yönü için dar bir koni

    [Header("Küçük Ateþ Collider Ayarlarý")]
    public Vector3 smallFireColliderSize = new Vector3(1f, 0.5f, 1f);

    [Header("Terrain Layer Ayarý")]
    public LayerMask terrainLayer;

    private Vector3 initialScale;
    private bool hasSpawnedFirstGeneration = false;
    [Header("Sistem Kontrolü")]
    public bool isYanginBasladi = false; // Yangýn kontrol kilidi
    [HideInInspector] public bool isExtinguishing = false; // Söndürülme aþamasýnda mý?

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
        if (!isYanginBasladi) return;

        // ? Emniyet Kilidi: Söndürme iþlemi esnasýnda büyüme hesaplamasýný atla!
        if (isExtinguishing) 
        {
            isExtinguishing = false; 
            return; 
        }
        Vector3 current = Object.transform.localScale;
        float delta = scaleSpeed * Time.deltaTime;

        current.x = Mathf.Min(current.x + delta, maxScaleX);
        current.y = Mathf.Min(current.y + delta, maxScaleY);
        current.z = Mathf.Min(current.z + delta, maxScaleZ);

        Object.transform.localScale = current;

        if (!hasSpawnedFirstGeneration && current.x >= maxScaleX * spawnThresholdPercentage)
        {
            Debug.Log("Ana ateþ büyüme eþiðine ulaþtý. Ýlk nesil küçük ateþleri rüzgar yönünde doðuruyor...");
            hasSpawnedFirstGeneration = true;
            StartCoroutine(SpawnSmallFires(this, 1));
        }

        if (smokeEffect != null)
        {
            var main = smokeEffect.main;
            main.startSize = Mathf.Lerp(0.5f, 2.0f, Object.transform.localScale.x / maxScaleX);
        }
    }

    public IEnumerator SpawnSmallFires(ScaleOverTimeAndSpawnFires parentFire, int generation)
    {
        if (generation > maxFireGeneration) yield break;

        if (smallFirePrefabs == null || smallFirePrefabs.Length == 0)
        {
            Debug.LogError("smallFirePrefabs dizisi boþ!");
            yield break;
        }

        Vector3 parentPosition = parentFire != null ? parentFire.Object.transform.position : Object.transform.position;
        float adjustedSpawnRadius = spawnRadius * (1 + generation * 0.2f);
        List<Vector3> spawnPositions = new List<Vector3>();

        // 1) Rüzgar yönünü normalize et ve Y eksenini sýfýrla (Yatay düzlemde yayýlým)
        Vector3 normalizedWind = windDirection.normalized;
        normalizedWind.y = 0;

        for (int i = 0; i < numberOfSmallFires; i++)
        {
            Vector3 spawnPosition;
            bool validPosition = false;
            int attempts = 0;

            do
            {
                // 2) KRÝTÝK DEÐÝÞÝKLÝK: 360 derece rastgelelik yerine rüzgar yönünü baz alýyoruz
                // Rüzgar yönünün etrafýna küçük bir sapma (koni efekti) ekliyoruz
                Vector3 randomAngleOffset = new Vector3(Random.Range(-windSpreadingCone, windSpreadingCone), 0f, Random.Range(-windSpreadingCone, windSpreadingCone));
                Vector3 finalSpreadDirection = (normalizedWind + randomAngleOffset).normalized;

                // Ateþi rüzgar doðrultusunda ileriye doðru fýrlatýyoruz
                float randomDistance = Random.Range(adjustedSpawnRadius * 0.5f, adjustedSpawnRadius);
                Vector3 offset = finalSpreadDirection * randomDistance;
                
                spawnPosition = parentPosition + offset;

                RaycastHit hit;
                if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, terrainLayer))
                {
                    if (hit.collider.CompareTag("terrain"))
                    {
                        spawnPosition = hit.point;
                        validPosition = true;
                    }
                }

                if (validPosition)
                {
                    foreach (Vector3 pos in spawnPositions)
                    {
                        if (Vector3.Distance(pos, spawnPosition) < spawnRadius / 2f)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }

                attempts++;
            }
            while (!validPosition && attempts < 15); // Deneme limiti artýrýldý

            if (!validPosition) continue;

            spawnPositions.Add(spawnPosition);

            GameObject selectedPrefab = smallFirePrefabs[Random.Range(0, smallFirePrefabs.Length)];
            GameObject smallFire = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            smallFire.tag = "fire";

            // Doðacak yeni küçük ateþ prefabýna da bu rüzgar verilerini aktarýyoruz ki o da rüzgara doðru büyüsün
            ScaleOverTimeAndSpawnFires childScript = smallFire.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (childScript != null)
            {
                childScript.windDirection = this.windDirection;
                childScript.windSpreadingCone = this.windSpreadingCone;
                childScript.maxFireGeneration = this.maxFireGeneration;
            }

            StartCoroutine(FadeInAndScaleOverTime(smallFire, generation));

            // Burasý her tekil küçük ateþin arasýndaki saniyeyi belirler
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator FadeInAndScaleOverTime(GameObject smallFire, int generation)
    {
        if (smallFire == null) yield break;

        float elapsedTime = 0f;
        smallFire.transform.localScale = Vector3.zero;

        while (elapsedTime < fadeInTime)
        {
            if (smallFire == null) yield break;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        Vector3 targetScale = new Vector3(smallFireMaxScaleX, smallFireMaxScaleY, smallFireMaxScaleZ);
        bool hasSpawnedNewFires = false;

        BoxCollider boxCollider = smallFire.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = smallFire.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = smallFireColliderSize;
            
        }
       
        Vector3 newCenter = boxCollider.center;

        newCenter.y = 0.81f; 

        boxCollider.center = newCenter;

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

            elapsedTime += Time.deltaTime * smallFireGrowthSpeed;
            float progress = Mathf.Clamp01(elapsedTime / smallFireGrowthTime);
            smallFire.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

            if (localSmokeEffect != null)
            {
                var main = localSmokeEffect.main;
                main.startSize = Mathf.Lerp(0.5f, 1.5f, smallFire.transform.localScale.x / targetScale.x);
            }

            // Bir sonraki nesil ateþ dalgasýnýn spawnlanma aný
            if (!hasSpawnedNewFires && smallFire.transform.localScale.x >= (targetScale.x * spawnThresholdPercentage))
            {
                hasSpawnedNewFires = true;
                
                // Dalgalar arasýnda 'spawnDelay' kadar mola vererek hýzý kýrýyoruz
                yield return new WaitForSeconds(spawnDelay);
                
                if (smallFire != null)
                {
                    StartCoroutine(SpawnSmallFires(this, generation + 1));
                }
            }

            yield return null;
        }

        if (smallFire != null)
            smallFire.transform.localScale = targetScale;

        if (smallFire != null && Random.value < destroyProbability)
        {
            float fireLifetime = 10f + (generation * 2f);
            yield return new WaitForSeconds(fireLifetime);

            if (smallFire != null)
                Destroy(smallFire);
        }
    }
}