using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Unity'nin kendi sahne k�t�phanesi

// ? KR�T�K DE����KL�K: �ak��may� �nlemek i�in s�n�f ad�n� 'SahneYonetici' yapt�k
public class SahneYonetici : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sahnede duran ana yang�n nesnesini (Eren) buraya s�r�kleyin.")]
    public ScaleOverTimeAndSpawnFires anaYanginScripti;

    [Tooltip("Ba�lang��ta sahnede gizli duracaksa ana ate� nesnesi")]
    public GameObject anaYanginObjesi;

    public static bool isFireStart = false;
    void Start()
    {
        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(false);
        }
    }
    // Singleton pattern — DontDestroyOnLoad nesnelerinde şunu kullan:
   

    public void YanginiBaslat()
    {
        Debug.Log("[DEBUG] YanginiBaslat çalıştı");
        Debug.Log("[DEBUG] anaYanginObjesi null mu: " + (anaYanginObjesi == null));
        Debug.Log("[DEBUG] anaYanginScripti null mu: " + (anaYanginScripti == null));
    
        isFireStart = true;
    
        if (anaYanginObjesi != null)
        {
            anaYanginObjesi.SetActive(true);
            Debug.Log("[DEBUG] anaYanginObjesi SetActive(true) yapıldı, aktif mi: " + anaYanginObjesi.activeSelf);
        }
    
        if (anaYanginScripti != null)
        {
            anaYanginScripti.isYanginBasladi = true;
            Debug.Log("[DEBUG] isYanginBasladi = " + anaYanginScripti.isYanginBasladi);
            Debug.Log("[DEBUG] Object null mu: " + (anaYanginScripti.Object == null));
        }
    }

    /// <summary>
    /// ? Mevcut sahneyi tamamen s�f�rlar ve yeniden ba�lat�r.
    /// </summary>
    public void SahneyiYenidenBaslat()
    {
        isFireStart = false; // ← EKLENDİ
        Time.timeScale = 1f; // ← Eğer oyunu durdurduysanız
        Debug.Log("[SAHNE YÖNETİCİSİ] Sahne yeniden başlatılıyor...");
        Scene mevcutSahne = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(mevcutSahne.name);
    }
    /// <summary>
    /// ?? Build Settings'teki bir sonraki sahneye ge�i� yapar.
    /// </summary>
    public void SonrakiSahneyeGec()
    {
        // Tüm static değişkenleri sıfırla
        SahneYonetici.isFireStart = false;
        YanginDurumYonetici.isSceneSuccessful = false;

        Time.timeScale = 1f;

        Debug.Log("[SAHNE YÖNETİCİSİ] Sonraki sahneye geçiliyor...");
        int mevcutSahneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mevcutSahneIndex + 1);
    }
    /// <summary>
    /// ? Uygulamay� tamamen kapat�r ve ��k�� yapar.
    /// </summary>
    public void UygulamadanCik()
    {
        Debug.Log("[SAHNE Y�NET�C�S�] Uygulamadan ��k�� yap�l�yor...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}