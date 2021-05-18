using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class DrawTexture : MonoBehaviour
{
   
   
    SpriteRenderer rend;
    Texture2D texture;

    void Start()
    {
        BetterStreamingAssets.Initialize();

        rend = GetComponent<SpriteRenderer>();


        //1696/720
      texture  = new Texture2D(1696, 720, TextureFormat.ARGB32, false);

        

    }

    void Update()
    {

        InvokeRepeating("UpdateTexture", 0f, 60f);


    }
    void UpdateTexture()
    {

        Debug.Log("Lat:" + GPS.Instance.latitude.ToString() + "  Long:" + GPS.Instance.longitude.ToString() + " alt:"+ GPS.Instance.latitude.ToString());

        if (GPS.Instance.latitude != 0.0f && GPS.Instance.longitude != 0.0f && GPS.Instance.longitude != 0.0f)
        {
            var bufferColor = Panorama.getBuffer(GPS.Instance.latitude, GPS.Instance.longitude, GPS.Instance.altitude);
            applyTexture(bufferColor);
        }
    }



    void applyTexture(byte[]buffer)
    {

     
        texture.LoadRawTextureData(buffer);

        texture.Apply();

        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);


        rend.sprite = newSprite;


        GetComponent<Image>().sprite = newSprite;

    }
}

