namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubMenu_Calibration : ViveSR_Experience_ISubMenu
    {
        bool wasDepthOn;

        ViveSR_Experience_Calibration calibrationScript;

        protected override void AwakeToDo()
        {
            calibrationScript = ViveSR_Experience_Button_Calibration.instance.CalibrationScript;
        }

        protected override void Execute()
        {
            if(currentSubBtnNum != (int)Calibration_SubBtn.Reset) calibrationScript.enabled = true;
            if (!calibrationScript.isCalibrating) base.Execute();
        }

        public void StartCalibration()
        {
            wasDepthOn = ViveSR_Experience_Button_EnableDepth.instance.isOn;
            if(wasDepthOn) ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(false);

            //Hide the sub menu
            RenderSubBtns(false);
            ViveSR_Experience.RenderButtons(false);

            //Activate the choosen calibration mode
            calibrationScript.isCalibrating = true;
            ViveSR_DualCameraRig.Instance.DualCameraCalibration.SetCalibrationMode(true, (CalibrationType)currentSubBtnNum);

            //Show digital controller
            if (!ViveSR_Experience.ShowControllerModel() && currentSubBtnNum == (int)Calibration_SubBtn.Alignment) ViveSR_Experience.SetControllerRenderer(true);
        }

        public void ReturnToSubMenu()
        {           
            isSubMenuOn = true;           
            if (!ViveSR_Experience.ShowControllerModel() && currentSubBtnNum == (int)Calibration_SubBtn.Alignment) ViveSR_Experience.SetControllerRenderer(false);

            ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onController, 0f);

            calibrationScript.isCalibrating = false;

            if (wasDepthOn) ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(true);

            RenderSubBtns(true);
            ViveSR_Experience.RenderButtons(true);

            ViveSR_DualCameraRig.Instance.DualCameraCalibration.SetCalibrationMode(false);
            calibrationScript.enabled = false;
        }
    }
}
