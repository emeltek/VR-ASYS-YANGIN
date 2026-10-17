using UnityEngine;

public class FireWaterCollision : MonoBehaviour
{
    [Header("Söndürme Hýz Ayarlarý")]
    public float extinguishSpeed = 1.5f;
    public float destroyThreshold = 0.15f;

    [Header("Görsel Efekt Ayarlarý")]
    public GameObject whiteExtinguishParticlePrefab;
    public float particleSpawnInterval = 0.12f;
    private float nextSpawnTime = 0f;

    // Merkezi ses kontrolü için statik zamanlayýcýlar (Tüm ateþler bu zamaný ortak kullanýr)
    private static float globalNextWarningTime = 0f;
    private static float globalWarningCooldown = 10f; // ? 10 Saniyelik Kesin Fren

    private void OnTriggerEnter(Collider other)
    {
        // ? Giren nesne oyuncu ise VE küresel olarak 10 saniyelik bekleme süresi dolduysa:
        if (other.CompareTag("Player") && Time.time >= globalNextWarningTime)
        {
            // Gelecekteki 10 saniyeyi kilitliyoruz
            globalNextWarningTime = Time.time + globalWarningCooldown;
            Debug.LogWarning("[ÝSG UYARI] Oyuncu ateþe çok yaklaþtý! 10 saniye boyunca bu uyarý tekrar çalmayacak.");

            // Sesi çalmasý için sahnedeki merkezi yöneticiden (WindPositionChecker'daki AudioSource'u kullanabiliriz) ses tetikliyoruz
            AudioSource merkeziSes = other.GetComponentInChildren<AudioSource>();
            
            
           
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fire"))
        {
            ScaleOverTimeAndSpawnFires fireScript = other.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (fireScript != null)
                fireScript.isExtinguishing = false;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("fire"))
        {
            ScaleOverTimeAndSpawnFires fireScript = other.GetComponent<ScaleOverTimeAndSpawnFires>();
            if (fireScript != null)
            {
                fireScript.isExtinguishing = true;
            }

            Extinguish(other.gameObject);

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

        currentScale.x -= extinguishSpeed * Time.deltaTime;
        currentScale.y -= extinguishSpeed * Time.deltaTime;
        currentScale.z -= extinguishSpeed * Time.deltaTime;

        currentScale.x = Mathf.Max(currentScale.x, 0f);
        currentScale.y = Mathf.Max(currentScale.y, 0f);
        currentScale.z = Mathf.Max(currentScale.z, 0f);

        fireObject.transform.localScale = currentScale;

        if (currentScale.x <= destroyThreshold)
        {
            if (YanginDurumYonetici.Instance != null)
                YanginDurumYonetici.Instance.AtesSonduruldu(fireObject);
            Destroy(fireObject);
        }
    }
}