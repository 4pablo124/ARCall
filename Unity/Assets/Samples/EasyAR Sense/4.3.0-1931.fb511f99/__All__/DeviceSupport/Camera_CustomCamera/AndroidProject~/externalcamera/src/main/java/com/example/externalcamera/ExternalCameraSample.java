//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

package com.example.externalcamera;

import android.graphics.ImageFormat;
import android.graphics.SurfaceTexture;
import android.hardware.Camera;
import android.os.SystemClock;

import java.io.IOException;
import java.util.List;

public class ExternalCameraSample
{
    private Camera mCamera;
    private SurfaceTexture mSurfaceTexture = new SurfaceTexture(0);
    private Camera.Size mPreviewSize;
    private List<Camera.Size> mSupportedPreviewSizes;

    private ExternalCameraParameters mCameraParameters;

    public boolean open()
    {
        int id = 0;
        mCamera = Camera.open(id);
        if(mCamera != null)
        {
            Camera.Parameters params = mCamera.getParameters();
            params.setPreviewFormat(ImageFormat.NV21);
            mSupportedPreviewSizes = params.getSupportedPreviewSizes();
            Camera.Size size = getOptimalPreviewSize(1280, 960);
            params.setPreviewSize(size.width, size.height);
            params.setFocusMode(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE);
            mPreviewSize = params.getPreviewSize();
            mCamera.setParameters(params);

            Camera.CameraInfo info = new Camera.CameraInfo();
            Camera.getCameraInfo(id, info);

            mCameraParameters = new ExternalCameraParameters();
            mCameraParameters.setWidth(mPreviewSize.width);
            mCameraParameters.setHeight(mPreviewSize.height);
            mCameraParameters.setOrientation(info.orientation);
            mCameraParameters.setCameraType(1);//cameraType.Default = 0 , cameraType.back = 1, cameraType.front = 2
            return true;
        }
        return false;
    }

    private float getRatioError(float x, float x0) {
        float a = (x / Math.max(x0, 1) - 1);
        float b = (x0 / Math.max(x, 1) - 1);
        return a * a + b * b;
    }

    private Camera.Size getOptimalPreviewSize(int width, int height) {
        Camera.Size s = null;
        float minError = Float.MAX_VALUE;
        for (Camera.Size size : mSupportedPreviewSizes) {
            float error = getRatioError(width, size.width) + getRatioError(height, size.height);
            if (error < minError) {
                minError = error;
                s = size;
            }
        }
        return s;
    }

    private boolean ready()
    {
        return mCamera != null;
    }

    public static class ByteArrayWrapper
    {
        public byte[] Buffer;
        public int BufferLength;
        public ExternalCameraParameters camParams;
    }
    public interface Callback
    {
        void onPreviewFrame(ByteArrayWrapper dataWrapper);
    }
    public boolean start(final Callback callback)
    {
        if (!ready())
            return false;
        try {
            mCamera.setPreviewTexture(mSurfaceTexture);
            mCamera.setPreviewCallbackWithBuffer(new Camera.PreviewCallback() {
                @Override
                public void onPreviewFrame(byte[] data, Camera camera) {
                    mCameraParameters.setTimestamp(SystemClock.elapsedRealtimeNanos());
                    ByteArrayWrapper wrapper = new ByteArrayWrapper();
                    wrapper.Buffer = data.clone();
                    wrapper.BufferLength = data.length;
                    wrapper.camParams = mCameraParameters;
                    callback.onPreviewFrame(wrapper);
                    camera.addCallbackBuffer(data);
                }
            });
            for (int i = 0; i < 2; i++)
                mCamera.addCallbackBuffer(new byte[mPreviewSize.width*mPreviewSize.height*3/2]);
            mCamera.startPreview();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return true;
    }

    public boolean stop()
    {
        if (!ready())
            return true;
        mCamera.setPreviewCallback(null);
        mCamera.stopPreview();
        mCamera.release();
        mCamera = null;
        return true;
    }

    public ExternalCameraParameters getCameraParameters()
    {
        return mCameraParameters;
    }

    public int getPixelFormat()
    {
        return 2;
    }
}
