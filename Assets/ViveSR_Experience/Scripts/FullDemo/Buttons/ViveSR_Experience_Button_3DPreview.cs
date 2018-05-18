using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_3DPreview : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_3DPreview _instance;
        public static ViveSR_Experience_Button_3DPreview instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_3DPreview>();
                }
                return _instance;
            }
        }
    }
}