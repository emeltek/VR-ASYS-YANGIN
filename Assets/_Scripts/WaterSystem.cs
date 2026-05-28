using System.Collections;
using UnityEngine;

public class FireWaterCollision : MonoBehaviour
{
    public GameObject smokeprefab;        
    public GameObject FireObject;  
    private float smokedestroy = 3f;    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("water"))
        {
           
            Vector3 spawnPos = transform.position;
            GameObject smoke = Instantiate(smokeprefab, spawnPos,Quaternion.Euler(-90,0,0));
            Destroy(gameObject);
            Destroy(smoke, smokedestroy);
        }
    }
}