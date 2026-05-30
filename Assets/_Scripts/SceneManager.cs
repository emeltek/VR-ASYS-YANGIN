using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sahnede duran ana yangýn nesnesini (Eren) buraya sürükleyin.")]
    public ScaleOverTimeAndSpawnFires anaYanginScripti;

    [Tooltip("Baþlangýçta sahnede gizli duracaksa ana ateþ nesnesi")]
    public GameObject anaYanginObjesi;

    void Start()
    {
        // Simülasyon baþýnda yangýn objesini tamamen gizleyebiliriz (Ýsteðe baðlý)
        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(false);
        }
    }

    // ? VR Butonuna basýldýðýnda Meta SDK Event Wrapper'ýn çaðýracaðý fonksiyon:
    public void YanginiBaslat()
    {
        Debug.Log("[SÝMÜLASYON] Start butonuna basýldý! Yangýn tetikleniyor...");

        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(true); // Ateþi görünür yap
        }

        if (anaYanginScripti != null)
        {
            anaYanginScripti.isYanginBasladi = true; // Kildi aç ve büyümeyi baþlat!
        }
    }
}
