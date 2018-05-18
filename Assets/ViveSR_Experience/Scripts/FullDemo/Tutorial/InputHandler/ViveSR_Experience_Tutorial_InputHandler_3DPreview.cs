namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_InputHandler_3DPreview : ViveSR_Experience_Tutorial_IInputHandler
    {
        protected override void MidPressedDown()
        {      
            if (SubMenu.currentSubBtnNum == (int)_3DPreview_SubBtn.Scan)
            {
                base.MidPressedDown();
                tutorial.SetLeftRight(!ViveSR_Experience_SubBtn_3DPreview_Scan.instance.isOn);

            }
            else if (SubMenu.currentSubBtnNum == (int)_3DPreview_SubBtn.Save)
            {
                if (!ViveSR_Experience_SubBtn_3DPreview_Save.instance.disabled)
                {
                    tutorial.SetRotatorCanvas(false);
                    tutorial.ToggleTutorial(false);
                    tutorial.SetTouchpadCanvas(false);
                }
            }  
        }

        public void MeshSaved()
        {
            tutorial.SetLeftRight(true);
            tutorial.ToggleTutorial(true);
            if (ViveSR_Experience_Button_3DPreview.instance.SubMenu.currentSubBtnNum == (int)_3DPreview_SubBtn.Save) SetSubBtnMessage("Disabled");
        }
    }
}