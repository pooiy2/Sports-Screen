using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public MeasureDepth mMeasureDepth;
    public MultiSourceManager mMultiSource;

    public RawImage mRawImage;
    public RawImage mRawDepth;

    
    //Gets color camera and depth camera from kinect

    // Update is called once per frame
    void Update()
    {
        mRawImage.texture = mMultiSource.GetColorTexture();

        mRawDepth.texture = mMeasureDepth.mDepthTexture;
    }
}
