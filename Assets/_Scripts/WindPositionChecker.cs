using UnityEngine;

public class WindPositionChecker : MonoBehaviour
{
    [Header("Rüzgar Ayarlarý")]
    // Ýlk scriptinizdeki rüzgar yönü (Örn: X yönünde esiyor)
    public Vector3 windDirection = new Vector3(1, 0, 0); 

    [Header("Referanslar")]
    public Transform playerTransform; // VR Kamera / Oyuncu Pozisyonu

    [Header("Ayarlar")]
    [Range(-1f, 1f)]
    // Hassasiyet eþiði (0.5 idealdir, tam rüzgar yönü açýsýný belirler)
    public float safetyThreshold = 0.3f; 

    private float penaltyTimer = 0f;
    private float penaltyInterval = 2f; // Kaç saniyede bir hata uyarýsý versin?

    void Update()
    {
        // Sahnede "fire" tagine sahip en yakýn ateþi bulalým
        GameObject targetFire = FindClosestFire();

        if (targetFire != null && playerTransform != null)
        {
            // Oyuncudan ateþe doðru olan yön vektörü
            Vector3 playerToFire = (targetFire.transform.position - playerTransform.position).normalized;
            
            // Yükseklik (Y) farklarýný sýfýrlýyoruz ki sadece yatay düzlemde hesaplasýn
            playerToFire.y = 0; 
            Vector3 normalizedWind = windDirection.normalized;
            normalizedWind.y = 0;

            // Ýki vektör arasýndaki açýsal benzerliði hesaplar (-1 ile 1 arasýnda deðer döner)
            float dotProduct = Vector3.Dot(playerToFire, normalizedWind);

            // Eðer dotProduct sonucu pozitif ve eþikten büyükse: Rüzgar arkamýzda (GÜVENLÝ)
            // Eðer dotProduct negatifse: Oyuncu rüzgarýn estiði yönde duruyor, alevler üzerine geliyor! (TEHLÝKELÝ)
            if (dotProduct < safetyThreshold)
            {
                TriggerWindDanger();

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

            // 1. Puan Kes
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.totalScore -= 10; // Rüzgar hatasý cezasý
                Debug.Log("Rüzgarý karþýna aldýn! Duman yüzüne geliyor. -10 Puan!");
            }

            // 2. VR Ses / UI Uyarýsý Tetikle
            // (Buraya ses oynatma veya ekrana yazý yazdýrma kodunu ekleyebilirsin)
            
            // 3. Opsiyonel: VR kamerasýnýn önüne hafif siyah duman UI paneli getirerek görüþü bozabilirsin.
        }
    }
}