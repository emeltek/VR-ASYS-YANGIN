using UnityEngine;

public class FireWaterCollision : MonoBehaviour
{
    [Header("Söndürme Hýz Ayarlarý")]
    [Tooltip("Ateþin saniyede ne kadar hýzlý küçüleceðini belirler. Deðer büyürse daha hýzlý söner.")]
    public float extinguishSpeed = 1.5f; // Hýzlandýrýldý! (Eski deðer 0.4f idi)

    [Tooltip("Ateþ bu ölçeðin altýna düþtüðünde tamamen sönmüþ sayýlýr ve yok edilir.")]
    public float destroyThreshold = 0.15f;

    [Header("Görsel Efekt Ayarlarý")]
    [Tooltip("Ateþ söndürülürken çýkacak olan BEYAZ parçacýk efekti (Prefab).")]
    public GameObject whiteExtinguishParticlePrefab;

    [Tooltip("Beyaz efektin kaç saniyede bir doðacaðýný belirler.")]
    public float particleSpawnInterval = 0.12f; // Efekt sýklýðý da hýzla uyumlu hale getirildi
    private float nextSpawnTime = 0f;

    /// <summary>
    /// Hortum ateþe deðdiði sürece her frame çalýþýr.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("fire"))
        {
            // 1) Ateþin büyüme script'ini bul ve o anki frame için büyümeyi dondur
            ScaleOverTimeAndSpawnFires fireScript = other.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (fireScript != null)
            {
                fireScript.isExtinguishing = true;
            }

            // 2) Ateþi hýzlýca küçült
            Extinguish(other.gameObject);

            // 3) Beyaz parçacýk efektini doður
            if (whiteExtinguishParticlePrefab != null && Time.time >= nextSpawnTime)
            {
                nextSpawnTime = Time.time + particleSpawnInterval;
                Vector3 spawnPosition = other.transform.position + Vector3.up * 0.3f;
                GameObject whiteCloud = Instantiate(whiteExtinguishParticlePrefab, spawnPosition, Quaternion.identity);
                Destroy(whiteCloud, 1.2f);
            }
        }
    }

    private void Extinguish(GameObject fireObject)
    {
        Vector3 currentScale = fireObject.transform.localScale;

        // Belirlenen yüksek hýzda küçültme iþlemi
        currentScale.x -= extinguishSpeed * Time.deltaTime;
        currentScale.y -= extinguishSpeed * Time.deltaTime;
        currentScale.z -= extinguishSpeed * Time.deltaTime;

        currentScale.x = Mathf.Max(currentScale.x, 0f);
        currentScale.y = Mathf.Max(currentScale.y, 0f);
        currentScale.z = Mathf.Max(currentScale.z, 0f);

        fireObject.transform.localScale = currentScale;

        // Eþik deðerinin altýna indiði an tamamen yok et
        if (currentScale.x <= destroyThreshold)
        {
            Debug.Log("[YANGIN SÖNDÜ] Bir ateþ odaðý hýzlýca söndürüldü.");
            Destroy(fireObject);
        }
    }
}