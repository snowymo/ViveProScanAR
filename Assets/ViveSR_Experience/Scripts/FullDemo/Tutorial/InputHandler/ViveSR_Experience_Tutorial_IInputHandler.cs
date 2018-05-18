using UnityEngine;
using System.Linq;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_IInputHandler : MonoBehaviour
    {
        [SerializeField] Button ButtonType;
        protected int ThisButtonTypeNum;
        [SerializeField] protected ViveSR_Experience_ISubMenu SubMenu;
        protected ViveSR_Experience_Tutorial tutorial;
        [SerializeField] protected bool isTriggerPressed;

        private void Awake()
        {
            ThisButtonTypeNum = (int)ButtonType;
            tutorial = ViveSR_Experience_Button_Tutorial.instance.tutorialScript;
            AwakeToDo();
        }
        protected virtual void AwakeToDo(){ }
        public virtual void Touched(Vector2 touchpad)
        {
            if (tutorial.currentSprite != TouchpadSprite.none)
                tutorial.SetTouchpadText(tutorial.touchpadTexts[(int)tutorial.currentSprite].GetDefaultText());
            tutorial.currentSprite = tutorial.GetCurrentSprite(touchpad);
            tutorial.RunSpriteAnimation();
        }
        
        public virtual void TouchedUp()
        {
            ResetSprite();
        }

        public virtual void ResetSprite()
        {
            tutorial.SetTouchpadCanvas(false);
            tutorial.currentSprite = TouchpadSprite.none;
            tutorial.RunSpriteAnimation();  //Clean up after sprite is none.
        }

        public virtual void PressedDown()
        {
            if (tutorial.currentSprite == TouchpadSprite.left || tutorial.currentSprite == TouchpadSprite.right)
            {
                LeftRightPressedDown();
            }
            else if (SubMenu != null && (tutorial.currentSprite == TouchpadSprite.up || tutorial.currentSprite == TouchpadSprite.down))
            {
                if (!isTriggerPressed)
                {
                    SetSubBtnMessage();
                }
            }
            else if (tutorial.currentSprite == TouchpadSprite.mid)
            {
                MidPressedDown();
            }
        }
        
        protected void SetSubBtnMessage()
        {
            string subMsgType;

            if (SubMenu.subBtnScripts[SubMenu.currentSubBtnNum].disabled)
                subMsgType = "Disabled";
            else if (SubMenu.subBtnScripts[SubMenu.currentSubBtnNum].isOn)
                subMsgType = "On";
            else subMsgType = "Available";
                                         
            try
            {
                tutorial.SetRotatorText(tutorial.MainLineManagers[ThisButtonTypeNum].subLineManager.SubBtns[SubMenu.currentSubBtnNum].lines.First(x => x.messageType == subMsgType).text);
            }
            catch
            {
                Debug.LogWarning("[Tutorial] The line for Subbtn array"+ SubMenu.currentSubBtnNum+ " "+ subMsgType + " is not found. Check " + (Button)ViveSR_Experience.rotator.currentButtonNum +"'s SubLineManager.");
            }
        }

        protected void SetSubBtnMessage(string subMsgType)
        {
            try
            {
                tutorial.SetRotatorText(tutorial.MainLineManagers[ThisButtonTypeNum].subLineManager.SubBtns[SubMenu.currentSubBtnNum].lines.First(x => x.messageType == subMsgType).text);
            }
            catch
            {
                Debug.LogWarning("[Tutorial] The line for Subbtn array" + SubMenu.currentSubBtnNum + " " + subMsgType + " is not found. Check " + (Button)ViveSR_Experience.rotator.currentButtonNum + "'s SubLineManager.");
            }
        }

        protected virtual void LeftRightPressedDown()
        {
            StartCoroutine(tutorial.MoveTowards(ViveSR_Experience.ButtonScripts[ThisButtonTypeNum].disabled));

            if (SubMenu == null || !ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].SubMenu.isSubMenuOn)
            {
                tutorial.SetMainMessage();

                //Set mid btn to grey if the button is disabled
                tutorial.touchpadImages[(int)TouchpadSprite.mid].color = ViveSR_Experience.ButtonScripts[ThisButtonTypeNum].disabled ?
                     Color.grey : Color.white;

                //recover disabled touchpad buttons
                if (tutorial.isUpDownOn) tutorial.SetUpDown(false);
                if (!tutorial.isLeftRightOn) tutorial.SetLeftRight(true);

                tutorial.SetRotatorCanvas(true);

                tutorial.triggerArrow.SetActive(false);
                tutorial.gripArrow.SetActive(false);
            }
        }
        protected virtual void MidPressedDown()
        {                         
            if (SubMenu == null) tutorial.SetMainMessage();
            else
            {              
                tutorial.SetUpDown(true);
                SetSubBtnMessage();
            }
        }

        public virtual void TriggerDown()
        {
            isTriggerPressed = true;
        }
        public virtual void TriggerUp()
        {
            isTriggerPressed = false;
        }
    }
}