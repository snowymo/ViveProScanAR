using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_Tutorial : ViveSR_Experience_IButton
    {
        private static ViveSR_Experience_Button_Tutorial _instance;
        public static ViveSR_Experience_Button_Tutorial instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Button_Tutorial>();
                }
                return _instance;
            }
        }

        [SerializeField] bool turnOnWhenStart;
        public ViveSR_Experience_Tutorial tutorialScript;

        protected override void StartToDo()
        {
            //start with tutorial on
            if (turnOnWhenStart) ForceExcuteButton(true);
        }

        public override void ActionToDo()
        {
            tutorialScript.enabled = isOn;
            tutorialScript.ToggleTutorial(isOn);
        }
    }
}