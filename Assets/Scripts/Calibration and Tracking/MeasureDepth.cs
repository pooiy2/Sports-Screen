using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

//Screen calibration from tutorial

public class MeasureDepth : MonoBehaviour
{
    public delegate void NewTriggerPoints(List<Vector2> triggerPoints);
    public static event NewTriggerPoints OnTriggerPoints = null;

    public MultiSourceManager mMultiSource;
    public Texture2D mDepthTexture;

    //Cutoffs
    [Range(0, 5.0f)]
    public float mDepthSensitivity = 0;
    [Range(0, 5.0f)]
    public float mOffWall = 0;

    [Range(-50, 50f)]
    public float mWallDepth = -50;

    [Header("Top and Bottom")]
    [Range(-5, 5.0f)]
    public float mTopCutOff = 1;
    [Range(-5, 5.0f)]
    public float mBottomCutOff = -1;

    [Header("Left and Right")]
    [Range(-5, 5.0f)]
    public float mLeftCutOff = -1;
    [Range(-5, 5.0f)]
    public float mRightCutOff = 1;

    //Depth Data
    private ushort[] mDepthData = null;
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;
    private List<ValidPoint> mValidPoints = null;
    //private List<ValidPoint> mOrigin = null;
    private List<Vector2> mTriggerPoints = null;

    //Kinect
    private KinectSensor mSensor = null;
    private CoordinateMapper mMapper = null;
    private Camera mCamera = null;

    private readonly Vector2Int mDepthResolution = new Vector2Int(512, 424);
    private Rect mRect;
    
    private void Awake(){
        mSensor = KinectSensor.GetDefault();
        mMapper = mSensor.CoordinateMapper;
        mCamera = Camera.main;

        int arraySize = mDepthResolution.x * mDepthResolution.y;

        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints  = new ColorSpacePoint[arraySize];
    }

    private void Update(){
        mValidPoints = DepthToColor();
        /*
        mTriggerPoints  = FilterToTrigger(mValidPoints);

        if(OnTriggerPoints!=null && mTriggerPoints.Count!=0){
            OnTriggerPoints(mTriggerPoints);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            
            
            mRect = CreateRect(mValidPoints);

            mDepthTexture = CreateTexture(mValidPoints);

            //mOrigin = mValidPoints;
        }
        */
    }

    private void OnGUI(){
        GUI.Box(mRect, "");

        if(mTriggerPoints==null){
            return;
        }
        foreach(Vector2 point in mTriggerPoints){
            Rect rect = new Rect(point, new Vector2(10, 10));
            GUI.Box(rect, "");
        }
    }

    private List<ValidPoint> DepthToColor(){
        //Maps rgb colorspace to depth space

        //Points to return, color data with depth data
        List<ValidPoint> ValidPoints = new List<ValidPoint>();

        //Get depth
        mDepthData = mMultiSource.GetDepthData();

        //Map
        mMapper.MapDepthFrameToCameraSpace(mDepthData, mCameraSpacePoints);
        mMapper.MapDepthFrameToColorSpace(mDepthData, mColorSpacePoints);

        //Filter
        for (int i = 0; i < mDepthResolution.x; i++){
            for (int j = 0; j < mDepthResolution.y; j++){

                    //Lower resolution of depth
                    int sampleIndex = (j * mDepthResolution.x) + i;
                    sampleIndex*=1;

                    //Cutoff Tests
                    if (mCameraSpacePoints[sampleIndex].X < mLeftCutOff){
                        continue;
                    }

                    if (mCameraSpacePoints[sampleIndex].X > mRightCutOff){
                        continue;
                    }

                    if (mCameraSpacePoints[sampleIndex].Y > mTopCutOff){
                        continue;
                    }

                    if (mCameraSpacePoints[sampleIndex].Y < mBottomCutOff){
                        continue;
                    }

                    //Add depth to point
                    ValidPoint newPoint = new ValidPoint(mColorSpacePoints[sampleIndex], mCameraSpacePoints[sampleIndex].Z);

                    //Depth test
                    if (mCameraSpacePoints[sampleIndex].Z >= mWallDepth){
                        newPoint.mWithinWallDepth = true;
                    }

                    ValidPoints.Add(newPoint);
            }
        }
        return ValidPoints;
    }

    private List<Vector2> FilterToTrigger(List<ValidPoint> validPoints){
        List<Vector2> triggerPoints = new List<Vector2>();

        foreach(ValidPoint point in validPoints){
            if (!point.mWithinWallDepth){
                if(mWallDepth - mOffWall > point.z & point.z > mWallDepth - mDepthSensitivity){
                    Vector2 screenPoint = ScreenToCamera(new Vector2(point.colorSpace.X, point.colorSpace.Y));
                    
                    triggerPoints.Add(screenPoint); 
                }
            }
        }

        return triggerPoints;
    }

    private Texture2D CreateTexture(List<ValidPoint> ValidPoints){
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false);
        
        for (int x=0; x<1920; x++){
            for (int y=0; y<1080; y++){
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        foreach(ValidPoint point in ValidPoints){
            newTexture.SetPixel((int)point.colorSpace.X, (int)point.colorSpace.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }

    #region Rect Creation
    private Rect CreateRect(List<ValidPoint> points){
        if(points.Count==0){
            return new Rect();
        }
        //Get corners of rect
        Vector2 topLeft = GetTopLeft(points);
        Vector2 bottomRight = GetBottomRight(points);

        //Translaste to viewport
        Vector2 screenTopLeft = ScreenToCamera(topLeft);
        Vector2 screenBottomRight = ScreenToCamera(bottomRight);

        //Rect Dimensions
        int width = (int)(screenBottomRight.x - screenTopLeft.x);
        int height = (int)(screenBottomRight.y - screenTopLeft.y);

        //Create
        Vector2 size = new Vector2(width, height);
        Rect rect = new Rect(screenTopLeft, size);

        return rect;
    }

    private Vector2 GetTopLeft(List<ValidPoint> points){
        Vector2 topLeft = new Vector2(int.MaxValue, int.MaxValue);
        foreach(ValidPoint point in points){
            //Left most x 
            if(point.colorSpace.X < topLeft.x){
                topLeft.x = point.colorSpace.X;
            }

            //Top most y
            if(point.colorSpace.Y < topLeft.y){
                topLeft.y = point.colorSpace.Y;
            }
        }
        return topLeft;
    }

    private Vector2 GetBottomRight(List<ValidPoint> points){
        Vector2 bottomRight = new Vector2(int.MinValue, int.MinValue);
        foreach(ValidPoint point in points){
            //Right most x 
            if(point.colorSpace.X > bottomRight.x){
                bottomRight.x = point.colorSpace.X;
            }

            //Bottom most y
            if(point.colorSpace.Y > bottomRight.y){
                bottomRight.y = point.colorSpace.Y;
            }
        }
        return bottomRight;
    }

    private Vector2 ScreenToCamera(Vector2 screenPosition){
        Vector2 normalizedScreen = new Vector2(Mathf.InverseLerp(0, 1920, screenPosition.x), Mathf.InverseLerp(0, 1080, screenPosition.y));

        Vector2 screenPoint = new Vector2(normalizedScreen.x * mCamera.pixelWidth, normalizedScreen.y * mCamera.pixelHeight);

        return screenPoint;
    }

    #endregion
}

public class ValidPoint{
    public ColorSpacePoint colorSpace;
    public float z = 0.0f;

    public bool mWithinWallDepth = false;

    public ValidPoint(ColorSpacePoint newColorSpace, float newZ){
        colorSpace = newColorSpace;
        z = newZ;
    }

}