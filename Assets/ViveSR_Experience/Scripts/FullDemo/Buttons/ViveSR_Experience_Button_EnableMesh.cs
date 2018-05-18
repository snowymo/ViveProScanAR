using Valve.VR.InteractionSystem;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_EnableMesh : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_EnableMesh _instance;
        public static ViveSR_Experience_Button_EnableMesh instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_EnableMesh>();
                }
                return _instance;
            }
        }

        public override void ActionToDo()
        {
            if(!isOn)
            {
                ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.ForceExcute(false);
                ViveSR_Experience_SubBtn_EnableMesh_Static.instance.ForceExcute(false);
                ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.ForceExcute(false);
                EnableButton(true);
            }
        }
    }
}