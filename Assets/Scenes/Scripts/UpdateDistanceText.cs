using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UpdateDistanceText : MonoBehaviour
{





    private TMP_Text _tmPro;
    private int distancetemp = 0;
    private int finalDistance = 0;
    private System.Drawing.Color pixel_color;
    private Touch touch;
    private int lastX =0;
    private int lastY =0;


    void Start()
    {
        _tmPro = GetComponent<TMP_Text>();

    }



    void Update()
    {


        if (Input.touchCount == 1)
        {
            //on récupère les coordonnées d'un pixel toucher par le user
            touch = Input.GetTouch(0);

            //try catch car pas toutes les couleurs disponibles et sinon crash
            try
            {
                //on récupère la couleur du pixel choisi stocké dans la liste de drawtexture
                pixel_color = DrawTexture.coordList.FirstOrDefault(item => item.Item1 == (int)(touch.position.x) && item.Item2 == (int)(touch.position.y)).Item3;

                //on récupère la depth de la couleur correspondante qui est stocké dans la liste de Panorama
                distancetemp = Panorama.colorDepth.FirstOrDefault(item => item.Item1.ToString().Equals(pixel_color.ToString())).Item2;

                lastX = (int)(touch.position.x);
                lastY = (int)(touch.position.y);
               finalDistance = getDistanceByColor(distancetemp);
            }
            catch
            {

                _tmPro.text = "Distance indisponible pour ce point !";
              
            }

        }

        else
        {


            _tmPro.text = "Distance totale depuis le point "+ lastX+ " " +lastY +" : " + finalDistance + " mètres !";


        }






    }

    //en fonction de la distance la plus grande (viewrange de panorama) on peut trouver proportionnellement la distance de la couleur actuelle par règle de 3
    public int getDistanceByColor(int colorDistance)
    {

        int depth = 0;

        depth = Panorama.viewrange - (int)(Panorama.viewrange * colorDistance / Panorama.colorDepth.Count);

        return depth;

    }
}