using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample5_Segmentation : MonoBehaviour
    {
      
        bool isMaterialSet;

        private void Update()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING && !isMaterialSet)
            {          

                isMaterialSet = true;
            }

            if(ViveSR_Experience.targetHand != null)
            {    
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {

                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {

                }
            }
        }
    }
}