using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class AdvancedCalibration : MonoBehaviour
{
    //Pass ins
    public RawImage TopLeft;
    public RawImage TopRight;
    public RawImage BoottomLeft;
    public Camera Camera;

    //public MeasureDepth mMeasureDepth;
    public MultiSourceManager mMultiSource;
    public GameObject qr;

    //Variables
    public int start = 0;
    Color[] img = null;
    Texture2D saveImg;
    RawImage square;
    List<Vector2> Centers = new List<Vector2>();
    List<int> Votes = new List<int>();
    public int check = 0;
    int screenHeight;
    int screenWidth;
    int[] top = new int[3];

    //Resulting list consisting of 1's at screen position
    public List<int> corners = new List<int>();

    // Runs Before Update
    void Awake()
    {
        int size = (int) Screen.height/3;
        TopLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        TopRight.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        BoottomLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }

    // Update is called once per frame
    void Update()
    {
        
        //Works on different parts of calibation depending on the current interval
        if (start==0){ 
            //Waits while kinect camera feed is not active SOMETIMES DOES NOT WORK
            img = mMultiSource.GetColorTexture().GetPixels();
            if (Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05){
                start++;
                screenHeight = mMultiSource.ColorHeight;
                screenWidth = mMultiSource.ColorWidth;
                print("Dimensions");print(screenHeight*screenWidth);
            }
        } else if (start==1){
            //Initializes the corners list to all 1's
            for (int i = 0; i < screenHeight * screenWidth; i++){
                corners.Add(1);
            }
            start++;
        } else if (start==2){
            //Saves the unaltered current view of the kinect camera
            saveImg = mMultiSource.GetColorTexture();
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgraw.png");
            start++;
        } else if (start==3){
            Calibrate(); //Turn screen image to black and white
            Texture2D saveImg = mMultiSource.GetColorTexture();
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgqr.png"); //Saves B&W image
            start++;
        } else if (start==4){
            Fiducial();  //Finds qr fiducuals, if they exist
            start++;
            print("Length: ");
            print(Centers.Count);
        } else if (start==5){
            start++;
            FindTop();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            Vector2 center1;
            for (int x = 0; x < screenHeight; x++){
                for (int y = 0; y < screenWidth; y++){
                    int i = x * screenWidth + y;
                    //Replace with top 3
                    foreach (int centerIndex in top){
                        center1 = Centers[centerIndex];
                        if (Math.Abs(x-center1.x) < 4 & Math.Abs(y-center1.y) < 4){
                            img[i] = Color.green;
                        }
                    }
                }
            }
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/foundqr.png");
        }
    }

    int Sum(int[] counts){
        int result = 0;
        foreach (int count in counts){
            result+=count;
        }
        return result;
    }

    void Fiducial(){
        //Looks for qr squares and returns center points
        int[] counts = new int[5];
        int counting = 0;
        int i;
        int total = 0;
        Vector2 center = Vector2.zero;
        for (int x = 0; x < screenHeight; x++){
            counts = new int[5];
            counting = 0;
            for (int y = 0; y < screenWidth; y++){
                i = x * screenWidth + y;
                switch (counting) {
                    case 0:
                        if (img[i]==Color.black){
                            counts[0]++;
                        } else {
                            counting++;
                        }
                        break;
                    case 1:
                        if (img[i]==Color.white){
                            counts[1]++;
                        } else {
                            counting++;
                        }
                        break;
                    case 2:
                        if (img[i]==Color.black){
                            counts[2]++;
                        } else {
                            counting++;
                        }
                        break;
                    case 3:
                        if (img[i]==Color.white){
                            counts[3]++;
                        } else {
                            counting++;
                        }
                        break;
                    case 4: 
                        if (img[i]==Color.black){
                            counts[4]++;
                        } else {
                            check+=1;
                            total = Sum(counts);
                            if (Verify(counts)==1){ //If the ratios match a qr code
                                center.x = x;
                                center.y = y - total/2;
                                int[] vertical = Vertical(x * screenWidth + (int) center.y); //Gets the color counts for the portential qr point vertically
                                //If vertical matches qr ratio and vertical size matches horizontal size and the center has been found for the first time
                                if (Verify(vertical)==1 & Math.Abs(Sum(vertical)-total) < total * 0.5){ 
                                    //center = OptimizeCenter();
                                    if (UniqueCenter(center, total)==1){
                                        Centers.Add(center);
                                        Votes.Add(0);
                                        counting = 0; 
                                    }
                                    
                                    counts = new int[5];
                                }
                            }
                            total = 0;
                            counting = 3;
                            counts[0] = counts[2];
                            counts[1] = counts[3];
                            counts[2] = counts[4];
                            counts[3] = 0;
                            counts[4] = 0;
                        }
                        break;
                }
            }
        }
    }
    
    Vector2 OptimizeCenter(){
        //TODO
        return Vector2.zero;
    }

    void FindTop(){
        //Finds the indexes of the centers with the top 3 votes
        top[0]=0;top[1]=1;top[2]=2; //Init top list to -1
        Array.Sort(top);
        for (int i = 2; i < Votes.Count; i++){
            if (Votes[i] > Votes[top[2]]){
                top[2] = i;
            }
            if (Votes[top[2]] > Votes[top[1]]){
                top[2] = top[1];
                top[1] = i;
            }
            if (Votes[top[1]] > Votes[top[0]]){
                top[1] = top[0];
                top[0] = i;
            }
        }
    }

    int UniqueCenter(Vector2 center, int total){
        //Checks if a qr center has been found already
        Vector2 foundCenter;
        for(int i = 0; i < Centers.Count; i++){
            foundCenter = Centers[i];
            if (Math.Abs(foundCenter.x - center.x) < total & Math.Abs(foundCenter.y-center.y) < total){
                Votes[i]+=1;
                return 0;
            }
        }
        return 1;
    }

    int Verify(int[] counts){
        //Checks if list is a valid qr
        if (Math.Abs(counts[0]-counts[4]) > Math.Min(counts[0], counts[4]) * 1.5){
            return 0;
        }
        if (Math.Abs(counts[1]-counts[3]) > Math.Min(counts[1], counts[3]) * 1.5){
            return 0;
        }
        if (counts[2] > (counts[0]+counts[1]+counts[3]+counts[4])){
            return 0;
        }
        return 1;
    }

    int[] Vertical(int i){
        //Counts black and white pixels vertically instead of horizontally
        //Checks vertically as well
        int[] vertical = new int[5]; 
        int counting = 0;
        int temp = i;
        bool loop = true;
        //First go up
        while (loop){
            temp+=screenWidth;
            if (temp < 0 | temp > screenWidth * screenHeight){
                break;
            }
            switch (counting){
                case 0:
                    if (img[temp]==Color.black){
                        vertical[2]+=1;
                    } else{
                        counting++;
                    }
                    break;
                case 1:
                    if (img[temp]==Color.white){
                        vertical[1]+=1;
                    } else{
                        counting++;
                    }
                    break;
                case 2:
                    if (img[temp]==Color.black){
                        vertical[0]+=1;
                    } else{
                        loop = false;
                    }
                    break;
            }
        }
        counting = 0;
        temp = i;
        loop = true;
        //Then go down
        while (loop){
            temp-=screenWidth;
            if (temp < 0 | temp > screenWidth * screenHeight){
                break;
            }
            switch (counting){
                case 0:
                    if (img[temp]==Color.black){
                        vertical[2]+=1;
                    } else{
                        counting++;
                    }
                    break;
                case 1:
                    if (img[temp]==Color.white){
                        vertical[3]+=1;
                    } else{
                        counting++;
                    }
                    break;
                case 2:
                    if (img[temp]==Color.black){
                        vertical[4]+=1;
                    } else{
                        loop = false;
                    }
                    break;
            }
        }
        return vertical;
    }

    void Calibrate(double e = .995){
        //Uses the camera feed to find qr square
        img = mMultiSource.GetColorTexture().GetPixels();
        //Changes each pixel to either black or white depending on threshold e
        for (int i = 0; i < screenHeight * screenWidth; i++){
            if (img[i].r < e & img[i].g < e & img[i].b < e){
                img[i] = Color.black;
            } else {
                img[i] = Color.white;
            }
        }
    }

    public void SaveTextureAsPNG(Texture2D _texture, string _fullPath){
        //Saves Textures to a PNG file
        byte[] _bytes =_texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
    }
}
