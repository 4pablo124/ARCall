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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurfaceTracking
{
    public class UIController : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;
        public TouchController TouchControl;
        public Button BackButton;

        private SurfaceTrackerFrameFilter tracker;

        private void Awake()
        {
            tracker = Session.GetComponentInChildren<SurfaceTrackerFrameFilter>();
            TouchControl.TurnOn(TouchControl.transform, Camera.main, false, false, true, true);

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
                "CenterMode: " + Session.CenterMode + Environment.NewLine +
                Environment.NewLine +
                "Gesture Instruction" + Environment.NewLine +
                "\tMove on Surface: One Finger Move" + Environment.NewLine +
                "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                "\tScale: Two Finger Pinch";

            if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                var touch = Input.touches[0];
                if (touch.phase == TouchPhase.Moved)
                {
                    var viewPoint = new Vector2(touch.position.x / Screen.width, touch.position.y / Screen.height);
                    var coord = Session.ImageCoordinatesFromScreenCoordinates(viewPoint);
                    if (tracker && tracker.Tracker != null && coord.OnSome)
                    {
                        tracker.Tracker.alignTargetToCameraImagePoint(coord.Value.ToEasyARVector());
                    }
                }
            }
        }

        public void SwitchCenterMode()
        {
            while (true)
            {
                Session.CenterMode = (ARSession.ARCenterMode)(((int)Session.CenterMode + 1) % Enum.GetValues(typeof(ARSession.ARCenterMode)).Length);
                if (Session.CenterMode == ARSession.ARCenterMode.Camera ||
                    Session.CenterMode == ARSession.ARCenterMode.WorldRoot)
                {
                    break;
                }
            }
        }
    }
}
