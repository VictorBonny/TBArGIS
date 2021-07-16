using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


[RequireComponent(typeof(SpriteRenderer))]
public class DrawTexture : MonoBehaviour
{

    public static DrawTexture Instance { set; get; }

    int x;
    int y;
    Boolean controle;
    public SpriteRenderer rend;
    public Texture2D texture;
    public Sprite newSprite;
    public static List<Tuple<int, int, System.Drawing.Color>> coordList = new List<Tuple<int, int, System.Drawing.Color>>();

    void Start()
    {
        Instance = this;

        //initialisation du plugin qui permet de lire les fichier .hgt en streaming assets
        BetterStreamingAssets.Initialize();

        rend = GetComponent<SpriteRenderer>();



        //beaucoup de calculs à faire donc on doit ralentir l'appel la méthode
        //update chaque minute
        InvokeRepeating("UpdateTexture", 10f, 60f);


    }



    public void UpdateTexture()
    {



        //avoir la vue depuis Cran-Montana : Latitude : 46.3 Longitude: 7.4667 hauteur : ~ 1'000 + orientation du téléphone récupéré depuis la classe GPS
        var bufferColor = Panorama.getBuffer(46.3f, 7.4667f, 1000, GPS.Instance.bearingSmartphone);
    
  
        applyTexture(bufferColor);





    }



    //creation texture d'après buffer
    void applyTexture(byte[] buffer)
    {
        texture = new Texture2D(Panorama.width, 1080, TextureFormat.ARGB32, false);



        var colorArray = new Color32[buffer.Length / 4];
        var colorArrayClear = new Color32[buffer.Length / 4];
        var colorTexture = new Color32();



        for (var i = 0; i < buffer.Length; i += 4)
        {




            colorTexture.a = buffer[i + 0];
            colorTexture.r = buffer[i + 1];
            colorTexture.g = buffer[i + 2];
            colorTexture.b = buffer[i + 3];


            // traitement transparence si blanc
            if ((colorTexture.r == 255 & colorTexture.g == 255 & colorTexture.b == 255))
            {
                colorTexture = Color.clear;
            }

            //colorArray necessaire pour garder en memoire la couleur de chaque pixel
            colorArray[i / 4] = colorTexture;


            // traitement transparence complète sauf les ridges
            if (!(colorTexture.r == 0 & colorTexture.g == 0 & colorTexture.b == 0))
            {
                colorTexture = Color.clear;
            }


            //colorArray necessaire pour créer la texture
            colorArrayClear[i / 4] = colorTexture;



        }




        //tableau avec les couleurs pour set les positions de couleur pour chaque pixel
        setPixelInformation(colorArray);


        //tableau avec couleur des ridge et le reste transparent pour la texture
        texture.SetPixels32(colorArrayClear);



        texture.Apply();

        Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.0f, 0.0f));
        

        rend.sprite = newSprite;


        GetComponent<Image>().sprite = newSprite;

    }

    //garder référence couleur pour chaque pixel de la texture
    private void setPixelInformation(Color32[] colorArray)
    {
    

        x = 0;
        y = 0;
        controle = false;

        for (var i = 0; i < colorArray.Length; i++)
        {

            if (i % Panorama.width == 0 && x != 0)
            {
                controle = true;
            }


            Tuple<int, int, System.Drawing.Color> temp = new Tuple<int, int, System.Drawing.Color>(x, y, System.Drawing.Color.FromArgb(colorArray[i].a, colorArray[i].r, colorArray[i].g, colorArray[i].b));


            coordList.Add(temp);

            x++;

            if (controle)
            {

                x = 0;
                y++;
                controle = false;
            }


        }
    }


}

