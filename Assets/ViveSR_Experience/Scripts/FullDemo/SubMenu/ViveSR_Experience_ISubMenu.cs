using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{ 
    public class ViveSR_Experience_ISubMenu : MonoBehaviour
    {
        public bool isSubMenuOn;
        public int currentSubBtnNum;

        //Prevent coroutine overlap
        IEnumerator prevTrueCoroutine;
        IEnumerator prevFalseCoroutine;

        [SerializeField] Texture originalTexture;
        [SerializeField] Texture selectingModeTexture;

        public List<GameObject> subBtns;
        [SerializeField] public List<ViveSR_Experience_ISubBtn> subBtnScripts;
        
        float subBtnEnlargingSpeed = 2f;

        float tempTime = 0.1f;
        float cooldownTime = 0.1f;


        private void Awake()
        {
            AwakeToDo();
        }

        protected virtual void AwakeToDo() {}

        public virtual void ToggleSubMenu(bool isOn)
        {
            isSubMenuOn = isOn;
            RenderSubBtns(isOn);

            ViveSR_Experience.ButtonRenderers[ViveSR_Experience.rotator.currentButtonNum].material.mainTexture = isOn ? selectingModeTexture : originalTexture;
            ViveSR_Experience.ButtonRenderers[ViveSR_Experience.rotator.currentButtonNum].material.SetTexture("_EmissionMap", isOn ? selectingModeTexture : originalTexture);

            if (isSubMenuOn)
            {
                prevTrueCoroutine = Enlarge(currentSubBtnNum, true);
                StartCoroutine(prevTrueCoroutine);
                
                ViveSR_Experience.ButtonRenderers[ViveSR_Experience.rotator.currentButtonNum].material.SetColor("_EmissionColor", ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].BrightColor);
            }
            else
            {
                StartCoroutine(Enlarge(currentSubBtnNum, false));

                //when the user rotate away, the button can still be bright if there is something functioning in its submenu
                bool IsSomeSubBtnOn = false;
                foreach (ViveSR_Experience_ISubBtn subBtn in subBtnScripts)
                {
                    if (subBtn.isOn)
                    {
                        IsSomeSubBtnOn = true;
                        break;
                    }
                }
                ViveSR_Experience.ButtonRenderers[ViveSR_Experience.rotator.currentButtonNum].material.SetColor("_EmissionColor", IsSomeSubBtnOn ? ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].BrightColor : ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].OriginalEmissionColor);
                //------------
            }
        }
        public IEnumerator Enlarge(int currentSubBtnNum, bool on)
        {
            while (on ? subBtns[currentSubBtnNum].transform.localScale.y < 1.5 : subBtns[currentSubBtnNum].transform.localScale.y > 1)
            {
                subBtns[currentSubBtnNum].transform.localScale += (on? 1 : -1) * new Vector3(subBtnEnlargingSpeed * Time.deltaTime, subBtnEnlargingSpeed * Time.deltaTime, subBtnEnlargingSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            }
            subBtns[currentSubBtnNum].transform.localScale = on ? new Vector3(1.5f, 1.5f, 1.5f) : new Vector3(1f, 1f, 1f);
        }

        void Update()
        {
            if (isSubMenuOn) HandleTouchpadInput_SubMenu();
        }

        bool AllowHover()
        {
            return !ViveSR_Experience_SubBtn_3DPreview_Save.instance.IsSaving()
                   && !ViveSR_Experience_StaticMesh.instance.IsLoading();
        }

        void HandleTouchpadInput_SubMenu()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (ViveSR_Experience.targetHandScript != null &&
            controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

                if (Vector2.Distance(touchPad, Vector2.zero) < 0.5) //Act on subBtn
                {
                    Execute();
                }
                else if (touchPad.y >= 0.5) //Selection goes upwards
                {
                    if (AllowHover())
                    {
                        if (Time.timeSinceLevelLoad - tempTime > cooldownTime)
                        {
                            if (currentSubBtnNum - 1 > -1)
                            {
                                HoverBtn(-1);
                            }
                        }
                    }
                }
                else if (touchPad.y <= -0.5) //Selection goes downwards
                {
                    if (AllowHover())
                    {
                        if (Time.timeSinceLevelLoad - tempTime > cooldownTime)
                        {
                            if (currentSubBtnNum + 1 < subBtns.Count)
                            {
                                HoverBtn(1);
                            }
                        }
                    }
                }
            }
        }

        void HoverBtn(int accumulateNum)
        {
            //Prevent coroutine overlap
            StopCoroutine(prevTrueCoroutine);
            if (prevFalseCoroutine != null) StopCoroutine(prevFalseCoroutine);

            //Shrink the previously hovered subBtn
            prevFalseCoroutine = Enlarge(currentSubBtnNum, false);
            StartCoroutine(prevFalseCoroutine);

            currentSubBtnNum += accumulateNum;
            //Enlarges the currently hovered subBtn
            prevTrueCoroutine = Enlarge(currentSubBtnNum, true);
            StartCoroutine(prevTrueCoroutine);
            tempTime = Time.timeSinceLevelLoad;
        }
        
        //When mid is pressed...
        protected virtual void Execute()
        {
            if(!subBtnScripts[currentSubBtnNum].disabled) subBtnScripts[currentSubBtnNum].Execute();
        }

        public void RenderSubBtns(bool on)
        {
            foreach (ViveSR_Experience_ISubBtn subBtn in subBtnScripts)
                subBtn.renderer.enabled = on;
        }
    }
}