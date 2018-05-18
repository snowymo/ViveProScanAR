using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample3_DynamicMesh : MonoBehaviour
    {
        [SerializeField] Material CollisionMaterial;

        bool isMaterialSet;
        [SerializeField] bool ShowDynamicCollision;

        [SerializeField] Text LeftText, RightText, ThrowableText;

        private void Update()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING && !isMaterialSet)
            {                  
                ViveSR_DualCameraDepthCollider.ChangeColliderMaterial(CollisionMaterial);
                CollisionMaterial.color = ShowDynamicCollision ? CollisionMaterial.color = new Color(0, 0, 0, 0.5f) : Color.clear;

                ViveSR_DualCameraImageCapature.EnableDepthProcess(true);
                ViveSR_DualCameraDepthCollider.SetColliderProcessEnable(true);

                isMaterialSet = true;
            }

            if(ViveSR_Experience.targetHand != null)
            {    
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    LeftText.enabled = true;
                    RightText.enabled = true;
                    ThrowableText.enabled = false;
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    LeftText.enabled = false;
                    RightText.enabled = false;
                    ThrowableText.enabled = true;
                }
            }
        }
    }
}