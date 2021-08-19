using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

//Initializes screen position and determines when a hit event occurs

public class DepthManager : MonoBehaviour
{
    //Kinect
    private KinectSensor mSensor = null;
    private CoordinateMapper mMapper = null;
    private Camera mCamera = null;


    //Pass ins
    public GameObject Calibration;
    public Event score;
    public MultiSourceManager mMultiSource;
    public Text fps;
    

    //Variables
    Calibration calibration;
    ushort[] depth = null;
    List<int> corners;
    int start = 0;
    ushort[] staticDistance = null;
    private CameraSpacePoint[] world = null;
    float[] distances = null;
    float cutoff;
    float t;
    float wait = 1;

    //Temporary until advanced tracking is added
    public Vector2 topLeft = Vector2.negativeInfinity;
    public Vector2 bottomRight = Vector2.positiveInfinity;
    //public float maxZ = float.NegativeInfinity;
    public float minZ = float.PositiveInfinity;

    public int count = 0;
    public float dots = 5;
    public float distance = 0;
    public float total = 0;
    public double error = 0;
    public Vector2 screenPos = Vector2.zero;
    public int frames = 0;
    public float start_time = 0;

    // Start is called before the first frame update
    void Awake()
    {
        mSensor = KinectSensor.GetDefault();
        mMapper = mSensor.CoordinateMapper;
        mCamera = Camera.main;
        staticDistance = new ushort[1080*1920];
        world = new CameraSpacePoint[1080 * 1920];
        distances = new float[1080*1920];
    }

    void Start(){
        calibration = Calibration.GetComponent <Calibration> ();
        t = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (calibration.start==101){
            corners = calibration.corners;
            calibration.start+=1;
            start = 1;
        }
        if (start==1){
            FindDistance();
            start_time = Time.time;
        } else if (start==2 & Time.time - t > wait){
            if (Time.time - t < 5){
                frames+=1;
            } else {
                fps.text = frames.ToString();
                frames = 1;
                t = Time.time;
            }
            hit();
        }
    }

    void FindDistance(){
        depth = mMultiSource.GetDepthData();
        mMapper.MapColorFrameToCameraSpace(depth, world);
        //Find average of screen distance
        for (int i = 0; i < 1080 * 1920; i++){
            if (corners[i]==1){
                //(j * mDepthResolution.x) + i
                //i is x and j is y
                distance+=world[i].Z;
                total+=1;
                //TEMP find topLeft and bottomRight
                if (world[i].X>topLeft.x){
                    topLeft.x = world[i].X;
                }
                if (world[i].Y>topLeft.y){
                    topLeft.y = world[i].Y;
                }
                if (world[i].X<bottomRight.x){
                    bottomRight.x = world[i].X;
                }
                if (world[i].Y<bottomRight.y){
                    bottomRight.y = world[i].Y;
                }
                if (world[i].Z<minZ){
                    minZ = world[i].Z;
                }
            }
        }
        print("MaxZ " + minZ.ToString());
        //Find the variance and standard deviation
        for (int i = 0; i < 1080 * 1920; i++){
            if (corners[i]==1){
                //(j * mDepthResolution.x) + i
                //i is x and j is y
                error+=Math.Pow(world[i].Z-distance/total, 2);
            }
        }
        cutoff = (float) Math.Pow((float) error/(total-1), 0.5);
        //For now only objects withing the x and y coordinates of the screen will be detected
        //TODO: Add object detection for the entirety of the room
        //Save distances
        for (int i = 0; i < 1080 * 1920; i++){
            if (corners[i]==1){
                //(j * mDepthResolution.x) + i
                //i is x and j is y
                error+=Math.Pow(world[i].Z-distance/total, 2);
            }
        }
        start = 2;
    }

    void hit(){
        count = 0;
        depth = mMultiSource.GetDepthData();
        mMapper.MapColorFrameToCameraSpace(depth, world);
        float avgX = 0;
        float avgY = 0;
        /*
        for (int i = 0; i < 1080 * 1920; i++){
            if (world[i].X > topLeft.x){
                continue;
            }
            if (world[i].X < bottomRight.x){
                continue;
            }
            if (world[i].Y > topLeft.y){
                continue;
            }
            if (world[i].Y < bottomRight.y){
                continue;
            }
            if (world[i].Z > minZ + 0.1){
                continue;
            }
            if (world[i].Z < minZ - 0.3){
                continue;
            }
            count++;
            avgX+=world[i].X;
            avgY+=world[i].Y;
        }*/
        for (int i = 0; i < 1080 * 1920; i+=4){
            if (world[i].X < topLeft.x & world[i].X > bottomRight.x){
                if (world[i].Y < topLeft.y & world[i].Y > bottomRight.y){
                    if (world[i].Z < minZ - 0.05 & world[i].Z > minZ - 0.3){
                        count+=1;
                        avgX+=world[i].X;
                        avgY+=world[i].Y;
                    }
                }
            } 
        }
        if (count!=0){
            print(count.ToString());
        }
        avgX/=count;
        avgY/=count;
        if (count>dots & Time.time - t > wait){
            t = Time.time;
            screenPos.x = (avgX-bottomRight.x)/(topLeft.x-bottomRight.x);
            screenPos.y = (avgY-bottomRight.y)/(topLeft.y-bottomRight.y);
            score.set(screenPos);
        }
    }
}
