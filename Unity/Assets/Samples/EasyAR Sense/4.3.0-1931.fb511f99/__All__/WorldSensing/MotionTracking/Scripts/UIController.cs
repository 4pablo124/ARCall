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

namespace MotionTracking
{
    public class UIController : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;
        public Button UnlockPlaneButton;
        public GameObject Plane;
        public TouchController TouchControl;
        public Button BackButton;

        private VIOCameraDeviceUnion vioCamera;

        private void Awake()
        {
            vioCamera = Session.GetComponentInChildren<VIOCameraDeviceUnion>();
            vioCamera.DeviceCreated += () =>
            {
                if (vioCamera.Device.Type() == typeof(MotionTrackerCameraDevice))
                {
                    TouchControl.TurnOn(TouchControl.gameObject.transform, Session.Assembly.Camera, false, false, true, true);
                }
                else
                {
                    TouchControl.TurnOn(TouchControl.gameObject.transform, Session.Assembly.Camera, true, true, true, true);
                    UnlockPlaneButton.gameObject.SetActive(false);
                }
            };

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
                "CenterMode: " + Session.CenterMode + Environment.NewLine;

            if (vioCamera.Device != null)
            {
                if (vioCamera.Device.Type() == typeof(MotionTrackerCameraDevice))
                {
                    Status.text += Environment.NewLine +
                    "Gesture Instruction" + Environment.NewLine +
                    "\tMove on Detected Plane: One Finger Move" + Environment.NewLine +
                    "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                    "\tScale: Two Finger Pinch";
                }
                else
                {
                    Status.text += Environment.NewLine +
                    "Gesture Instruction" + Environment.NewLine +
                    "\tMove in View: One Finger Move" + Environment.NewLine +
                    "\tMove Near/Far: Two Finger Vertical Move" + Environment.NewLine +
                    "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                    "\tScale: Two Finger Pinch";
                }
            }

            if (vioCamera.Device != null && vioCamera.Device.Type() == typeof(MotionTrackerCameraDevice))
            {
                if (!UnlockPlaneButton.interactable)
                {
                    var viewPoint = new Vector2(0.5f, 0.333f);
                    var points = vioCamera.HitTestAgainstHorizontalPlane(viewPoint);
                    if (points.Count > 0)
                    {
                        var viewportPoint = Camera.main.WorldToViewportPoint(Plane.transform.position);
                        if (!Plane.activeSelf || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || Mathf.Abs(Plane.transform.position.y - points[0].y) > 0.15)
                        {
                            Plane.SetActive(true);
                            Plane.transform.position = points[0];
                            Plane.transform.localScale = Vector3.one * (Session.Assembly.CameraRoot.position - points[0]).magnitude;
                        }
                    }
                }

                if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    var touch = Input.touches[0];
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            TouchControl.transform.position = hitInfo.point;
                            UnlockPlaneButton.interactable = true;
                        }
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

        public void UnlockPlane()
        {
            UnlockPlaneButton.interactable = false;
        }
    }
}
