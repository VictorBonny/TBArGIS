using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }


    public float latitude;
    public float longitude;
    public float altitude;
    public float bearingSmartphone;

  
    // service de gps pour récupérer position du smartphone et son orientation
   
    private void Start()
    {

        Instance = this;
        Input.compass.enabled = true;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
       
    }


    //démarrage du service de location
    IEnumerator StartLocationService()
    {

        if(!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled GPS");
            yield break;
        }

        Input.location.Start();
        int maxWait = 20;
        while(Input.location.status == LocationServiceStatus.Initializing && maxWait >0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determin device location");
            yield break;
        }
        else
        {
            //si ok chaque 0,2 secondes on raffraichit les données
            InvokeRepeating("UpdateLocation", 0f, 0.2f);
        }

      


    }

    private void UpdateLocation()
    {

        if (Input.location.status == LocationServiceStatus.Running)
        {

            //latitute
            latitude = Input.location.lastData.latitude;
            //longitude
            longitude = Input.location.lastData.longitude;
            //altitude
            altitude = Input.location.lastData.altitude;
            //orientation
            bearingSmartphone = Input.compass.trueHeading;

        }
        else
        {

        }

    }

}
