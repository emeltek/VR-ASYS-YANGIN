using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePosition : MonoBehaviour
{
    public GameObject fireEffect; // Ateş partikülü prefab
    public GameObject initialEffect; // Başlangıç efekti (yangın büyürkenki efekt)
    public float spreadRadius = 3f; // Yangının yayılma mesafesi
    public float growthRate = 0.2f; // Başlangıç efektinin büyüme hızı
    public float maxRadius = 10f; // Maksimum büyüme yarıçapı
    public float fireSpeed = 2f; // Yangının ilerleme hızı

    private Terrain terrain;
    private float currentRadius = 0f; 

    // Yangının izlerini tutacak bir liste
    private List<GameObject> fireEffects = new List<GameObject>();

    private Vector3 fireDirection;  // Yangının ilerleyeceği yön

    void Start()
    {
        // Terrain nesnesine eriş
        terrain = Terrain.activeTerrain;

        if (terrain == null)
        {
            Debug.LogError("Aktif bir Terrain bulunamadı!");
            return;
        }

        if (initialEffect == null)
        {
            Debug.LogError("Başlangıç efekti atanmadı!");
            return;
        }

        // Başlangıç efektini başlat
        initialEffect.transform.localScale = Vector3.zero;

        // Yangının başlangıç yönünü belirleyelim
        fireDirection = transform.forward; // Yangın başlangıç yönü
    }

    void Update()
    {
        // Efekti büyüt
        if (currentRadius < maxRadius)
        {
            currentRadius += growthRate * Time.deltaTime;
            initialEffect.transform.localScale = Vector3.one * currentRadius;

            // Yangın etkisinin ağaçlara uygulanması
            SpreadFireFromPosition(transform.position, currentRadius);
        }
        else
        {
            // Efekt maksimum boyuta ulaştıysa durdur
            Destroy(initialEffect);
            enabled = false; // Script'i devre dışı bırak
        }

        // Yangının ilerlemesi
        MoveFireEffect();
    }

    private void MoveFireEffect()
    {
        // Yangının ilerlemesi için yeni pozisyon hesapla
        Vector3 newFirePosition = transform.position + fireDirection * fireSpeed * Time.deltaTime;

        // Yeni yangın efekti oluştur
        GameObject fire = Instantiate(fireEffect, newFirePosition, Quaternion.identity);
        fire.layer = LayerMask.NameToLayer("FireEffect");

        // Ateşi listeye ekle (geride kalan yangın izleri için)
        fireEffects.Add(fire);

        // Sonraki pozisyonu güncelle
        transform.position = newFirePosition;
    }

    private void SpreadFireFromPosition(Vector3 startPosition, float radius)
    {
        TreeInstance[] trees = terrain.terrainData.treeInstances;
        foreach (var tree in trees)
        {
            Vector3 worldPosition = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;

            // Yangın belirli bir yarıçapta ise
            if (Vector3.Distance(startPosition, worldPosition) <= radius)
            {
                // Eğer ağaçta zaten bir efekt varsa, tekrarlama
                if (Physics.CheckSphere(worldPosition, 0.5f, LayerMask.GetMask("FireEffect")))
                    continue;

                // Ateş partikülü oluştur
                GameObject fire = Instantiate(fireEffect, worldPosition, Quaternion.identity);
                fire.layer = LayerMask.NameToLayer("FireEffect");

                // Ateşi listeye ekle (geride kalan yangın izleri için)
                fireEffects.Add(fire);
            }
        }

        // Geride kalan yangın efektlerini temizleme (eski efektler belirli bir sürede yok edilebilir)
        CleanUpOldFireEffects();
    }

    private void CleanUpOldFireEffects()
    {
        // Geride kalan yangın izlerini temizle
        for (int i = fireEffects.Count - 1; i >= 0; i--)
        {
            // Efektin ömrü bitti mi?
            if (fireEffects[i] == null) // Eğer efekt yoksa
            {
                fireEffects.RemoveAt(i); // Listeyi temizle
            }
            else if (Time.time - fireEffects[i].GetComponent<ParticleSystem>().time > 50f) // Efekt 5 saniyeden uzun sürüyorsa
            {
                Destroy(fireEffects[i]);
                fireEffects.RemoveAt(i);
            }
        }
    }
}
