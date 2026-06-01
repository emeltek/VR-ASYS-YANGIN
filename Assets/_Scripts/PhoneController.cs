using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    public AudioSource audio;
    public bool isExamScene = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.PuanEkle(20);
            else
                Debug.LogWarning("[PhoneController] ScoreManager.Instance null!");

            if (isExamScene == false && audio != null)
                audio.Play();

            gameObject.SetActive(false);
        }
    }
}