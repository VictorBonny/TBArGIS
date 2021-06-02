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



        // size texture : 1696/720
        texture = new Texture2D(1696, 720, TextureFormat.ARGB32, false);


        // InvokeRepeating("UpdateFakeTexture", 0f, 10f);

        InvokeRepeating("UpdateTexture", 0f, 60f);

    }


    // Is working but because of slow download of the texture.
    // Next tests will be done with an update method that applies a fake texture

    void UpdateTexture()
    {

        Debug.Log("Lat:" + GPS.Instance.latitude.ToString() + "  Long:" + GPS.Instance.longitude.ToString() + " alt:" + GPS.Instance.latitude.ToString());

        if (GPS.Instance.latitude != 0.0f && GPS.Instance.longitude != 0.0f && GPS.Instance.longitude != 0.0f)
        {
            var bufferColor = Panorama.getBuffer(GPS.Instance.latitude, GPS.Instance.longitude, GPS.Instance.altitude);
            applyTexture(bufferColor);
        }
    }



    //generate texture based on hgt files
    void applyTexture(byte[] buffer)
    {


        var colorArray = new Color32[buffer.Length / 4];
        for (var i = 0; i < buffer.Length; i += 4)
        {
            var color = new Color32();
            color.a = buffer[i + 0];
            color.r = buffer[i + 1];
            color.g = buffer[i + 2];
            color.b = buffer[i + 3];

            if (color.r == 255 & color.g == 255 & color.b == 255)
                color = Color.clear;

            colorArray[i / 4] = color;
        }
        texture.SetPixels32(colorArray);
        texture.Apply();

        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);


        rend.sprite = newSprite;


        GetComponent<Image>().sprite = newSprite;




    }

    void UpdateFakeTexture()
    {

        System.Random _random = new System.Random();
        Color[] colors = new Color[(int)1696 * (int)720];


        for (int x = 0; x < (int)1696; x++)

        {

            for (int y = 0; y < (int)720; y++)

            {

                if (_random.Next(10) % 4 == 0)
                {

                    colors[x + (y * (int)1696)] = Color.red;

                }

                else

                {


                }



            }



        }

        texture.SetPixels(colors);

        texture.Apply();

        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        rend.sprite = newSprite;

        GetComponent<Image>().sprite = newSprite;

    }
}

