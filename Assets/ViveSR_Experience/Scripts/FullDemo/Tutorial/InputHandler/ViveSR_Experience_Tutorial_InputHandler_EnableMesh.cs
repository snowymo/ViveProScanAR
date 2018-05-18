using UnityEngine;
using System.Linq;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_InputHandler_EnableMesh : ViveSR_Experience_Tutorial_IInputHandler
    {
        [SerializeField] ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr_Static;
        [SerializeField] ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr_Dynamic;

        public override void Touched(Vector2 touchpad)
        {
            tutorial.currentSprite = tutorial.GetCurrentSprite(touchpad);

            if (isTriggerPressed)
            {
                if (tutorial.currentSprite == TouchpadSprite.right || tutorial.currentSprite == TouchpadSprite.left || tutorial.currentSprite == TouchpadSprite.up || tutorial.currentSprite == TouchpadSprite.down)
                {
                    tutorial.SetTouchpadText(tutorial.touchpadTexts[(int)tutorial.currentSprite].buttonTexts.First(x => x.messageType == "staticMesh").text);
                    SetTriggerMessage(true);
                    tutorial.RunSpriteAnimation();
                }
            }
            else
            {
                base.Touched(touchpad);
            }
        }

        public override void TriggerDown()
        {
            base.TriggerDown();
            if ((ViveSR_Experience_StaticMesh.instance.IsMeshReady() && ViveSR_Experience_StaticMesh.instance.enabled)
            || ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.isOn)
                holdObj(true);
        }

        public override void TriggerUp()
        {
            base.TriggerUp();
            if ((ViveSR_Experience_StaticMesh.instance.IsMeshReady() && ViveSR_Experience_StaticMesh.instance.enabled)
            || ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.isOn)
                holdObj(false); 
        }

        void holdObj(bool isTriggerDown)
        {
            tutorial.currentSprite = TouchpadSprite.none;
            tutorial.SetMid(!isTriggerDown);
            SetTriggerMessage(isTriggerDown);
        }

        void SetTriggerMessage(bool isTriggerDown)
        {
            string targetLine = "";
            //    sphere = 0,
            //    deer = 1,  
            //    dart = 2,

            if (isTriggerDown)
            {
                if (dartGeneratorMgr_Static.isActiveAndEnabled || dartGeneratorMgr_Dynamic.isActiveAndEnabled)
                {
                    ViveSR_Experience_DartGeneratorMgr currentMgr = dartGeneratorMgr_Static.isActiveAndEnabled ? dartGeneratorMgr_Static : dartGeneratorMgr_Dynamic;

                    ViveSR_Experience_IDartGenerator DartGenerator = currentMgr.dartGenerators[(int)currentMgr.dartPlacementMode];
                    if (currentMgr.dartPlacementMode != DartPlacementMode.Portal)
                    {
                        if (DartGenerator.currentDartPrefeb == 2) targetLine = "Trigger(Dart)";
                        else if (DartGenerator.currentDartPrefeb == 0) targetLine = "Trigger(Sphere)";
                        else if (DartGenerator.currentDartPrefeb == 1) targetLine = "Trigger(ViveDeer)";
                    }
                    else { targetLine = "Trigger(Portal)"; }

                    tutorial.SetRotatorText(tutorial.MainLineManagers[ViveSR_Experience.rotator.currentButtonNum].mainLines.First(x => x.messageType == targetLine).text);
                }
            }
            else
            {
                SetSubBtnMessage();
            }
                                      
             tutorial.triggerArrow.SetActive(!isTriggerDown);
        }

        protected override void MidPressedDown()
        {
            base.MidPressedDown();

            if (SubMenu.currentSubBtnNum == (int)EnableMesh_SubBtn.Static)
            {
                if (!ViveSR_Experience_StaticMesh.instance.IsMeshReady()) //mesh hasn't been loaded.
                {
                    if (ViveSR_Experience_SubBtn_EnableMesh_Static.instance.isOn)
                    {
                        //hide tutorial when loading mesh
                        tutorial.SetTouchpadCanvas(false);
                        tutorial.ToggleTutorial(false);
                    }
                }
                else
                {
                    if (!ViveSR_Experience_StaticMesh.instance.enabled)
                    {
                        if(!ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.isOn &&
                        !ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn)
                            tutorial.triggerArrow.SetActive(false);
                    }
                    else
                    {
                        tutorial.triggerArrow.SetActive(true);
                        if (!ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn)
                            tutorial.gripArrow.SetActive(true);
                    }                       
                }
            }
            else if (SubMenu.currentSubBtnNum == (int)EnableMesh_SubBtn.Dynamic)
            {
                if (!ViveSR_Experience_SubBtn_EnableMesh_Dynamic.instance.isOn)
                {
                    if (!ViveSR_Experience_StaticMesh.instance.enabled &&
                       !ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn)
                        tutorial.triggerArrow.SetActive(false);
                }
                else
                {
                    tutorial.triggerArrow.SetActive(true);
                    if (!ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn)
                        tutorial.gripArrow.SetActive(true);
                }
            }
            else if (SubMenu.currentSubBtnNum == (int)EnableMesh_SubBtn.VRMode)
            {
                if (ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn)
                {
                    tutorial.gripArrow.SetActive(false);
                    ViveSR_Experience_StaticMesh.instance.showCldMode = 4;
                    ViveSR_Experience_StaticMesh.instance.SwitchShowCollider();
                }
                else
                {
                    if(ViveSR_Experience_StaticMesh.instance.enabled)
                        tutorial.gripArrow.SetActive(true);
                }
            }
        }
        public void MeshReady()
        {
            //tutorial.SetLeftRight(false);
            tutorial.triggerArrow.SetActive(true);
            tutorial.gripArrow.SetActive(true);
            tutorial.ToggleTutorial(true);
            SetSubBtnMessage();
        }
    }
}