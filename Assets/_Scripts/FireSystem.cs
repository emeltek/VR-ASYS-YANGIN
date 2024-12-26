using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
public class FireSystem : MonoBehaviour
{
    private ParticleSystem ps;
    public GameObject flameTarget;
    public float shapeIncValue = 0.1f;
    public float emissionIncValue = 0.1f;
    public float sizeIncValue = 0.01f;
 
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        transform.position = new Vector3(25, 0.75f, 0);
        transform.DOMove(flameTarget.transform.position, 120);
    }

    void Update()
    {
        var shape = ps.shape;
        var emission = ps.emission;
        var main = ps.main;
        shapeIncValue += 0.1f * Time.deltaTime;
        emissionIncValue += 0.8f * Time.deltaTime;
        if (sizeIncValue < 5)
        {
            sizeIncValue += 0.1f * Time.deltaTime;
            main.startSize = sizeIncValue;
        }
        emission.rateOverTime = emissionIncValue;
        shape.radius = shapeIncValue;
        transform.position = Vector3.forward * 0.1f ;
    }

}
