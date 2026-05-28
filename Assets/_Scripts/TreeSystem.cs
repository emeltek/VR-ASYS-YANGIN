using UnityEngine;

public class TreeSystem : MonoBehaviour
{
    [Header("BoxCollider Settings")]
    public Vector3 colliderCenter = Vector3.zero;
    public Vector3 colliderSize = Vector3.one;
    public bool isTrigger = true;

    void Start()
    {
        // Iterate through all child transforms
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            // Skip the parent itself
            if (child == transform) continue;

            GameObject go = child.gameObject;

            // Check and add/configure BoxCollider
            BoxCollider col = go.GetComponent<BoxCollider>();
            if (col == null)
            {
                col = go.AddComponent<BoxCollider>();
                if (col == null)
                {
                    Debug.LogWarning("BoxCollider eklenemedi: " + go.name);
                    continue;
                }
            }
            col.center = colliderCenter;
            col.size = colliderSize;
            col.isTrigger = isTrigger;
            

            // Check and add/configure Rigidbody
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogWarning("Rigidbody eklenemedi: " + go.name);
                    continue;
                }
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}