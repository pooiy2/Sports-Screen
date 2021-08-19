using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;


//TODO: Redo entire calibration using fiducial marker


public class Calibration : MonoBehaviour
{
    //Pass Ins
    public GameObject TopLeft;
    public MeasureDepth mMeasureDepth;
    public MultiSourceManager mMultiSource;

    //Legacy Code, used to display live feed from kinect
    //Code doesn't work when deleted???
    public RawImage mRawImage;
    public RawImage mRawDepth;

    //Variables
    public int count = 0;
    public int start = 0;
    Color[] img = null;
    RawImage square;

    //Resulting list consisting of 1's at screen position
    public List<int> corners;


    // Start is called before the first frame update
    void Start()
    {
        corners = new List<int>();
        square = TopLeft.GetComponent<RawImage>();
        ChangeColor(Color.black); //Change the color of the screen to black
    }

    // Update is called once per frame
    void Update()
    {
        //Updates depth and image textures
        //mRawImage.texture = mMultiSource.GetColorTexture();

        //mRawDepth.texture = mMeasureDepth.mDepthTexture;

        //At different intervals work on calibrating screen
        if (start==0){
            img = mMultiSource.GetColorTexture().GetPixels();
            if (Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05){
                start+=1;
            }
        }
        if (start==1){    //Initialize list of ints to 1 
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                corners.Add(1);
            }
            start++;
            ChangeColor(Color.black); //Change the color of the screen to black
        } else if (start==50) {    //Change all ints in list to 0 where the camera does not pick up the color black
            Calibrate(0, 0, 0, .5);
            start++;
            ChangeColor(Color.white);
            //Save image with blue over the parts that the camera thinks is black
            img = mMultiSource.GetColorTexture().GetPixels();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                if (corners[i]==1){
                    img[i] = Color.blue;
                }
            }
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgbefore.png");

        } else if (start==100) { //Change all ints in list to 0 where canera does not pick up the color white
            Calibrate(1, 1, 1, .2);
        //Save image with blue over the parts that the camera first thought was black and now is white (should only include the screen)
            img = mMultiSource.GetColorTexture().GetPixels();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                if (corners[i]==1){
                    img[i] = Color.red;
                    count+=1;
                }
            }
            //Save image
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgafter.png");
            Destroy(TopLeft); //Remove image used to change screen color once calibration is complete
            start++;
        } else if (start>0 & start<100){
            start++;
        }
    }

    public void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
         {
             //Saves Textures to a PNG file
             byte[] _bytes =_texture.EncodeToPNG();
             System.IO.File.WriteAllBytes(_fullPath, _bytes);
             Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
         }

    void Calibrate(double r, double b, double g, double e){
        //Uses the camera feed to determine the colors of each pixel and see if it matches the given rgb values within an error of e
        img = mMultiSource.GetColorTexture().GetPixels();
        for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
            if (Math.Abs(img[i].r-r) < e & Math.Abs(img[i].g-g) < e & Math.Abs(img[i].b-b) < e){
                //Do nothing
            } else {
                corners[i] = 0;
            }
        }
    }

    void ChangeColor(Color color){
        //Changes the color of the screen
        square.color = color;
    }
}