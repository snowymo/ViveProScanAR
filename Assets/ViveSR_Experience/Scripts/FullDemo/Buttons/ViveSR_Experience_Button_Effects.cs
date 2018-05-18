namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_Effects : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_Effects _instance;
        public static ViveSR_Experience_Button_Effects instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_Effects>();
                }
                return _instance;
            }
        }

        public ViveSR_Experience_Effects EffectsScript;

        bool wasDepthOn;

        public override void ActionToDo()
        {
            //Disable [Enable Depth] when [Shader Effects] is on
            if (isOn) wasDepthOn = ViveSR_Experience_Button_EnableDepth.instance.isOn;
            if (wasDepthOn) ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(!isOn);

            EffectsScript.enabled = isOn;
            EffectsScript.ToggleEffects(isOn);
        }
        protected override void UpdateToDo()
        {    
            if (isOn && ViveSR_Experience.targetHand != null)
            {     
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    //Hide all buttons and disable the Effects button while holding an effectball.
                    disabled = true;
                    ViveSR_Experience.RenderButtons(false);
                    EffectsScript.GenerateDart();
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    //Show all buttons and enables the Effects button after releasing an effectball.
                    disabled = false;
                    ViveSR_Experience.RenderButtons(true);
                    EffectsScript.ReleaseDart();
                }
            }
        }
    }
}