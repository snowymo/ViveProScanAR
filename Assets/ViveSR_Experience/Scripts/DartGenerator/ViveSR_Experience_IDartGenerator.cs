using System.Collections.Generic;
using UnityEngine;
namespace Vive.Plugin.SR.Experience
{
    //public enum DartType
    //{   
    //    sphere = 0,
    //    deer = 1,
    //    dart = 2,
    //}
    public class ViveSR_Experience_IDartGenerator : MonoBehaviour
    {
        public bool isHolding = true;
        public int currentDartPrefeb;
        [SerializeField] protected List<GameObject> dart_prefabs;
        public List<GameObject> InstantiatedDarts;
        protected GameObject currentGameObj;

        [SerializeField] bool deleteOnDisable;

        protected ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr;

        private void Awake()
        {
            dartGeneratorMgr = GetComponent<ViveSR_Experience_DartGeneratorMgr>();
            AwakeToDo();
        }

        protected virtual void AwakeToDo() { }
        // Update is called once per frame
        void Update()
        {
            if (ViveSR_Experience.targetHand != null)
            {
                if (!ViveSR_RigidReconstruction.IsExporting && !ViveSR_RigidReconstruction.IsScanning)
                    HandleTriggerInput();   
            }
        }

        void HandleTriggerInput()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (Time.timeSinceLevelLoad - dartGeneratorMgr.tempTime > dartGeneratorMgr.coolDownTime)
                {
                    TriggerPress();
                }
            }
            else if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (isHolding)
                {
                    dartGeneratorMgr.tempTime = Time.timeSinceLevelLoad;

                    if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
                    {
                        Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                        if (touchPad.x > 0.5 || touchPad.x < -0.5)
                        {
                            SwitchDart(touchPad);
                        }
                        else if (touchPad.y > 0.5)
                        {
                            dartGeneratorMgr.SwitchPlacementMode();
                        }
                        else if(touchPad.y < 0.5)
                        {
                            dartGeneratorMgr.DestroyObjs();
                        }
                    }

                    TriggerHold(); 
                }
            }
            else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (isHolding)
                {        
                    TriggerRelease();
                }
            }
        }

        public virtual void TriggerPress() { }
        protected virtual void TriggerHold() {}
        public virtual void TriggerRelease() { }

        void SwitchDart(Vector2 touchPad)
        {     
            int typeNum = currentDartPrefeb;

            if (touchPad.x > 0.5) currentDartPrefeb = typeNum + 1 < dart_prefabs.Count ? typeNum + 1 : 0;
            else if (touchPad.x < -0.5) currentDartPrefeb = typeNum - 1 > -1 ? typeNum - 1 : dart_prefabs.Count - 1;

            Destroy(currentGameObj);
            InstantiatedDarts.RemoveAt(InstantiatedDarts.Count - 1);

            GenerateDart();
                                        
            InstantiatedDarts.Add(currentGameObj);
           
        }
        protected virtual void GenerateDart() { }

        private void OnDisable()
        {
            if (deleteOnDisable)
            {
                foreach (GameObject obj in InstantiatedDarts)
                    Destroy(obj);
                InstantiatedDarts.Clear();
            }
        }

        public void DestroyObjs()
        {
            if (isHolding)
            {
                for (int i = 0; i < InstantiatedDarts.Count - 1; i++)
                    Destroy(InstantiatedDarts[i]);
            }
            else
            {
                foreach (GameObject obj in InstantiatedDarts)
                    Destroy(obj);
            }
        }
         
               
    }
}