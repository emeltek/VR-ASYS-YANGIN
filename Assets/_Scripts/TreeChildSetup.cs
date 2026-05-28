using UnityEngine;

public class TreeChildSetup : MonoBehaviour
{
    [Header("Dissolve Ayarlarý")]
    public string parameterName = "_Dissolve";
    public float initialDissolve = 0.02f;
    public float targetDissolve = 1.0f;
    public float dissolveSpeed = 0.2f;

    [Header("Efekt ve Prefab Ayarlarý")]
    public GameObject[] replacementTrees;
    public GameObject smokeEffectPrefab;
    public GameObject firePrefab;
    public int fireCount = 5;
    public float fireSpawnRadius = 2f;
    public float fireGrowDuration = 3f;
    public Vector3 fireTargetScale = Vector3.one;

    void Start()
    {
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
        {
            var go = rend.gameObject;

            // 0) Tree tag’ýný ata (önceden Tags penceresinde tanýmlý olmalý)
            go.tag = "Tree";

            // 1) TreeDissolveController ekle ve parametreleri ata
            var dis = go.GetComponent<TreeDissolveController>() ?? go.AddComponent<TreeDissolveController>();
            dis.parameterName = parameterName;
            dis.initialDissolve = initialDissolve;
            dis.targetDissolve = targetDissolve;
            dis.dissolveSpeed = dissolveSpeed;
            dis.replacementTrees = replacementTrees;
            

            // 2) Collider kontrolü ve gerekiyorsa ekleme
            var col = go.GetComponent<Collider>();
            if (col == null)
                col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;

            // 3) ChildCollisionProxy ekle ve controller’ý ata
            var proxy = go.GetComponent<ChildCollisionProxy>() ?? go.AddComponent<ChildCollisionProxy>();
            proxy.controller = dis;
        }
    }
}
