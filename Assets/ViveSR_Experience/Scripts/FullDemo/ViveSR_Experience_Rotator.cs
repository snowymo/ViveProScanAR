using UnityEngine;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Rotator : MonoBehaviour
    {
        float distToCenter = -0.025f;
        float rotateSpeed = 200f;
        float buttonEnlargingSpeed = 1.5f;

        bool isRotating;
        bool isTouchPositive;
        float localY, targetY;
        float rotateAngle;
        
        public int currentButtonNum;

        bool isUISet = false;

        //Prevent overlapping of the Enlarge coroutine.
        IEnumerator prevTrueCoroutine, prevFalseCoroutine;

        float cooldowntime = 0.1f;
        float tempTime = 2f;

        bool isRotatingAllow()
        {                                                                                                                                    
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            return !controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)
                   && !controller.GetTouch(SteamVR_Controller.ButtonMask.Trigger)
                   && !ViveSR_DualCameraCalibrationTool.IsCalibrating
                   && ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING
                   && !ViveSR_Experience_SubBtn_3DPreview_Scan.instance.isScanning
                   && !ViveSR_Experience_SubBtn_3DPreview_Scan.instance.isOn
                   && !ViveSR_Experience_StaticMesh.instance.IsLoading()
                   && !isRotating
                   && controller.hasTracking;
        }

        private void Update()
        {
            if (ViveSR_Experience.targetHand != null)
            { 
                if(!isUISet)
                {
                    SetUITransform();

                    isUISet = true;
                }
                else
                {
                    if(isRotatingAllow()) HandleTouchPad();
                }
            }
        }

        void SetUITransform()
        {
            //Attachpoint controlls the positioning of the UI.
            ViveSR_Experience.AttachPoint.transform.parent = ViveSR_Experience.targetHand.transform.Find("Attach_ControllerTip").transform;
            ViveSR_Experience.AttachPoint.transform.localPosition = new Vector3(0f, 0.015f, 0.02f);
            ViveSR_Experience.AttachPoint.transform.localEulerAngles = new Vector3(60f, 0f, 0f);

            //Rotate the Buttons and then expands them to form a circle.
            for (int i = 0; i < ViveSR_Experience.Buttons.Count; i++)
            {
                //Add 90 degrees to match the controller's orientation.
                if (i == 0) rotateAngle = 90;

                //Rotate the Buttons.
                ViveSR_Experience.Buttons[i].transform.localEulerAngles += new Vector3(0, rotateAngle, 0);

                //Extend the Button from the geo center of all Buttons.
                ViveSR_Experience.Buttons[i].transform.GetChild(0).transform.localPosition += new Vector3(distToCenter, 0, 0);

                //Accumulate the degree number for the next button.
                rotateAngle += 360 / ViveSR_Experience.Buttons.Count;
            }

            //Move the UI to Attachpoint.
            transform.localPosition = new Vector3(0, -0.15f, 0);
            if (distToCenter >= 0) transform.localScale = new Vector3(-3, 3, 3);
            else transform.localScale = new Vector3(3, 3, -3);

            //Enlarge the current Button.
            ViveSR_Experience.Buttons[currentButtonNum].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        void HandleTouchPad()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            //This block sets the direction of rotation, how much it should rotate, and triggers the Rotate coroutine.
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                
                //Mid: Excute the choosen Buttn.
                if (Vector2.Distance(touchPad, Vector2.zero) < 0.5)
                {
                    //Toggle on and off. Some Buttons do not allow toggling off.
                    if ((ViveSR_Experience.ButtonScripts[currentButtonNum].isOn && ViveSR_Experience.ButtonScripts[currentButtonNum].allowToggle) || !ViveSR_Experience.ButtonScripts[currentButtonNum].isOn)
                    {
                        if (!ViveSR_Experience.ButtonScripts[currentButtonNum].disabled)
                        {
                            ViveSR_Experience.ButtonScripts[currentButtonNum].isOn = !ViveSR_Experience.ButtonScripts[currentButtonNum].isOn;
                            ViveSR_Experience.ButtonScripts[currentButtonNum].ActOnRotator(ViveSR_Experience.ButtonScripts[currentButtonNum].isOn);
                        }
                    }
                }
                else if (touchPad.x > 0.5f || touchPad.x < -0.5f)
                {
                    if (Time.timeSinceLevelLoad - tempTime > cooldowntime)
                    {
                        //When getting away from the previously hovered Button, ...
                        if (prevFalseCoroutine != null) StopCoroutine(prevFalseCoroutine);
                        prevFalseCoroutine = Enlarge(currentButtonNum, false);
                        StartCoroutine(prevFalseCoroutine);
                        if (ViveSR_Experience.ButtonScripts[currentButtonNum].isOn && ViveSR_Experience.ButtonScripts[currentButtonNum].disableWhenRotatedAway)
                            ViveSR_Experience.ButtonScripts[currentButtonNum].ActOnRotator(false);

                        //Get a new button number.
                        isTouchPositive = touchPad.x > 0 ? true : false;
                        currentButtonNum += isTouchPositive ? 1 : -1;
                        if (currentButtonNum < 0) currentButtonNum = ViveSR_Experience.Buttons.Count - 1;
                        else if (currentButtonNum > ViveSR_Experience.Buttons.Count - 1) currentButtonNum = 0;

                        //Enlarge.
                        if (!ViveSR_Experience.ButtonScripts[currentButtonNum].disabled)
                        {
                            prevTrueCoroutine = Enlarge(currentButtonNum, true);
                            StartCoroutine(prevTrueCoroutine);
                        }

                        //Set the target degree.
                        targetY = localY + (360 / ViveSR_Experience.Buttons.Count) * (isTouchPositive ? 1 : -1);
                        targetY = Mathf.RoundToInt(targetY / (360 / ViveSR_Experience.Buttons.Count)) * (360 / ViveSR_Experience.Buttons.Count);

                        StartCoroutine(Rotate());
                    }
                }
            }
        }

        IEnumerator Enlarge(int currentButtonNum, bool on) 
        {   
            //on ? enlarge : shrink
            while (on ? ViveSR_Experience.Buttons[currentButtonNum].transform.localScale.x < 1.5 : ViveSR_Experience.Buttons[currentButtonNum].transform.localScale.x > 1)
            {
                ViveSR_Experience.Buttons[currentButtonNum].transform.localScale += (on ? 1 : -1) * new Vector3(buttonEnlargingSpeed * Time.deltaTime, buttonEnlargingSpeed * Time.deltaTime, buttonEnlargingSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            ViveSR_Experience.Buttons[currentButtonNum].transform.localScale = on ? new Vector3(1.5f, 1.5f, 1.5f) : new Vector3(1f, 1f, 1f);
        }

        IEnumerator Rotate()
        {
            isRotating = true;
            while (isTouchPositive ? localY < targetY : localY > targetY)
            {
                localY += (isTouchPositive ? 1 : -1) * rotateSpeed * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, localY, transform.localEulerAngles.z);

                yield return new WaitForEndOfFrame();
            }
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, targetY, transform.localEulerAngles.z);
            isRotating = false;
            tempTime = Time.timeSinceLevelLoad;
        }
    }
}