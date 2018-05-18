using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_3DPreview_Scan : ViveSR_Experience_ISubBtn
    {
        private static ViveSR_Experience_SubBtn_3DPreview_Scan _instance;
        public static ViveSR_Experience_SubBtn_3DPreview_Scan instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_SubBtn_3DPreview_Scan>();
                }
                return _instance;
            }
        }
        public bool isScanning;
        [SerializeField] _3DPreview_SubBtn SubBtnType; 

        [SerializeField] GameObject ControllerVisibilityDetector;

        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
        }

        public override void ExecuteToDo()
        {  
            ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;

#if UNITY_EDITOR 
            SetScanning(isOn);
            if(isOn) ViveSR_Experience_SubBtn_3DPreview_Save.instance.EnableButton(true);
#else
             ControllerVisibilityDetector.SetActive(isOn);
            if (!isOn) ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onHeadSet, 0f);
             if (isScanning)
            {
                SetScanning(false);
                ViveSR_Experience_SubBtn_3DPreview_Save.instance.EnableButton(false);
            }
#endif
        }

        public void SetScanning(bool On)
        {
            if (On)
            {
                ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(true);
                ViveSR_RigidReconstruction.StartScanning();
            }
            else
            {
                ViveSR_RigidReconstruction.StopScanning();
                ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(false);
            }
            isScanning = (ViveSR_RigidReconstruction.IsScanning);
        }   
    }
}