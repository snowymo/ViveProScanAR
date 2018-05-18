//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    public class ViveSR_DualCameraImageRenderer : MonoBehaviour
    {
        public static bool UpdateDistortedMaterial
        {
            get { return _UpdateDistortedMaterial; }
            set { if (value != _UpdateDistortedMaterial) _UpdateDistortedMaterial = value; SetCallbackEnable(_CallbackMode); }
        }
        public static bool UpdateUndistortedMaterial
        {
            get { return _UpdateUndistortedMaterial; }
            set { if (value != _UpdateUndistortedMaterial) _UpdateUndistortedMaterial = value; SetCallbackEnable(_CallbackMode); }
        }
        public static bool UpdateDepthMaterial
        {
            get { return _UpdateDepthMaterial; }
            set { if (value != _UpdateDepthMaterial) _UpdateDepthMaterial = value; SetCallbackEnable(_CallbackMode); }
        }
        public static UndistortionMethod UndistortMethod
        {
            get { return _UndistortMethod; }
            set { if (value != _UndistortMethod) _UndistortMethod = value; SetUndistortMode(_UndistortMethod); }
        }
        public static bool CallbackMode
        {
            get { return _CallbackMode; }
            set { if (value != _CallbackMode) SetCallbackEnable(value);}
        }
        private static bool _UpdateDistortedMaterial = false;
        private static bool _UpdateUndistortedMaterial = false;
        private static bool _UpdateDepthMaterial = false;
        private static bool _CallbackMode = false;
        private static UndistortionMethod _UndistortMethod = UndistortionMethod.DEFISH_BY_MESH;

        public List<Material> DistortedLeft;
        public List<Material> DistortedRight;
        public List<Material> UndistortedLeft;
        public List<Material> UndistortedRight;
        public List<Material> Depth;

        private ViveSR_Timer DistortedTimer = new ViveSR_Timer();
        private ViveSR_Timer UndistortedTimer = new ViveSR_Timer();
        private ViveSR_Timer DepthTimer = new ViveSR_Timer();
        public static float RealDistortedFPS;
        public static float RealUndistortedFPS;
        public static float RealDepthFPS;
        private int LastDistortedTextureUpdateTime = 0;
        private int LastUndistortedTextureUpdateTime = 0;
        private int LastDepthTextureUpdateTime = 0;

        private void Start()
        {
            SetUndistortMode(_UndistortMethod);            
        }

        private void Update()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING)
            {
                if (!CallbackMode)
                {
                    if (UpdateDistortedMaterial) ViveSR_DualCameraImageCapature.UpdateDistortedImage();
                    if (UpdateUndistortedMaterial) ViveSR_DualCameraImageCapature.UpdateUndistortedImage();
                    if (UpdateDepthMaterial) ViveSR_DualCameraImageCapature.UpdateDepthImage();
                }

                #region Distorted Image
                if (_UpdateDistortedMaterial)
                {
                    int currentCameraTimeIndex = ViveSR_DualCameraImageCapature.DistortedTimeIndex;
                    if (currentCameraTimeIndex != LastDistortedTextureUpdateTime)
                    {
                        DistortedTimer.Add(currentCameraTimeIndex - LastDistortedTextureUpdateTime);
                        RealDistortedFPS = 1000 / DistortedTimer.AverageLeast(100);
                        int frameIndex, timeIndex;
                        Texture2D textureCameraLeft, textureCameraRight;
                        ViveSR_DualCameraImageCapature.GetDistortedTexture(out textureCameraLeft, out textureCameraRight, out frameIndex, out timeIndex);
                        for (int i = 0; i < DistortedLeft.Count; i++)
                        {
                            if (DistortedLeft[i] != null)
                                DistortedLeft[i].mainTexture = textureCameraLeft;
                        }
                        for (int i = 0; i < DistortedRight.Count; i++)
                        {
                            if (DistortedRight[i] != null)
                                DistortedRight[i].mainTexture = textureCameraRight;
                        }
                        LastDistortedTextureUpdateTime = currentCameraTimeIndex;

                        ViveSR_DualCameraRig.Instance.TrackedCameraLeft.transform.localPosition = ViveSR_DualCameraImageCapature.GetDistortedLocalPosition(DualCameraIndex.LEFT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraLeft.transform.localRotation = ViveSR_DualCameraImageCapature.GetDistortedLocalRotation(DualCameraIndex.LEFT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraRight.transform.localPosition = ViveSR_DualCameraImageCapature.GetDistortedLocalPosition(DualCameraIndex.RIGHT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraRight.transform.localRotation = ViveSR_DualCameraImageCapature.GetDistortedLocalRotation(DualCameraIndex.RIGHT);
                    }
                }
                #endregion

                #region Undistorted Image
                if (_UpdateUndistortedMaterial)
                {
                    int currentUndistortedTimeIndex = ViveSR_DualCameraImageCapature.UndistortedTimeIndex;
                    if (currentUndistortedTimeIndex != LastUndistortedTextureUpdateTime)
                    {
                        UndistortedTimer.Add(currentUndistortedTimeIndex - LastUndistortedTextureUpdateTime);
                        RealUndistortedFPS = 1000 / UndistortedTimer.AverageLeast(100);
                        int frameIndex, timeIndex;
                        Texture2D textureUndistortedLeft, textureUndistortedRight;
                        ViveSR_DualCameraImageCapature.GetUndistortedTexture(out textureUndistortedLeft, out textureUndistortedRight, out frameIndex, out timeIndex);
                        for (int i = 0; i < UndistortedLeft.Count; i++)
                        {
                            if (UndistortedLeft[i] != null)
                                UndistortedLeft[i].mainTexture = textureUndistortedLeft;
                        }
                        for (int i = 0; i < UndistortedRight.Count; i++)
                        {
                            if (UndistortedRight[i] != null)
                                UndistortedRight[i].mainTexture = textureUndistortedRight;
                        }
                        LastUndistortedTextureUpdateTime = currentUndistortedTimeIndex;

                        ViveSR_DualCameraRig.Instance.TrackedCameraLeft.transform.localPosition = ViveSR_DualCameraImageCapature.GetUndistortedLocalPosition(DualCameraIndex.LEFT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraLeft.transform.localRotation = ViveSR_DualCameraImageCapature.GetUndistortedLocalRotation(DualCameraIndex.LEFT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraRight.transform.localPosition = ViveSR_DualCameraImageCapature.GetUndistortedLocalPosition(DualCameraIndex.RIGHT);
                        ViveSR_DualCameraRig.Instance.TrackedCameraRight.transform.localRotation = ViveSR_DualCameraImageCapature.GetUndistortedLocalRotation(DualCameraIndex.RIGHT);
                    }
                }
                #endregion

                #region Depth Image
                if (_UpdateDepthMaterial)
                {
                    int currentDepthTimeIndex = ViveSR_DualCameraImageCapature.DepthTimeIndex;
                    if (currentDepthTimeIndex != LastDepthTextureUpdateTime)
                    {
                        DepthTimer.Add(currentDepthTimeIndex - LastDepthTextureUpdateTime);
                        RealDepthFPS = 1000 / DepthTimer.AverageLeast(100);
                        int frameIndex, timeIndex;
                        Texture2D textureDepth;
                        ViveSR_DualCameraImageCapature.GetDepthTexture(out textureDepth, out frameIndex, out timeIndex);
                        for (int i = 0; i < Depth.Count; i++)
                        {
                            if (Depth[i] != null) Depth[i].mainTexture = textureDepth;
                        }
                        LastDepthTextureUpdateTime = currentDepthTimeIndex;
                    }
                }
                #endregion
            }
        }

        private void OnDisable()
        {
            SetCallbackEnable(false);
        }

        private static void SetUndistortMode(UndistortionMethod method)
        {
            if (_UndistortMethod == UndistortionMethod.DEFISH_BY_SRMODULE)
            {
                UpdateDistortedMaterial = false;
                UpdateUndistortedMaterial = true;
            }
            else
            {
                UpdateDistortedMaterial = true;
                UpdateUndistortedMaterial = false;
            }
            ViveSR_DualCameraRig.Instance.TrackedCameraLeft.ImagePlane.SetUndistortMethod(UndistortMethod);
            ViveSR_DualCameraRig.Instance.TrackedCameraRight.ImagePlane.SetUndistortMethod(UndistortMethod);
        }

        private static void SetCallbackEnable(bool enable)
        {
            if (enable)
            {
                if (UpdateDistortedMaterial) ViveSR_DualCameraImageCapature.RegisterDistortedCallback();
                if (UpdateUndistortedMaterial) ViveSR_DualCameraImageCapature.RegisterUndistortedCallback();
                if (UpdateDepthMaterial) ViveSR_DualCameraImageCapature.RegisterDepthCallback();
            }
            else
            {
                ViveSR_DualCameraImageCapature.UnregisterDistortedCallback();
                ViveSR_DualCameraImageCapature.UnregisterUndistortedCallback();
                ViveSR_DualCameraImageCapature.UnregisterDepthCallback();
            }
            _CallbackMode = enable;
        }
    }
}