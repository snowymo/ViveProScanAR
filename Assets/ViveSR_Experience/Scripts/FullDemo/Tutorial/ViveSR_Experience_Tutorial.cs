using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vive.Plugin.SR.Experience
{
    enum TextCanvas
    {
        onTouchPad = 0,
        onRotator = 1
    }
    public class ViveSR_Experience_Tutorial : MonoBehaviour
    {        
        bool allowUpdate = true;

        //For storing SteamVR touchpad gameobject so as to position Tutorial canvases
        GameObject touchpadGameObj;
        bool isTouchpadUISet;

        public List<ViveSR_Experience_Tutorial_MainLineManager> MainLineManagers; //Texts
        public List<ViveSR_Experience_Tutorial_IInputHandler> InputHandlers; //For handling inputs for differernt buttons

        [Header("Touchpad Hints")]
        public bool isUpDownOn; //For resetting the color of the touchpad up & down buttons
        public bool isLeftRightOn;
        public bool isMidOn;

        public TouchpadSprite currentSprite = TouchpadSprite.none;
        public TouchpadSprite previousSprite = TouchpadSprite.none;
        public GameObject touchpadImageGroup;
        public List<Image> touchpadImages;
        [SerializeField] List<ViveSR_Experience_Tutorial_TextureSwap> touchpadScripts;
        public List<ViveSR_Experience_Tutorial_TouchpadTexts> touchpadTexts;
        IEnumerator currentTouchPadCoroutine;

        [Header("Spinner")]
        [SerializeField] GameObject touchpadSpinnerImageGroup;
        public List<Image> spinngerImage;
        public int targetSpinnerImageNumber;
        public int targetSpinnerImageNumber_Prev = -1;

        [Header("Canvases")]
        public List<GameObject> tutorialCanvases;
        [SerializeField] List<Text> tutorialTexts;
        public GameObject triggerArrow; //Hint for effect balls
        public GameObject gripArrow; //Hint for effect balls
        public IEnumerator CurrentMoveTowardsCoroutine;
        
        //For locating canvases on rotator depending on if the current rotator button is disabled
        Vector3 OriginalPos;
        Vector3 PosOnDisable;

        private void Start()
        {
            foreach (Text text in tutorialTexts) text.transform.parent.SetParent(ViveSR_Experience.AttachPoint.transform);
            
            triggerArrow.transform.SetParent(ViveSR_Experience.AttachPoint.transform);
            triggerArrow.transform.localEulerAngles = new Vector3(20f, 0f, 4f);
            triggerArrow.transform.localPosition = new Vector3(-0.05f, -0.1f, -0.03f);
            gripArrow.transform.SetParent(ViveSR_Experience.AttachPoint.transform);
            gripArrow.transform.localEulerAngles = new Vector3(20f, 0f, 4f);
            gripArrow.transform.localPosition = new Vector3(-0.058f, -0.13f, -0.06f);
            tutorialCanvases[(int)TextCanvas.onTouchPad].transform.localPosition = new Vector3(0.07f, -0.085f, -0.056f);
            tutorialCanvases[(int)TextCanvas.onRotator].transform.localPosition = new Vector3(0f, -0.035f, -0.075f);
            
            //Variables for the MoveTowards coroutine
            OriginalPos = tutorialTexts[(int)TextCanvas.onRotator].gameObject.transform.parent.transform.localPosition;
            PosOnDisable = new Vector3(0f, -0.046f, -0.049f);
            
            SetMainMessage();
        }

        void LateUpdate() //Follow ViveSR_Experience.rotator.currentButtonNum
        {
            if (allowUpdate && ViveSR_Experience.targetHand != null)
            {
                if (!isTouchpadUISet) SetTouchPadUI();
                else
                {
                    if (ViveSR_Experience_Button_Tutorial.instance.isOn)
                    {
                        HandleTouchpad();
                        HandleTrigger();
                    }
                }
            }
        }

        void SetTouchPadUI()
        {
            if (touchpadGameObj == null)
            {
                try
                {
                     touchpadGameObj = ViveSR_Experience.targetHand.transform.Find("BlankController_Hand" + ((ViveSR_Experience.targetHand.name == "Hand1") ? "1" : "2")).gameObject.transform.GetChild(0).gameObject.transform.Find("trackpad").transform.GetChild(0).gameObject;
                }
                catch (System.NullReferenceException e)
                { Debug.Log(e); }

                if (touchpadGameObj != null)
                {
                    //touchpad
                    touchpadImageGroup.transform.SetParent(touchpadGameObj.transform);
                    touchpadImageGroup.transform.localPosition = new Vector3(0f, 0f, 0.003f);
                    touchpadImageGroup.transform.localEulerAngles = new Vector3(180f, 0f, 90f);
                    touchpadImageGroup.transform.localScale = new Vector3(0.0004f, 0.0004f, 0.0004f);

                    //spinner
                    touchpadSpinnerImageGroup.transform.SetParent(touchpadGameObj.transform);
                    touchpadSpinnerImageGroup.transform.localPosition = new Vector3(0f, 0f, 0.003f);
                    touchpadSpinnerImageGroup.transform.localEulerAngles = new Vector3(180f, 0f, 90f);
                    touchpadSpinnerImageGroup.transform.localScale = new Vector3(0.0004f, 0.0004f, 0.0004f);

                    isTouchpadUISet = true;
                }
            }
        }
        
        void HandleTrigger()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) InputHandlers[ViveSR_Experience.rotator.currentButtonNum].TriggerDown();
            else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) InputHandlers[ViveSR_Experience.rotator.currentButtonNum].TriggerUp();
        }

        public void RunSpriteAnimation()
        {

            if (currentSprite != previousSprite)
            {
                //Previous...
                //Disables animation.
                if (currentTouchPadCoroutine != null)
                {
                    StopCoroutine(currentTouchPadCoroutine);
                    currentTouchPadCoroutine = null;
                }
                if (previousSprite != TouchpadSprite.none)
                {
                    touchpadScripts[(int)previousSprite].isAnimating = false;
                    if(touchpadImages[(int)previousSprite].color != Color.grey) touchpadImages[(int)previousSprite].color = Color.white;
                }

                //Current...
                //Enables animation.
                if (currentSprite == TouchpadSprite.none)
                {
                    SetTouchpadText("");
                    SetTouchpadCanvas(false);
                }
                else
                {                                  
                    SetTouchpadCanvas(true);
                              
                    //Start animating the hovered sprite.
                    touchpadScripts[(int)currentSprite].isAnimating = true;
                    currentTouchPadCoroutine = touchpadScripts[(int)currentSprite].Animate();
                    StartCoroutine(currentTouchPadCoroutine);

                    //Set touched sprite color to highlight.
                    touchpadImages[(int)currentSprite].color = ViveSR_Experience_Button_Tutorial.instance.BrightColor;//the hovered sprite
                }
                previousSprite = currentSprite;
            }
        }

        public TouchpadSprite GetCurrentSprite(Vector2 touchPad)
        {
            if (Vector2.Distance(touchPad, Vector2.zero) <= 0.5)
            {
                if(!ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].disabled)
                    currentSprite = TouchpadSprite.mid;
            }
            else if (touchPad.x > 0.5f) currentSprite = TouchpadSprite.right;
            else if (touchPad.x < -0.5f) currentSprite = TouchpadSprite.left;
            else if (touchPad.y > 0.5f) currentSprite = TouchpadSprite.up;
            else if (touchPad.y < 0.5f) currentSprite = TouchpadSprite.down;

            if (currentSprite != TouchpadSprite.none)
            {   
                if (touchpadScripts[(int)currentSprite].isDisabled)
                    currentSprite = TouchpadSprite.none;
            }
            return currentSprite;
        }

        void HandleTouchpad()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchpad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                InputHandlers[ViveSR_Experience.rotator.currentButtonNum].Touched(touchpad);
            }
            else if (controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
                InputHandlers[ViveSR_Experience.rotator.currentButtonNum].TouchedUp();

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
                InputHandlers[ViveSR_Experience.rotator.currentButtonNum].PressedDown();
        }

        public void SetUpDown(bool isAvailable)
        {
            isUpDownOn = isAvailable;
            touchpadImages[(int)TouchpadSprite.up].color = isAvailable ? Color.white : Color.grey;
            touchpadImages[(int)TouchpadSprite.down].color = isAvailable ? Color.white : Color.grey;
            touchpadScripts[(int)TouchpadSprite.up].isDisabled = !isAvailable;
            touchpadScripts[(int)TouchpadSprite.down].isDisabled = !isAvailable;
        }
        public void SetLeftRight(bool isAvailable)
        {
            isLeftRightOn = isAvailable;
            touchpadImages[(int)TouchpadSprite.left].color = isAvailable ? Color.white : Color.grey;
            touchpadImages[(int)TouchpadSprite.right].color = isAvailable ? Color.white : Color.grey;
            touchpadScripts[(int)TouchpadSprite.left].isDisabled = !isAvailable;
            touchpadScripts[(int)TouchpadSprite.right].isDisabled = !isAvailable;
        }
        public void SetMid(bool isAvailable)
        {
            isMidOn = isAvailable;
            touchpadImages[(int)TouchpadSprite.mid].color = isAvailable ? Color.white : Color.grey;
            touchpadScripts[(int)TouchpadSprite.mid].isDisabled = !isAvailable;
        }
        public void SetMainMessage()
        {
            string msgType;
      
            if (ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].disabled)
                msgType = "Disabled";
            else if (ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].isOn)
                msgType = "On";
            else msgType = "Available";
            try
            {
                SetRotatorText(MainLineManagers[ViveSR_Experience.rotator.currentButtonNum].mainLines.First(x => x.messageType == msgType).text);
            }
            catch
            {
                Debug.LogWarning("[Tutorial] The line for the " + msgType + " state is not found. Check " + (Button)ViveSR_Experience.rotator.currentButtonNum + "'s MainLineManager.");
            }
        }

        public void ToggleTutorial(bool isOn)
        {
            allowUpdate = isOn;

            SetRotatorCanvas(isOn);
            SetTouchpadImage(isOn);

            foreach (Text txt in tutorialTexts) txt.text = "";

            if (isOn && ViveSR_Experience.rotator.currentButtonNum == (int) Button.Tutorial) SetMainMessage();
            else
            {
                currentSprite = TouchpadSprite.none;
                RunSpriteAnimation(); //stop previous coroutine;
            }
        }

        //Adjust the position of the rotator canvas.
        public IEnumerator MoveTowards(bool isDisabled)
        {
            Vector3 targetPos;

            if (isDisabled) targetPos = PosOnDisable;
            else targetPos = OriginalPos;

            Transform canvasTransform = tutorialTexts[(int)TextCanvas.onRotator].gameObject.transform.parent.transform;

            while (Vector3.Distance(canvasTransform.localPosition, targetPos) > 0.01)
            {
                canvasTransform.localPosition = Vector3.MoveTowards(canvasTransform.localPosition, targetPos, 0.1f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            canvasTransform.localPosition = targetPos;
        }

        public void SetRotatorCanvas(bool on)
        {
            tutorialCanvases[(int)TextCanvas.onRotator].SetActive(on);
        }
        public void SetTouchpadCanvas(bool on)
        {
            tutorialCanvases[(int)TextCanvas.onTouchPad].SetActive(on);
        }
        public void SetRotatorText(string text)
        {
            tutorialTexts[(int)TextCanvas.onRotator].text = text;
        }
        public void SetTouchpadText(string text)
        {
            tutorialTexts[(int)TextCanvas.onTouchPad].text = text;
        }

        public void SetTouchpadImage(bool on)
        {
            touchpadImageGroup.SetActive(on);
        }
    }
}