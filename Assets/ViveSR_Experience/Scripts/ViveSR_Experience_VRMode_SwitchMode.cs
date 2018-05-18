using UnityEngine;
namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_VRMode_SwitchMode : MonoBehaviour
    {
        [SerializeField] GameObject VRMode_bg;
        public DualCameraDisplayMode currentMode = DualCameraDisplayMode.MIX;
        [SerializeField] Material SkyboxMaterial;
        [SerializeField] bool setSkybox;

        public void SwithMode(DualCameraDisplayMode mode)
        {
            if (mode != currentMode)
            {
                currentMode = mode;
                ViveSR_DualCameraRig.Instance.SetMode(mode);

                if(VRMode_bg != null)
                    VRMode_bg.SetActive(mode == DualCameraDisplayMode.VIRTUAL);

                if (setSkybox)
                    SetSkybox(mode == DualCameraDisplayMode.VIRTUAL);
            }
        }

        private void OnDestroy()
        {
            if (setSkybox) SetSkybox(false);
        }

        public void SetSkybox(bool on)
        {
            SkyboxMaterial.SetFloat("_Exposure", on ? 1 : 0);
        }
    }
}