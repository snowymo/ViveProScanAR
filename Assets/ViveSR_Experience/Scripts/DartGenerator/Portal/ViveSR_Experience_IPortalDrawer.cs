using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_IPortalDrawer : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public ViveSR_PortalMgr portalMgr;
        public GameObject RaycastStartPoint;

        public virtual void FinishDrawing(){ }

        private void Awake()
        {
            AwakeToDo();
        }
        protected virtual void AwakeToDo() { }

        private void Start()
        {
            StartToDo();
        }

        protected virtual void StartToDo() { }

        private void Update()
        {  
            if (ViveSR_Experience.targetHand != null)
            {
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;
                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    TriggerPress(); 
                }
                else if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
                {
                    TriggerHold();
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    TriggerRelease();
                }
            }
        }

        protected virtual void TriggerPress() { }
        protected virtual void TriggerHold() { }
        protected virtual void TriggerRelease() { }
    }
}