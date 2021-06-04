﻿namespace UnityEngine.XR.Mock.Example
{
    public class CameraProjectionMatrixUpdater : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Camera m_ARCamera;
#pragma warning restore

        void Update()
        {
            if (m_ARCamera == null)
                return;

            // Force the camera to recompute its projection matrix
            // before passing it off to the ARCamera. This allows us
            // to handle viewport changes, which affect the projection
            // matrix.
            m_ARCamera.ResetProjectionMatrix();
            CameraApi.projectionMatrix = m_ARCamera.projectionMatrix;
        }
    }
}
