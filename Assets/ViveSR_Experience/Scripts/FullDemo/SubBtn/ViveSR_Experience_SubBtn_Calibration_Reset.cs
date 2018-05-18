using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_Calibration_Reset : ViveSR_Experience_ISubBtn
    {
        [SerializeField] Calibration_SubBtn SubBtnType;
        
        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
        }

        public override void Execute()
        {
            ViveSR_DualCameraRig.Instance.DualCameraCalibration.ResetCalibration();
            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "[Calibration]\nReset Succeeded!", true);
        }
    }
}