using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Oculus.Interaction;
public class FireStart : MonoBehaviour

{
    [Header("Referanslar")]
    [Tooltip("Klavyeden Space'e basýnca tetiklenecek olan Event Wrapper bileþeni.")]
    public InteractableUnityEventWrapper eventWrapper;

    [Header("Klavye Ayarý")]
    public KeyCode testKey = KeyCode.Space;

    void Update()
    {
        // Eðer editördeysek ve belirlenen tuþa (Space) basýldýysa
        if (Input.GetKeyDown(testKey))
        {
            if (eventWrapper != null)
            {
                Debug.Log($"[PC Test] {testKey} tuþuna basýldý. Butonun 'When Select' eventleri tetikleniyor...");
                
                // Meta SDK'in o buton seçildiðinde çaðýrdýðý tüm fonksiyonlarý el ile tetikliyoruz
                eventWrapper.WhenSelect.Invoke();
            }
            else
            {
                Debug.LogWarning("Event Wrapper referansý eksik! Lütfen Inspector'dan atayýn.");
            }
        }
    }
}
