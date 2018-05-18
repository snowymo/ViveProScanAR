using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_EnableMesh_Dynamic : ViveSR_Experience_ISubBtn
    {
        private static ViveSR_Experience_SubBtn_EnableMesh_Dynamic _instance;
        public static ViveSR_Experience_SubBtn_EnableMesh_Dynamic instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_Dynamic>();
                }
                return _instance;
            }
        }
        [SerializeField] EnableMesh_SubBtn SubBtnType;
        [SerializeField] Material CollisionMaterial;
        [SerializeField] bool ShowDynamicCollision;
        [SerializeField] ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr_dynamic;

        void SetDynamicMeshDisplay(bool isOn)
        {
            ShowDynamicCollision = isOn;
            CollisionMaterial.color = isOn ? new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f)  : Color.clear;
        }

        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
            SetDynamicMeshDisplay(ShowDynamicCollision);
        }

        public override void ExecuteToDo()
        {
            ViveSR_DualCameraDepthCollider.ChangeColliderMaterial(CollisionMaterial);//temp

            ViveSR_DualCameraDepthCollider.SetColliderProcessEnable(isOn);
                                 
            if (isOn)
            {
                ViveSR_Experience_SubBtn_EnableMesh_Static.instance.ForceExcute(false);
                ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.ForceExcute(false);
            }

            ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(isOn);

            if (!isOn) dartGeneratorMgr_dynamic.DestroyObjs();
            dartGeneratorMgr_dynamic.gameObject.SetActive(isOn);
        }
        protected override void UpdateToDo()
        {
            if (isOn && ViveSR_Experience.targetHand != null)
            {
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    if (Time.timeSinceLevelLoad - dartGeneratorMgr_dynamic.tempTime > dartGeneratorMgr_dynamic.coolDownTime)
                    {
                        ViveSR_Experience_Button_EnableMesh.instance.SubMenu.RenderSubBtns(false);
                        ViveSR_Experience.RenderButtons(false);
                        disabled = true;
                    }
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    disabled = false;

                    if (dartGeneratorMgr_dynamic.dartGenerators[(int)dartGeneratorMgr_dynamic.dartPlacementMode].isHolding)
                    {
                        ViveSR_Experience_Button_EnableMesh.instance.SubMenu.RenderSubBtns(true);
                        ViveSR_Experience.RenderButtons(true);
                    }
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
                {
                    SetDynamicMeshDisplay(!ShowDynamicCollision);
                }
            }
        }               
    }
}