using UnityEngine;
using System.IO;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_EnableMesh_Static : ViveSR_Experience_ISubBtn
    {
        private static ViveSR_Experience_SubBtn_EnableMesh_Static _instance;
        public static ViveSR_Experience_SubBtn_EnableMesh_Static instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_Static>();
                }
                return _instance;
            }
        }

        [SerializeField] EnableMesh_SubBtn SubBtnType;
        
        public string mesh_path = "Recons3DAsset/Model.obj";
        public string cld_path = "Recons3DAsset/Model_cld.obj";
        public ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr_static;
        protected override void StartToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
            EnableButton(File.Exists(mesh_path) && File.Exists(cld_path));
        }

        public override void ExecuteToDo()
        {
            if (isOn)
            {
                ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.ForceExcute(false);

                ViveSR_Experience_StaticMesh.instance.showCldMode = 4;
                ViveSR_Experience_StaticMesh.instance.SwitchShowCollider();
            }
            else ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(false);

            ViveSR_Experience_StaticMesh.instance.enabled = isOn;
            ViveSR_Experience_StaticMesh.instance.LoadMesh(isOn);

            if(!isOn) dartGeneratorMgr_static.DestroyObjs();
            dartGeneratorMgr_static.gameObject.SetActive(isOn); 
        }
        protected override void UpdateToDo()
        {
            if(isOn && ViveSR_Experience.targetHand != null)
            { 
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    disabled = true;

                    if (Time.timeSinceLevelLoad - dartGeneratorMgr_static.tempTime > dartGeneratorMgr_static.coolDownTime)
                    {
                        ViveSR_Experience_Button_EnableMesh.instance.SubMenu.RenderSubBtns(false);
                        ViveSR_Experience.RenderButtons(false);
                        disabled = true;
                    }
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    disabled = false;

                    if (dartGeneratorMgr_static.dartGenerators[(int)dartGeneratorMgr_static.dartPlacementMode].isHolding)
                    {
                        ViveSR_Experience_Button_EnableMesh.instance.SubMenu.RenderSubBtns(true);
                        ViveSR_Experience.RenderButtons(true);
                    }
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
                {
                    if (!ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn) ViveSR_Experience_StaticMesh.instance.SwitchShowCollider();
                }
            }
        }




       
    }
}