//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using Common;
using easyar;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MotionTracking_ImageTarget
{
    public class UIController : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;
        public TouchController TouchControl;
        public Button BackButton;

        private VIOCameraDeviceUnion vioCamera;
        private ImageTargetController controllerNamecard;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void ImportSampleStreamingAssets()
        {
            FileUtil.ImportSampleStreamingAssets();
        }
#endif

        private void Awake()
        {
            vioCamera = Session.GetComponentInChildren<VIOCameraDeviceUnion>();
            controllerNamecard = GameObject.Find("ImageTarget").GetComponent<ImageTargetController>();
            TouchControl.TurnOn(TouchControl.gameObject.transform, Camera.main, true, true, true, true);

            var launcher = "AllSamplesLauncher";
            if (Application.CanStreamedLevelBeLoaded(launcher))
            {
                var button = BackButton.GetComponent<Button>();
                button.onClick.AddListener(() => { UnityEngine.SceneManagement.SceneManager.LoadScene(launcher); });
            }
            else
            {
                BackButton.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            Status.text = "Device Model: " + SystemInfo.deviceModel + Environment.NewLine +
                "VIO Device Type: " + (vioCamera.Device == null ? "-" : vioCamera.Device.DeviceType.ToString()) + Environment.NewLine +
                "Tracking Status: " + (Session.WorldRootController == null ? "-" : Session.WorldRootController.TrackingStatus.ToString()) + Environment.NewLine +
                "CenterMode: " + Session.CenterMode + Environment.NewLine +
                Environment.NewLine +
                "Gesture Instruction" + Environment.NewLine +
                "\tMove in View: One Finger Move" + Environment.NewLine +
                "\tMove Near/Far: Two Finger Vertical Move" + Environment.NewLine +
                "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                "\tScale: Two Finger Pinch";
        }

        public void SwitchCenterMode()
        {
            while (true)
            {
                Session.CenterMode = (ARSession.ARCenterMode)(((int)Session.CenterMode + 1) % Enum.GetValues(typeof(ARSession.ARCenterMode)).Length);
                if (Session.CenterMode == ARSession.ARCenterMode.SpecificTarget)
                {
                    Session.CenterTarget = controllerNamecard;
                }
                if (Session.CenterMode == ARSession.ARCenterMode.FirstTarget ||
                    Session.CenterMode == ARSession.ARCenterMode.Camera ||
                    Session.CenterMode == ARSession.ARCenterMode.SpecificTarget ||
                    Session.CenterMode == ARSession.ARCenterMode.WorldRoot)
                {
                    break;
                }
            }
        }
    }
}
