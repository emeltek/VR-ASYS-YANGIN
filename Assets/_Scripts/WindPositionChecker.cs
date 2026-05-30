using UnityEngine;

public class WindPositionChecker : MonoBehaviour
{
    [Header("Rüzgar Ayarlarý")]
    public Vector3 windDirection = new Vector3(1, 0, 0); 

    [Header("Referanslar")]
    public Transform playerTransform; // VR Kamera / Oyuncu Pozisyonu

    [Header("Ayarlar")]
    [Range(-1f, 1f)]
    public float safetyThreshold = 0.3f; 

    [Tooltip("Kullanýcýnýn tehlikeli pozisyondayken uyarý almasý için ateþe ne kadar yakýn olmasý gerektiðini belirler (Metre cinsinden).")]
    public float triggerDistance = 5f; // ? YENÝ: Yakýnlýk sýnýrýný 5 metre yaptýk

    private float penaltyTimer = 0f;
    public float penaltyInterval = 3f; // Kaç saniyede bir sesli uyarý tekrarlansýn?

    [Header("Sesli Uyarý Ayarlarý")]
    public AudioSource warningAudioSource;
    public AudioClip wrongBehaviorClip;

    void Update()
    {
        // Sahnede "fire" tagine sahip en yakýn ateþi bulalým
        GameObject targetFire = FindClosestFire();

        if (targetFire != null && playerTransform != null)
        {
            // ? YENÝ: Oyuncu ile en yakýn ateþ arasýndaki gerçek mesafeyi ölçüyoruz
            float distanceToFire = Vector3.Distance(playerTransform.position, targetFire.transform.position);

            // Oyuncudan ateþe doðru olan yön vektörü
            Vector3 playerToFire = (targetFire.transform.position - playerTransform.position).normalized;
            
            // Sadece yatay düzlemde hesaplama (X-Z)
            playerToFire.y = 0; 
            Vector3 normalizedWind = windDirection.normalized;
            normalizedWind.y = 0;

            // Ýki vektör arasýndaki açýsal benzerlik (-1 ile 1 arasý)
            float dotProduct = Vector3.Dot(playerToFire, normalizedWind);

            // ? ÞART GÜNCELLENDÝ: 
            // Hem rüzgar yönü tehlikeliyse (dotProduct < threshold) 
            // HEM DE oyuncu ateþe belirlenen mesafeden yakýnsa (distanceToFire <= triggerDistance)
            if (dotProduct < safetyThreshold && distanceToFire <= triggerDistance)
            {
                TriggerWindDanger();
            }
            else
            {
                // Kullanýcý güvenli bölgedeyse veya ateþten uzaklaþtýysa zamanlayýcýyý sýfýrla
                penaltyTimer = penaltyInterval; 
            }
        }
    }

    GameObject FindClosestFire()
    {
        GameObject[] fires = GameObject.FindGameObjectsWithTag("fire");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = playerTransform.position;

        foreach (GameObject fire in fires)
        {
            Vector3 diff = fire.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = fire;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void TriggerWindDanger()
    {
        penaltyTimer += Time.deltaTime;

        if (penaltyTimer >= penaltyInterval)
        {
            penaltyTimer = 0f;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.totalScore -= 10; 
                Debug.Log("[TEHLÝKE] Hem rüzgarý karþýna aldýn hem de ateþe çok yakýnsýn! -10 Puan!");
            }

            if (warningAudioSource != null && wrongBehaviorClip != null)
            {
                if (!warningAudioSource.isPlaying)
                {
                    warningAudioSource.PlayOneShot(wrongBehaviorClip);
                }
            }
        }
    }
}