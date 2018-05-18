using UnityEngine;using UnityEngine.UI;using System.Linq;namespace Vive.Plugin.SR.Experience{    public class ViveSR_Experience_Tutorial_InputHandler_Calibration : ViveSR_Experience_Tutorial_IInputHandler    {
        ViveSR_Experience_Calibration calibrationScript;

        protected override void AwakeToDo()
        {
            calibrationScript = ViveSR_Experience_Button_Calibration.instance.CalibrationScript;
        }
        public override void Touched(Vector2 touchpad)        {
            if (calibrationScript.isSpinning_tutorial) //Only when spinning
            {
                tutorial.currentSprite = TouchpadSprite.none;
                if (Vector2.Distance(touchpad, Vector2.zero) > 0.5)
                    Spin();
            }
            else
            {
                //Set current sprite to animate.
                tutorial.currentSprite = tutorial.GetCurrentSprite(touchpad);
                tutorial.RunSpriteAnimation();
            }

            if (tutorial.currentSprite != TouchpadSprite.none)
            {
                if (!calibrationScript.isCalibrating)
                {
                    tutorial.SetTouchpadText(tutorial.touchpadTexts[(int)tutorial.currentSprite].GetDefaultText());
                }
                else if(calibrationScript.isCalibrating && !calibrationScript.isSpinning_tutorial) //Change texts according to the modes, only don't change touchpad text when spinning
                {                             
                    string targetText = SubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Focus ? "calibration_relative" : "calibration_absolute";
                    tutorial.SetTouchpadText(tutorial.touchpadTexts[(int)tutorial.currentSprite].buttonTexts.First(x => x.messageType == targetText).text);
                }
            }        }

        public override void TouchedUp()
        {
            base.TouchedUp();

            if (calibrationScript.isCalibrating && !calibrationScript.isSpinning_tutorial)
                ResetSpinnerImages();
        }        public override void PressedDown()
        {
            if (tutorial.currentSprite == TouchpadSprite.left || tutorial.currentSprite == TouchpadSprite.right)
            {
                if (!calibrationScript.isCalibrating)
                {
                    base.PressedDown();
                }
                
                if (calibrationScript.isCalibrating && !calibrationScript.isSpinning_tutorial)
                {
                    ResetSpinnerImages();
                }
            }
            else if (tutorial.currentSprite == TouchpadSprite.up || tutorial.currentSprite == TouchpadSprite.down)
            {
                if (!calibrationScript.isCalibrating)
                {
                    tutorial.SetRotatorCanvas(true);

                    //Set rotator message for calibration
                    Calibration_SubBtn currentSubBtn = (Calibration_SubBtn)SubMenu.currentSubBtnNum;
                    tutorial.SetRotatorText(tutorial.MainLineManagers[ViveSR_Experience.rotator.currentButtonNum].subLineManager.SubBtns[SubMenu.currentSubBtnNum].lines.First(x => x.messageType == "Available").text);
                    if (currentSubBtn == Calibration_SubBtn.Alignment) tutorial.SetRotatorCanvas(true);
                }
            }
            else if (tutorial.currentSprite == TouchpadSprite.mid)
            {                                       
                MidPressedDown();
            }
        }

        protected override void MidPressedDown()
        {
            if (!ViveSR_DualCameraCalibrationTool.IsCalibrating) //calibration menu
            {
                ResetSpinnerImages();
                foreach (Image img in tutorial.spinngerImage) img.enabled = false;
                
                //Allow up and down touchpad btn for calibration menu
                if (SubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Reset)
                    tutorial.SetRotatorCanvas(false);
                else
                {
                    base.MidPressedDown();
                    tutorial.SetRotatorCanvas(true);
                }
            }
            else //Calibrating
            {
                tutorial.SetRotatorCanvas(false);
            }
        }

        void ResetSpinnerImages()
        {
            tutorial.SetTouchpadImage(true);
            foreach (Image img in tutorial.spinngerImage) img.enabled = false;
            tutorial.targetSpinnerImageNumber_Prev = -1;
        }

        void Spin()
        {
            tutorial.SetTouchpadImage(false);
            if (calibrationScript.rotatingAngle > 0) tutorial.targetSpinnerImageNumber = (SubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Focus)? 1 : 0;
            else if (calibrationScript.rotatingAngle < 0) tutorial.targetSpinnerImageNumber = (SubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Focus) ? 0:1;

            if (calibrationScript.isSpinning)
                if (tutorial.targetSpinnerImageNumber == 1)
                    tutorial.spinngerImage[1].gameObject.transform.localEulerAngles += new Vector3(0f, 0f, 1f);
                else if (tutorial.targetSpinnerImageNumber == 0)
                    tutorial.spinngerImage[0].gameObject.transform.localEulerAngles += new Vector3(0f, 0f, -1f);

            if (tutorial.targetSpinnerImageNumber_Prev != tutorial.targetSpinnerImageNumber)
            {
                bool isClockWise = tutorial.targetSpinnerImageNumber == 0;
                tutorial.SetTouchpadText(isClockWise ? "[Spin] Clockwise" : "[Spin] Rotate Counter Clockwise");
                tutorial.spinngerImage[0].enabled = isClockWise;
                tutorial.spinngerImage[1].enabled = !isClockWise;
            }
            tutorial.targetSpinnerImageNumber_Prev = tutorial.targetSpinnerImageNumber;
        }
    }}