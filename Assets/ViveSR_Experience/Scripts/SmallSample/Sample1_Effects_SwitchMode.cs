using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample1_Effects_SwitchMode : MonoBehaviour
    {
        [SerializeField] ViveSR_Experience_Effects EffectsScript;
        [SerializeField] ViveSR_Experience_VRMode_SwitchMode SwitchModeScript;

        [SerializeField] GameObject canvas;
        [SerializeField] Text EffectText;

        void Update()
        {
            if (ViveSR_Experience.targetHand != null)
            {
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (SwitchModeScript.currentMode == DualCameraDisplayMode.MIX)
                {
                    if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        EffectsScript.GenerateDart();
                        canvas.SetActive(false);
                    }
                    else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        EffectsScript.ReleaseDart();
                        canvas.SetActive(true);
                    }
                }

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && !controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
                {
                    SwitchModeScript.SwithMode(SwitchModeScript.currentMode == DualCameraDisplayMode.MIX ? DualCameraDisplayMode.VIRTUAL : DualCameraDisplayMode.MIX);
                    if (SwitchModeScript.currentMode == DualCameraDisplayMode.MIX) EffectsScript.ChangeShader(0);

                    EffectText.text = SwitchModeScript.currentMode == DualCameraDisplayMode.MIX? "Effect Candy->" : "";
                }
            }
        }
    }
}