using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_EnableMesh_VRMode : ViveSR_Experience_ISubBtn
    {
        private static ViveSR_Experience_SubBtn_EnableMesh_VRMode _instance;
        public static ViveSR_Experience_SubBtn_EnableMesh_VRMode instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_VRMode>();
                }
                return _instance;
            }
        }
        [SerializeField] EnableMesh_SubBtn SubBtnType;

        [SerializeField] ViveSR_Experience_VRMode_SwitchMode SwitchModeScript;

        [SerializeField] ViveSR_Experience_IDartGenerator PortalDartGenerator;

        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
        }

        public override void ExecuteToDo()
        {
            DualCameraDisplayMode targetMode = isOn ? DualCameraDisplayMode.VIRTUAL : DualCameraDisplayMode.MIX;
            ViveSR_Experience_StaticMesh.instance.RenderMesh(targetMode == DualCameraDisplayMode.VIRTUAL);
            SwitchModeScript.SwithMode(targetMode);
            if(isOn) ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.ForceExcute(false);
           // ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr_static = ViveSR_Experience_SubBtn_EnableMesh_Static.instance.dartGeneratorMgr_static;

          //  dartGeneratorMgr_static.gameObject.SetActive(true);
            //if (isOn)
            //{   
            //    //remove portal generator because portals only work in Mixed Mode;
            //    if (dartGeneratorMgr_static.dartPlacementMode == DartPlacementMode.Portal)
            //    {
            //        dartGeneratorMgr_static.SwitchPlacementMode();
            //    }                      
            //    PortalDartGenerator = dartGeneratorMgr_static.dartGenerators[2];
            //    dartGeneratorMgr_static.dartGenerators.Remove(PortalDartGenerator);
            //}
            //else
            //{
            //    ViveSR_Experience_SubBtn_EnableMesh_Static.instance.dartGeneratorMgr_static.dartGenerators.Add(PortalDartGenerator);
            //}
        }        
    }
}