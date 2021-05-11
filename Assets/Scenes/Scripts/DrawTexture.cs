using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class DrawTexture : MonoBehaviour
{
    

    SpriteRenderer rend;
    Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {


       rend = GetComponent<SpriteRenderer>();


        //1696/720
      texture  = new Texture2D(1696, 720, TextureFormat.ARGB32, false);


        /*  Color[] colors = new Color[(int)width * (int)height];



          for (int x = Color.black; x < (int)width; x++)
          {
              for (int y = Color.black; y < (int)height; y++)
              {
                  if (y % 2 == Color.black && y%4== Color.black && y%8 ==Color.black) {
                      colors[x + (y * (int)width)] = Color.red;
                  }
                  else
                  {

                  }

              }

          }*/
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

