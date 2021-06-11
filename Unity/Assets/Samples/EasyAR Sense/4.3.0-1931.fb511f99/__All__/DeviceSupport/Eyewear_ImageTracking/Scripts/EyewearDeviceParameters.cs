//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System.Collections.Generic;
using UnityEngine;

namespace Samples
{
    public class EyewearDeviceParameters : MonoBehaviour
    {
        private static string eyewearActionOne = "QUALCOMM A01";
        private static string eyewearBT350 = "EPSON EMBT3S";
        private Dictionary<string, List<RenderCameraParameters>> parameterList = new Dictionary<string, List<RenderCameraParameters>>();

        public VideoCameraDevice cameraDevice;
        public RenderCameraController leftEyeRenderCameraController;
        public RenderCameraController rightEyeRenderCameraController;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void ImportSampleStreamingAssets()
        {
            FileUtil.ImportSampleStreamingAssets();
        }
#endif

        private void Awake()
        {
            if (!cameraDevice)
            {
                return;
            }
            parameterList.Add(eyewearActionOne, new List<RenderCameraParameters>());
            parameterList[eyewearActionOne].Add(Resources.Load<RenderCameraParameters>("Parameters/ShadowCreator ActionOne/LeftEye"));
            parameterList[eyewearActionOne].Add(Resources.Load<RenderCameraParameters>("Parameters/ShadowCreator ActionOne/RightEye"));
            parameterList.Add(eyewearBT350, new List<RenderCameraParameters>());
            parameterList[eyewearBT350].Add(Resources.Load<RenderCameraParameters>("Parameters/Epson BT350/LeftEye"));
            parameterList[eyewearBT350].Add(Resources.Load<RenderCameraParameters>("Parameters/Epson BT350/RightEye"));

            var deviceModel = SystemInfo.deviceModel;
            if (deviceModel == parameterList[eyewearActionOne][0].DeviceModel)
            {
                cameraDevice.Parameters = new CameraParameters(
                    new Vec2I(1280, 960), new Vec2F(647.1996215716641f * 2, 653.2585489590703f * 2),
                    new Vec2F(342.3979149329917f * 2, 260.5633369417149f * 2), cameraDevice.CameraType, 0);

                leftEyeRenderCameraController.ExternalParameters = parameterList[eyewearActionOne][0];
                rightEyeRenderCameraController.ExternalParameters = parameterList[eyewearActionOne][1];
            }
            else if (deviceModel == parameterList[eyewearBT350][0].DeviceModel)
            {
                cameraDevice.CameraSize = new Vector2(1280, 720);

                leftEyeRenderCameraController.ExternalParameters = parameterList[eyewearBT350][0];
                rightEyeRenderCameraController.ExternalParameters = parameterList[eyewearBT350][1];
            }
        }
    }
}
