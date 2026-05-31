using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFireController : MonoBehaviour
{
   public AudioSource audioSource;
   public bool isExamScene = false;
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("fire"))
      {
         if (isExamScene == false)
         {
            audioSource.Play();

         }
         ScoreManager.Instance.totalScore -= 35;
      }
   }
}
