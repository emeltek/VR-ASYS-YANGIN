using UnityEngine;

public class FireWaterCollision : MonoBehaviour
{
    [Header("Söndürme Ayarlarý")]
    [Tooltip("Ateþin saniyede ne kadar küçüleceðini belirler.")]
    public float extinguishSpeed = 0.4f;

    [Tooltip("Ateþ bu ölçeðin altýna düþtüðünde tamamen sönmüþ sayýlýr ve yok edilir.")]
    public float destroyThreshold = 0.2f;

    [Header("Görsel Efekt Ayarlarý (Yeni)")]
    [Tooltip("Ateþ söndürülürken çýkacak olan BEYAZ parçacýk efekti (Prefab).")]
    public GameObject whiteExtinguishParticlePrefab;

    [Tooltip("Beyaz efektin kaç saniyede bir doðacaðýný belirler (Sürekli doðup kasmamasý için).")]
    public float particleSpawnInterval = 0.15f;
    private float nextSpawnTime = 0f;

    /// <summary>
    /// Hortumun ucundaki görünmez tetikleyici (isTrigger=True) ateþe deðdiði sürece çalýþýr.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("fire"))
        {
            // ? YENÝ: Çarptýðýmýz ateþin üzerindeki büyüme script'ini buluyoruz
            ScaleOverTimeAndSpawnFires fireScript = other.GetComponent<ScaleOverTimeAndSpawnFires>();
        
            if (fireScript != null)
            {
                // Ateþe "büyümeyi durdur, þu an seni söndürüyorum" talimatý veriyoruz
                fireScript.isExtinguishing = true;
            }

            // 1. Ateþi Söndürme Mantýðý (Küçültme)
            Extinguish(other.gameObject);

            // 2. Beyaz Parçacýk Efekti Tetikleme
            if (whiteExtinguishParticlePrefab != null && Time.time >= nextSpawnTime)
            {
                nextSpawnTime = Time.time + particleSpawnInterval;
                Vector3 spawnPosition = other.transform.position + Vector3.up * 0.3f; 
                GameObject whiteCloud = Instantiate(whiteExtinguishParticlePrefab, spawnPosition, Quaternion.identity);
                Destroy(whiteCloud, 1.5f);
            }
        }
    }

    /// <summary>
    /// Ateþ nesnesini yavaþ yavaþ küçültür ve sýnýrýn altýna inince yok eder.
    /// </summary>
    private void Extinguish(GameObject fireObject)
    {
        // Ateþin o anki yerel ölçeðini al
        Vector3 currentScale = fireObject.transform.localScale;

        // Her üç eksende de belirlenen hýzda küçült
        currentScale.x -= extinguishSpeed * Time.deltaTime;
        currentScale.y -= extinguishSpeed * Time.deltaTime;
        currentScale.z -= extinguishSpeed * Time.deltaTime;

        // Ölçeðin eksi deðerlere düþmesini engelle
        currentScale.x = Mathf.Max(currentScale.x, 0f);
        currentScale.y = Mathf.Max(currentScale.y, 0f);
        currentScale.z = Mathf.Max(currentScale.z, 0f);

        // Yeni ölçeði ateþe geri ata
        fireObject.transform.localScale = currentScale;

        // Eðer ateþ belirlenen eþik deðerinden daha fazla küçüldüyse, yangýn sönmüþtür!
        if (currentScale.x <= destroyThreshold)
        {
            Debug.Log("[YANGIN SÖNDÜ] Bir ateþ odaðý baþarýyla etkisiz hale getirildi.");
            Destroy(fireObject);
        }
    }
}