using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
   public AudioSource audio;

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         audio.Play();
         gameObject.SetActive(false);
      }
   }
}
