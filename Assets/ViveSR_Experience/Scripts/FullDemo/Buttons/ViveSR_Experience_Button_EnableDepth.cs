using UnityEngine;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_EnableDepth : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_EnableDepth _instance;
        public static ViveSR_Experience_Button_EnableDepth instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_EnableDepth>();
                }
                return _instance;
            }
        }

        [Header("Depth Image")]
        [SerializeField] GameObject depthImageGroup;
        [SerializeField] Material depthImageMaterial;
        public GameObject depthImage;

        bool hasPositioned = false;

        bool isMaterialSet = false;

        Texture dualCameraTexture;

        public void ShowDepthImage(bool on)
        {
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = on;
            depthImage.SetActive(on);
        }

        public override void ForceExcuteButton(bool on)
        {
            if (isOn != on) Action(on);
        }

        protected override void UpdateToDo()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING && !isMaterialSet)
            {
                //Assign depthImageMaterial to ViveSR.
                if (ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Count > 0)
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth[0] = depthImageMaterial;
                else
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Add(depthImageMaterial);

                isMaterialSet = true;
            }

            //Position the depth image gameobject.
            if (ViveSR_Experience.targetHand != null && !hasPositioned)
            {
                depthImageGroup.transform.SetParent(ViveSR_Experience.targetHand.transform.Find("Attach_ControllerTip").transform);
                depthImageGroup.transform.localPosition = new Vector3(-0.13f, 0.01f, -0.125f);
                depthImageGroup.transform.localEulerAngles = new Vector3(90, 0, 0);
                depthImageGroup.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);

                hasPositioned = true;
            }
        }

        public override void ActionToDo()
        {
            //Enable - Disable the depth engine.
            ViveSR_DualCameraImageCapature.EnableDepthProcess(isOn);
        }

        public override void ActOnRotator(bool isOn)
        {
            base.ActOnRotator(isOn);
            ShowDepthImage(isOn);
        }     
    }
}
