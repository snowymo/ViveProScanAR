using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_Calibration : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_Calibration _instance;
        public static ViveSR_Experience_Button_Calibration instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_Calibration>();
                }
                return _instance;
            }
        }

        public ViveSR_Experience_Calibration CalibrationScript;
    }
}