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

    // Start is called before the first frame update
    void Start()
    {
        BetterStreamingAssets.Initialize();

        rend = GetComponent<SpriteRenderer>();


        //1696/720
      texture  = new Texture2D(1696, 720, TextureFormat.ARGB32, false);

      


        var bufferColor = Panorama.getBuffer();

         applyTexture(bufferColor);
       

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

