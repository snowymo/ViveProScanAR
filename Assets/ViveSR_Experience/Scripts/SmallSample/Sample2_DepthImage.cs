using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample2_DepthImage : MonoBehaviour
    {
        [SerializeField] Material depthImageMaterial;
        bool isMaterialSet;

        private void Update()
        {
            if (ViveSR_Experience.targetHand != null && !isMaterialSet)
            {
                //Assign depthImageMaterial to ViveSR.
                if (ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Count > 0)
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth[0] = depthImageMaterial;
                else
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Add(depthImageMaterial);

                ViveSR_DualCameraImageCapature.EnableDepthProcess(true);
                ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
                isMaterialSet = true;
            }             
        }
    }
}