using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;


public class UpdateGPSText : MonoBehaviour
{
   
   public Text myText;
    // Start is called before the first frame update


    void Start()
    {

        myText = GameObject.Find("PositionLog").GetComponent<Text>();
    }
    void Update()
    {

        InvokeRepeating("UpdateText", 0f, 0.2f);

    }

    void UpdateText()
    {

      
        Debug.Log("Lat:" + GPS.Instance.latitude.ToString() + "  Long:" + GPS.Instance.longitude.ToString() + GPS.Instance.latitude.ToString());

    }
}
