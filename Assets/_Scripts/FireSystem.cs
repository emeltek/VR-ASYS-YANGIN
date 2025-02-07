using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSystem : MonoBehaviour
{
    public List<ParticleSystem> flameParticle;
    public float growthRate = 2f;
    private List<float> particleGrowths;
    public List<ParticleSystem> wallFlames;
    public List<ParticleSystem> treeFlames;
    void Start()
    {
        // ParticleGrowths listesini oluþtur ve boyutunu particleSystems listesine göre ayarla
        particleGrowths = new List<float>();
        for (int i = 0; i < flameParticle.Count; i++)
        {
            particleGrowths.Add(0f); 
        }
    }

    void Update()
    {
        if (flameParticle[9].shape.radius > 5 && flameParticle[9].shape.radius < 5.2)
        {
            BurnTress();
        }
        if (flameParticle[3].shape.radius > 12)
        {
            for (int i = 4; i <= 9; i++)
            {
                GrowthParticle(i);
            }
        }
        else if (flameParticle[0].shape.radius > 12)
        {
            for (int i = 1; i < 4; i++)
            {
                GrowthParticle(i);
            }
        }
        else
        {
            GrowthParticle(0);
        }
    }

    void GrowthParticle(int index)
    {
        ParticleSystem ps = flameParticle[index]; 

        if (!ps.gameObject.activeInHierarchy)
        {
            ps.gameObject.SetActive(true);
        }

        var shape = ps.shape;
        var emission = ps.emission;
        var main = ps.main;
        
        particleGrowths[index] += growthRate * Time.deltaTime;
        if (particleGrowths[index] < 5)
        {
            main.startSize = particleGrowths[index];
        }
        shape.radius = particleGrowths[index] / 5;
        if (index == 0)
        {
            emission.rateOverTime = particleGrowths[index]; 
        }
    }

    void BurnTress()
    {
        for (int i = 0; i < wallFlames.Count; i++)
        {
            if (!wallFlames[i].gameObject.activeInHierarchy)
            {
                wallFlames[i].gameObject.SetActive(true);
            }
            if (!treeFlames[i].gameObject.activeInHierarchy)
            {
                treeFlames[i].gameObject.SetActive(true);
            }
        }
    }
}