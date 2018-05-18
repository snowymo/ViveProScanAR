using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Calibration : MonoBehaviour
    {
        public bool isCalibrating;

        //AxisZ
        float startingAngle;
        float currentAngle;
        float temptime;

        //spinning
        public bool isSpinning;
        public bool isSpinning_tutorial;
        public float rotatingAngle = 0;

        //AxisXY
        bool isMoving;

        //Forwards & Backwards
       // bool isShifterFound;
      //  ViveSR_HMDCameraShifter shifter;

        [SerializeField] ViveSR_Experience_SubMenu_Calibration CalibrationSubMenu;

        private void Update()
        {
            //if (!isShifterFound && ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING)
            //{
            //    shifter = GameObject.Find("Camera (eye offset)").GetComponent<ViveSR_HMDCameraShifter>();

            //    isShifterFound = true;
            //}
            HandleTouchpadInput_Calibrating(); //Layer 3: calibrating
        }
        
        void HandleTouchpadInput_Calibrating()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

                //Mid: return to the sub menu.
                if (Vector2.Distance(touchPad, Vector2.zero) < 0.5)
                {
                    isSpinning_tutorial = false;
                    CalibrationSubMenu.ReturnToSubMenu();
                }
                else ResetCalibrationTouch(touchPad);
            }
            else if (controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                ResetCalibrationTouch(touchPad);
            }
            else if (controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                isSpinning = false;
                isSpinning_tutorial = false;
            }

            //AxisZ
            if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad)
            && !controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

                if(Vector2.Distance(touchPad, Vector2.zero) > 0.5) RotateAxisZ(touchPad);
            }
            //AxisXY
            else if (!isSpinning && controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

                RotateAxisXY(touchPad);
            }

            //shifter--- 
            //if (CalibrationSubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Alignment && 
            //    !isMoving && 
            //    controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)) //move the camera image forwards or backwards with short press on Y+ or Y-
            //{
            //    Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

            //    if (touchPad.y >= 0.5) shifter.CameraShiftZ -= 0.02f;
            //    if (touchPad.y <= -0.5) shifter.CameraShiftZ += 0.02f;
            //}
        }

        void ResetCalibrationTouch(Vector2 touchPad)
        {
            //Set startingAngle and convert Vector2 to degree.
            startingAngle = Vector2.Angle(new Vector2(1, 0), touchPad);
            if (touchPad.y > 0) startingAngle = 360 - startingAngle;

            //For detecting long press.
            temptime = Time.timeSinceLevelLoad;

            //Make changing Axis Z and Axis XY mutual excusive.
            isSpinning = false;
            isSpinning_tutorial = false;
            isMoving = false;
        }

        void RotateAxisZ(Vector2 touchPad)
        {
            //Set currentAngle and convert Vector2 to degree.
            currentAngle = Vector2.Angle(new Vector2(1, 0), touchPad);
            if (touchPad.y > 0) currentAngle = 360 - currentAngle;

            if (!isMoving)
            {
                rotatingAngle = 0;
                
                //Only works when moving more than 5 degrees.
                if (Mathf.Abs(currentAngle - startingAngle) < 300f)
                {
                    if (currentAngle > startingAngle + 5) RotateAxisZ_SetAngle(true);
                    else if (currentAngle < startingAngle - 5) RotateAxisZ_SetAngle(false);
                    else isSpinning = false;
                } 
                else
                {
                    if (currentAngle < 10 && currentAngle + 360 > startingAngle + 5) RotateAxisZ_SetAngle(true);
                    else if (currentAngle > 300 && currentAngle < 360 + startingAngle - 5) RotateAxisZ_SetAngle(false);
                    else isSpinning = false;
                }

                if (isSpinning) ViveSR_DualCameraRig.Instance.DualCameraCalibration.Calibration(CalibrationAxis.Z, rotatingAngle);
                
                startingAngle = currentAngle;
            }
        }

        void RotateAxisZ_SetAngle(bool isClockwise)
        {
            int shouldInvert = CalibrationSubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Focus ? 1 : -1;
            rotatingAngle = shouldInvert * (isClockwise ? -1 : 1) * Time.deltaTime * 5;
            isSpinning = true;
            isSpinning_tutorial = true;
        }

        void RotateAxisXY(Vector2 touchPad)
        {
            if (Time.time - temptime > 0.5) //Long press
            {
                isMoving = true;

                int shouldInvert = CalibrationSubMenu.currentSubBtnNum == (int)Calibration_SubBtn.Focus ? 1:-1;

                ViveSR_DualCameraCalibrationTool calibrationTool = ViveSR_DualCameraRig.Instance.DualCameraCalibration;

                if (touchPad.x >= 0.5)
                    calibrationTool.Calibration(CalibrationAxis.Y, shouldInvert* Time.deltaTime * 2); //Right
                if (touchPad.x <= -0.5)
                    calibrationTool.Calibration(CalibrationAxis.Y, shouldInvert * - Time.deltaTime * 2); //Left
                if (touchPad.y >= 0.5)
                    calibrationTool.Calibration(CalibrationAxis.X, shouldInvert * -Time.deltaTime * 2); //Up
                if (touchPad.y <= -0.5)
                    calibrationTool.Calibration(CalibrationAxis.X, shouldInvert * Time.deltaTime * 2); //Down
            }
        }
    }
}