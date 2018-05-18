using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Scan_ControllerDetection : MonoBehaviour
    {
        ViveSR_Experience_SubBtn_3DPreview_Scan scanScript;

        private void Awake()
        {
            scanScript = ViveSR_Experience_SubBtn_3DPreview_Scan.instance;
        }

        void OnBecameInvisible()
        {   
            if (ViveSR_Experience_SubBtn_3DPreview_Scan.instance.isOn && !scanScript.isScanning && ViveSR_Experience_HintMessage.instance != null)
            {
                ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onHeadSet, 0f);
                scanScript.SetScanning(true);
                ViveSR_Experience_SubBtn_3DPreview_Save.instance.EnableButton(true);
            }
        }

        void OnBecameVisible()
        {
            if (!scanScript.isScanning)
            {
                ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onHeadSet, "Put the controller out of sight to start scanning.", false);
            }
        }

    }
}