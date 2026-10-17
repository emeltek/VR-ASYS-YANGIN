using UnityEngine;

/// <summary>
/// Bu objeyi KnnDangerZoneSystem'de bir tehlike noktası (düğüm) yapar.
/// Obje aktif olduğunda bölgeye otomatik eklenir, kapandığında veya yok edildiğinde çıkarılır.
/// Yangında her ateş bu bileşeni otomatik alır (ScaleOverTimeAndSpawnFires bakın).
/// </summary>
public class KnnDangerNode : MonoBehaviour
{
    [Tooltip("Eklenecek bölgenin adı (KnnDangerZoneSystem'deki bölge adıyla birebir aynı olmalı).")]
    public string zoneName = "Yangın";

    [Tooltip("Obje bu ölçeğin altındayken (örn. ateş henüz büyümediyse veya sönüyorsa) düğüm sayılmaz. 0 = her zaman sayılır.")]
    [Min(0f)] public float minScale = 0.2f;

    bool isRegistered;
    KnnDangerZoneSystem registeredSystem;

    /// <summary>Objede KnnDangerNode yoksa ekler. Varsa dokunmaz.</summary>
    public static KnnDangerNode Ensure(GameObject go, string zoneName = "Yangın")
    {
        KnnDangerNode node = go.GetComponent<KnnDangerNode>();
        if (node == null)
        {
            node = go.AddComponent<KnnDangerNode>();
            node.zoneName = zoneName;
        }
        return node;
    }

    void Update()
    {
        // Sistem sonradan oluşabilir, ateş büyüyüp küçülebilir: her karede durumu eşitle
        bool shouldRegister = KnnDangerZoneSystem.Instance != null && transform.localScale.x >= minScale;
        if (shouldRegister && !isRegistered) Register();
        else if (!shouldRegister && isRegistered) Unregister();
    }

    void OnDisable()
    {
        Unregister();
    }

    void Register()
    {
        registeredSystem = KnnDangerZoneSystem.Instance;
        registeredSystem.AddPoint(zoneName, transform);
        isRegistered = true;
    }

    void Unregister()
    {
        if (!isRegistered) return;
        if (registeredSystem != null) registeredSystem.RemovePoint(zoneName, transform);
        registeredSystem = null;
        isRegistered = false;
    }
}
