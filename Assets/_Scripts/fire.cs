using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverTimeAndSpawnFires : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject Object;
    [HideInInspector] public int myGeneration = 0; // 0 = ana ate?
    [Header("B?y?me Ayarlar?")]
    public float scaleSpeed = 0.05f;
    public float maxScaleX = 2.0f;
    public float maxScaleZ = 2.0f;
    public float maxScaleY = 1.2f;

    [Header("VFX")]
    public ParticleSystem smokeEffect;

    [Header("K???k Ate? Prefab Ayarlar?")]
    public GameObject[] smallFirePrefabs;
    public int numberOfSmallFires = 3;
    public float spawnRadius = 4f;

    [Tooltip("Ate?lerin spawn olma dalgalar? aras?ndaki bekleme s?resi (Saniye).")]
    public float spawnDelay = 3f;

    [Header("Zamanlama Ayarlar?")]
    public float fadeInTime = 2f;
    public float growthTime = 5f;
    public float smallFireGrowthTime = 12f;

    [Header("K???k Ate? B?y?me H?z? ve Maks ?l?ek")]
    public float smallFireGrowthSpeed = 0.5f;
    public float smallFireMaxScaleX = 1.0f;
    public float smallFireMaxScaleY = 0.8f;
    public float smallFireMaxScaleZ = 1.0f;

    [Header("Spawn E?i?i ve Nesil Limitleri")]
    [Range(0.0f, 1.0f)] public float spawnThresholdPercentage = 0.85f;
    public int maxFireGeneration = 5;

    [Header("R?zgar ve Yok Olma")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0.0f, 1.0f)] public float destroyProbability = 0.5f;

    [Tooltip("Ate?lerin r?zgar y?n?nden ne kadar sapabilece?ini belirler.")]
    [Range(0f, 1f)] public float windSpreadingCone = 0.2f;

    [Header("K???k Ate? Collider Ayarlar?")]
    public Vector3 smallFireColliderSize = new Vector3(1f, 0.5f, 1f);

    [Header("Terrain Layer Ayar?")]
    public LayerMask terrainLayer;

    private Vector3 initialScale;
    private bool hasSpawnedFirstGeneration = false;

    [Header("Sistem Kontrol?")]
    public bool isYanginBasladi = false;
    [HideInInspector] public bool isExtinguishing = false;

    void Awake()
    {
        if (Object == null)
            Object = this.gameObject;

        // Her ateş KNN'de otomatik bir tehlike düğümü olur
        KnnDangerNode.Ensure(Object);
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
            Debug.Log("[DEBUG] Update atland? ? Object null: " + (Object == null) + " | isYanginBasladi: " + isYanginBasladi);
            return;
        }
        // ? isExtinguishing true ise b?y?tme kodunu tamamen atla
        // Flag'i burada false yapm?yoruz, FireWaterCollision y?netiyor
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
                Debug.Log("Ana ate? b?y?me e?i?ine ula?t?. ?lk nesil k???k ate?leri do?uruyor...");
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
            Debug.LogWarning($"[YANGIN IPTAL] Jenerasyon s?n?r? a??ld?! Mevcut Nesil: {generation}, S?n?r: {maxFireGeneration}");
            yield break;
        }

        if (smallFirePrefabs == null || smallFirePrefabs.Length == 0) yield break;
        if (parentFire == null || parentFire.Object == null) yield break;

        Vector3 parentPosition = parentFire.Object.transform.position;
        float baseRadius = spawnRadius;
        List<Vector3> spawnPositions = new List<Vector3>();

        Vector3 normalizedWind = windDirection.normalized;
        normalizedWind.y = 0;

        Debug.Log($"<color=cyan>[YANGIN BA?LADI]</color> {parentFire.gameObject.name} nesnesi, Nesil {generation} i?in {numberOfSmallFires} adet ate? do?urma d?ng?s?ne giriyor.");

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
                    // ? D?ZELTME 1: Layer kontrol? kald?r?ld?, sadece "terrain" tag kontrol? yap?l?yor.
                    // Projende terrain tag'i zaten "terrain" olarak ayarl?, bu yeterli.
                    if (hit.collider.CompareTag("terrain"))
                    {
                        spawnPosition = hit.point;
                        validPosition = true;
                    }
                    else
                    {
                        // Tag terrain de?il ama bir ?eye ?arpt? ? iskala say
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
                Debug.LogError($"<color=red>[DO?UM BA?ARISIZ]</color> Nesil {generation}, {i}. ate? i?in 25 denemede yer BULAMADI! " +
                               $"Sebep -> Raycast Iskas?: {raycastIskalamaSayisi}, Di?er Ate?lere Yak?nl?k Engeli: {mesafeTakilmaSayisi}");
                continue;
            }

            spawnPositions.Add(spawnPosition);

            GameObject selectedPrefab = smallFirePrefabs[Random.Range(0, smallFirePrefabs.Length)];
            GameObject smallFire = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            smallFire.tag = "fire";
            smallFire.name = $"Ates_Nesil_{generation}_Id_{i}_{Random.Range(100, 999)}";
            KnnDangerNode.Ensure(smallFire); // Prefab'da script olmasa da KNN düğümü olsun

            ScaleOverTimeAndSpawnFires childScript = smallFire.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (childScript != null)
            {
                // ? D?ZELTME 2: T?m ayarlar? parent'tan child'a kopyal?yoruz
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

            Debug.Log($"<color=green>[DO?UM BA?ARILI]</color> {smallFire.name} ba?ar?yla {spawnPosition} konumunda olu?turuldu.");

            // ? D?ZELTME 3: Coroutine'i child'?n kendi scripti ?zerinde ba?lat?yoruz
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

    // Fade-in bekleme s?resi
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

        // ? S?ND?RME KONTROL?: Su de?iyorsa bu kareyi atla, b?y?tme
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
                    Debug.Log($"<color=orange>[NES?L TETIKLENDI]</color> {smallFire.name} nesnesi Nesil {generation + 1} ate?lerini do?uruyor.");
                    childScript.StartCoroutine(childScript.SpawnSmallFires(childScript, generation + 1));
                }
            }
        }

        yield return null;
    }

        if (smallFire == null) yield break;
        smallFire.transform.localScale = targetScale;

        // Belirli bir olas?l?kla ate?i yok et
        if (Random.value < destroyProbability)
        {
            float fireLifetime = 10f + (generation * 2f);
            yield return new WaitForSeconds(fireLifetime);

            if (smallFire != null)
                Destroy(smallFire);
        }
    }
}