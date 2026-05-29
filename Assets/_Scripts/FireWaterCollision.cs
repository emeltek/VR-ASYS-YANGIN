using UnityEngine;

public class FireWaterCollision : MonoBehaviour
{
    [Header("Söndürme Ayarlarý")]
    [Tooltip("Ateþin saniyede ne kadar küçüleceðini belirler. Deðer büyürse ateþ daha hýzlý söner.")]
    public float extinguishSpeed = 0.4f; 

    [Tooltip("Ateþ bu ölçeðin altýna düþtüðünde tamamen sönmüþ sayýlýr.")]
    public float destroyThreshold = 0.15f;

    // YÖNTEM 1: Eðer hortumun ucuna koyduðun görünmez Collider (isTrigger açýk) ateþe deðerse
    private void OnTriggerStay(Collider other)
    {
        // Çarptýðýmýz nesnenin tag'i "fire" ise (küçük ateþler veya ana ateþ)
        if (other.CompareTag("fire"))
        {
            Extinguish(other.gameObject);
        }
    }

    // YÖNTEM 2: Eðer Particle System'ýn kendi "Collision" modunu kullanýyorsan (Parçacýklar ateþe çarptýðýnda)
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("fire"))
        {
            Extinguish(other);
        }
    }

    /// <summary>
    /// Ateþ nesnesini zamana baðlý olarak küçültür ve söndürür.
    /// </summary>
    private void Extinguish(GameObject fireObject)
    {
        if (fireObject == null) return;

        // Ateþin mevcut ölçeðini yavaþça azaltýyoruz
        Vector3 currentScale = fireObject.transform.localScale;
        float reduction = extinguishSpeed * Time.deltaTime;

        currentScale.x -= reduction;
        currentScale.y -= reduction;
        currentScale.z -= reduction;

        // Ateþin eksi deðerlere düþmesini engellemek için sýnýrlýyoruz
        fireObject.transform.localScale = new Vector3(
            Mathf.Max(currentScale.x, 0f),
            Mathf.Max(currentScale.y, 0f),
            Mathf.Max(currentScale.z, 0f)
        );

        // Eðer ateþ belirlenen eþik deðerinden daha fazla küçüldüyse (Söndüyse)
        if (fireObject.transform.localScale.x <= destroyThreshold)
        {
            // Sönme anýnda istersen buraya bir "týslama" ses efekti veya su buharý dumaný ekleyebilirsin.
            
            Destroy(fireObject);
            Debug.Log($"[Yangýn Sistemi] Bir adet '{fireObject.name}' baþarýyla söndürüldü.");
        }
    }
}