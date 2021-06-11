//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

package com.example.externalcamera;

public class ExternalCameraParameters {
    private int mWidth = 0;
    private int mHeight = 0;
    private int mOrientation = 0;
    private int mCameraType = 0;
    private long mTimestamp;

    public  void setWidth(int val)
    {
        mWidth = val;
    }

    public  void setHeight(int val)
    {
        mHeight = val;
    }

    public  void setOrientation(int val)
    {
        mOrientation = val;
    }

    public  void setCameraType(int val)
    {
        mCameraType = val;
    }

    public void setTimestamp(long val) { mTimestamp = val; }

    public  int getWidth()
    {
        return mWidth;
    }

    public int getHeight()
    {
        return mHeight;
    }

    public int getOrientation()
    {
        return mOrientation;
    }

    public int getCameraType() { return mCameraType;}

    public long getTimestamp() {return  mTimestamp;}
}
