using UnityEngine;
using System.Collections;

public class TreeDissolveController : MonoBehaviour
{
    [Header("Replacement Trees (assign in Inspector)")]
    public GameObject[] replacementTrees;

    [Header("Dissolve Settings")]
    public string parameterName = "_Dissolve";
    public float initialDissolve = 0.02f;
    public float targetDissolve = 1.0f;
    public float dissolveSpeed = 0.2f;

    [Header("Smoke Effect Settings")]
    [Tooltip("Name of the smoke child under the replacement prefab")]
    public string smokeChildName = "SmokeEffect";
    [Tooltip("Time in seconds before smoke child is destroyed")]
    public float smokeLifetime = 10f;

    private Material mat;
    private bool isDissolving = false;
    private bool hasReplaced = false;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("TreeDissolveController: Renderer not found!");
            return;
        }

        Material[] mats = rend.materials;
        if (mats.Length < 2)
        {
            Debug.LogError("TreeDissolveController: renderer.materials does not contain element 1!");
            return;
        }
        mat = mats[1];
        mats[1] = mat;
        rend.materials = mats;

        if (!mat.HasProperty(parameterName))
            Debug.LogError($"Material does not have property '{parameterName}'! Check shader graph reference name.");
        else
            mat.SetFloat(parameterName, initialDissolve);
    }

    public void StartDissolve()
    {
        if (!isDissolving && mat != null)
            StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        isDissolving = true;
        float current = initialDissolve;

        while (current < targetDissolve)
        {
            current += dissolveSpeed * Time.deltaTime;
            mat.SetFloat(parameterName, Mathf.Min(current, targetDissolve));
            yield return null;
        }

        mat.SetFloat(parameterName, targetDissolve);

        if (!hasReplaced)
        {
            hasReplaced = true;
            ReplaceTree();
        }
    }

    private void ReplaceTree()
    {
        Vector3 oldPos = transform.position;
        Quaternion oldRot = transform.rotation;

        if (replacementTrees != null && replacementTrees.Length > 0)
        {
            GameObject newTree = Instantiate(
                replacementTrees[Random.Range(0, replacementTrees.Length)],
                oldPos, oldRot);

            Vector3 eul = newTree.transform.eulerAngles;
            newTree.transform.eulerAngles = new Vector3(-90f, eul.y, eul.z);

            Vector3 p = newTree.transform.position;
            p.y -= 1f;
            newTree.transform.position = p;

            // Try to find the smoke effect by name
            Transform smokeChild = newTree.transform.Find(smokeChildName);
            if (smokeChild != null)
            {
                Destroy(smokeChild.gameObject, smokeLifetime);
            }
            else if (newTree.transform.childCount > 1)
            {
                // Fallback: destroy second child
                Transform fallback = newTree.transform.GetChild(1);
                Debug.LogWarning($"Smoke child '{smokeChildName}' not found. Destroying child at index 1 named '{fallback.name}' after {smokeLifetime}s.");
                Destroy(fallback.gameObject, smokeLifetime);
            }
            else
            {
                Debug.LogWarning("No smoke child found to destroy.");
            }
        }

        Destroy(gameObject);
    }
}