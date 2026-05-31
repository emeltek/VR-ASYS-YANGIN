using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrabbleController : MonoBehaviour
{

    public GameObject phone;
    public GameObject fire_Ex;
    public GameObject pin;
    public GameObject Hose;

    private void Start()
    {
        phone.GetComponent<BoxCollider>().enabled = false;
        fire_Ex.GetComponent<BoxCollider>().enabled = false;
        pin.GetComponent<BoxCollider>().enabled = false;
        Hose.GetComponent<BoxCollider>().enabled = false;
    }

    public void GrabbleObjectActive()
    {
        phone.GetComponent<BoxCollider>().enabled = true;
        fire_Ex.GetComponent<BoxCollider>().enabled = true;
        pin.GetComponent<BoxCollider>().enabled = true;
        Hose.GetComponent<BoxCollider>().enabled = true;
    }
}
