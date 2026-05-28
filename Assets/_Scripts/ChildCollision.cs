using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ChildCollisionProxy : MonoBehaviour
{
    [HideInInspector] public TreeDissolveController controller;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fire") && controller != null)
        {
            
            controller.StartDissolve();
        }
    }
}
