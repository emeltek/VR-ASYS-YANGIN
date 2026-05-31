using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Unity'nin kendi sahne kütüphanesi

// ? KRÝTÝK DEÐÝÞÝKLÝK: Çakýþmayý önlemek için sýnýf adýný 'SahneYonetici' yaptýk
public class SahneYonetici : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sahnede duran ana yangýn nesnesini (Eren) buraya sürükleyin.")]
    public ScaleOverTimeAndSpawnFires anaYanginScripti;

    [Tooltip("Baþlangýçta sahnede gizli duracaksa ana ateþ nesnesi")]
    public GameObject anaYanginObjesi;

    public static bool isFireStart = false;
    void Start()
    {
        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(false);
        }
    }

    public void YanginiBaslat()
    {
        Debug.Log("[SÝMÜLASYON] Start butonuna basýldý! Yangýn tetikleniyor...");
        
        isFireStart = true;
        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(true);
        }

        if (anaYanginScripti != null)
        {
            anaYanginScripti.isYanginBasladi = true;
        }
    }

    /// <summary>
    /// ? Mevcut sahneyi tamamen sýfýrlar ve yeniden baþlatýr.
    /// </summary>
    public void SahneyiYenidenBaslat()
    {
        Debug.Log("[SAHNE YÖNETÝCÝSÝ] Sahne yeniden baþlatýlýyor...");
        
        // ? Deðiþiklik: UnityEngine.SceneManagement altýndaki gerçek Scene yapýsýný çaðýrýyoruz
        Scene mevcutSahne = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(mevcutSahne.name);
    }

    /// <summary>
    /// ?? Build Settings'teki bir sonraki sahneye geçiþ yapar.
    /// </summary>
    public void SonrakiSahneyeGec()
    {
        Debug.Log("[SAHNE YÖNETÝCÝSÝ] Sonraki sahneye geçiliyor...");

        // ? Deðiþiklik: Çakýþma ihtimaline karþý tam uzantý (namespace) belirttik
        int mevcutSahneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mevcutSahneIndex + 1);
    }

    /// <summary>
    /// ? Uygulamayý tamamen kapatýr ve çýkýþ yapar.
    /// </summary>
    public void UygulamadanCik()
    {
        Debug.Log("[SAHNE YÖNETÝCÝSÝ] Uygulamadan çýkýþ yapýlýyor...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}