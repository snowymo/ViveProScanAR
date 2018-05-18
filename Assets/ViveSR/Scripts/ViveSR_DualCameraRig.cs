//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Vive.Plugin.SR
{
    [ExecuteInEditMode]
    public class ViveSR_DualCameraRig : MonoBehaviour
    {
        public Camera OriginalCamera;
        public Camera VirtualCamera;
        public Camera DualCameraLeft;
        public Camera DualCameraRight;
        public ViveSR_DualCameraImageRenderer DualCameraImageRenderer;
        public ViveSR_DualCameraCalibrationTool DualCameraCalibration;
        public ViveSR_TrackedCamera TrackedCameraLeft;
        public ViveSR_TrackedCamera TrackedCameraRight;
        public ViveSR_HMDCameraShifter HMDCameraShifter;

        public DualCameraDisplayMode Mode = DualCameraDisplayMode.MIX;
        public static DualCameraStatus DualCameraStatus { get; private set; }
        public static string LastError { get; private set; }

        [HideInInspector] public List<UnityAction> OnInitialComplete = new List<UnityAction>();
        [HideInInspector] public List<UnityAction> OnInitialFailed = new List<UnityAction>();

        private ViveSR_DualCameraRig() { }
        private static ViveSR_DualCameraRig Mgr = null;
        public static ViveSR_DualCameraRig Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR_DualCameraRig>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("ViveSR_DualCameraManager does not be attached on GameObject");
                }
                return Mgr;
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isEditor) ViveSR_Settings.Update();
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                SetMode(DualCameraDisplayMode.MIX);
            else if (Input.GetKeyDown(KeyCode.F2))
                SetMode(DualCameraDisplayMode.VIRTUAL);
            else if (Input.GetKeyDown(KeyCode.F3))
                SetMode(DualCameraDisplayMode.REAL);
        }

        public void Initial()
        {
            DualCameraStatus = DualCameraStatus.IDLE;
            if (ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
                StartCoroutine(InitialCoroutine());
        }

        public void Release()
        {
            DualCameraStatus = DualCameraStatus.IDLE;
            if (DualCameraCalibration != null)
                DualCameraCalibration.SaveDeviceParameter();
        }

        IEnumerator InitialCoroutine()
        {
            //if (VirtualCamera == null || DualCameraLeft == null || DualCameraRight == null || DualCameraImageRenderer == null)
            //{
            //    SetLastError("[ViveSR] Please check references");
            //    for (int i = 0; i < OnInitialFailed.Count; i++) if (OnInitialFailed[i] != null) OnInitialFailed[i]();
            //    yield break;
            //}

            int result = ViveSR_DualCameraImageCapature.Initial();
            if (result != (int)Error.WORK)
            {
                SetLastError("[ViveSR] Initial Camera error " + result);
                for (int i = 0; i < OnInitialFailed.Count; i++) if (OnInitialFailed[i] != null) OnInitialFailed[i]();
                yield break;
            }
            if (ViveSR_Framework.MODULE_ID_DEPTH != 0)
                ViveSR_DualCameraDepthExtra.InitialDepthCollider(ViveSR_DualCameraImageCapature.DepthImageWidth,
                                                                 ViveSR_DualCameraImageCapature.DepthImageHeight);

            if (DualCameraCalibration != null)
            {
                DualCameraCalibration.LoadDeviceParameter();
            }
            if (TrackedCameraLeft != null)
            {
                if (TrackedCameraLeft.ImagePlane != null) TrackedCameraLeft.ImagePlane.Initial();
                if (TrackedCameraLeft.ImagePlaneCalibration != null)
                {
                    TrackedCameraLeft.ImagePlaneCalibration.Initial();
                    TrackedCameraLeft.ImagePlaneCalibration.gameObject.SetActive(false);
                }
            }
            if (TrackedCameraRight != null)
            {
                if (TrackedCameraRight.ImagePlane != null) TrackedCameraRight.ImagePlane.Initial();
                if (TrackedCameraRight.ImagePlaneCalibration != null)
                {
                    TrackedCameraRight.ImagePlaneCalibration.Initial();
                    TrackedCameraRight.ImagePlaneCalibration.gameObject.SetActive(false);
                }
            }
            DualCameraStatus = DualCameraStatus.WORKING;
            for (int i = 0; i < OnInitialComplete.Count; i++) if (OnInitialComplete[i] != null) OnInitialComplete[i]();
            SetMode(Mode);
        }

        /// <summary>
        /// Decide whether real/virtual camera render or not.
        /// </summary>
        /// <param name="mode">Virtual, Real and Mix</param>
        public void SetMode(DualCameraDisplayMode mode)
        {
            if (OriginalCamera == null)
            {
                if (Camera.main == VirtualCamera) VirtualCamera.tag = "Untagged";
                OriginalCamera = Camera.main;
                VirtualCamera.tag = "MainCamera";
            }
            switch (mode)
            {
                case DualCameraDisplayMode.VIRTUAL:
                    if (OriginalCamera != VirtualCamera && OriginalCamera != null) OriginalCamera.enabled = true;
                    EnableViveCamera(false);
                    break;
                case DualCameraDisplayMode.REAL:
                    if (OriginalCamera != VirtualCamera && OriginalCamera != null) OriginalCamera.enabled = false;
                    EnableViveCamera(true, DualCameraMode.REAL);
                    break;
                case DualCameraDisplayMode.MIX:
                    if (OriginalCamera != VirtualCamera && OriginalCamera != null) OriginalCamera.enabled = false;
                    EnableViveCamera(true, DualCameraMode.MIX);
                    break;
            }
        }

        private void EnableViveCamera(bool active, DualCameraMode mode = DualCameraMode.MIX)
        {
            DualCameraImageRenderer.enabled = active;
            VirtualCamera.gameObject.SetActive(mode == DualCameraMode.MIX ? active : false);
            DualCameraLeft.gameObject.SetActive(active);
            DualCameraRight.gameObject.SetActive(active);
            TrackedCameraLeft.gameObject.SetActive(active);
            TrackedCameraRight.gameObject.SetActive(active);
        }

        private void SetLastError(string errMsg)
        {
            DualCameraStatus = DualCameraStatus.ERROR;
            LastError = errMsg;
            Debug.LogError(errMsg);
        }
    }
}