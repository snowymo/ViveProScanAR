/*
 * References used within the ViveSR_Experience namespace.
 * Find ViveController if it's lost.
 */

using UnityEngine;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience : MonoBehaviour
    {
        public static GameObject targetHand;
        public static Hand targetHandScript;

        [SerializeField] ViveSR_Experience_Rotator _rotator;
        public static ViveSR_Experience_Rotator rotator;
 
        [SerializeField] List<GameObject> _Buttons;
        public static List<GameObject> Buttons;

        [SerializeField] List<ViveSR_Experience_IButton> _ButtonScripts;
        public static List<ViveSR_Experience_IButton> ButtonScripts;

        [SerializeField] List<Renderer> _ButtonRenderers;
        public static List<Renderer> ButtonRenderers;

        [SerializeField] bool _showControllerModel;
        static bool showControllerModel;

        static List<Renderer> controllerRenderers = new List<Renderer>();
        GameObject ControllerObjGroup;

        [SerializeField] GameObject _PlayerHeadCollision;
        public static GameObject PlayerHeadCollision;
        GameObject PlayerHead;

        [SerializeField] GameObject _AttachPoint;
        public static GameObject AttachPoint;

        IEnumerator CollectControllerParts()
        {
            while (controllerRenderers.Count == 0)
            {
                for (int i = 0; i < ControllerObjGroup.transform.childCount; i++)
                {
                    Renderer targetRenderer = ControllerObjGroup.transform.GetChild(i).GetComponent<Renderer>();
                    if (targetRenderer != null) controllerRenderers.Add(targetRenderer);
                }
                yield return new WaitForEndOfFrame();
            }
            SetControllerRenderer(false);
        }
        
        public static void SetControllerRenderer(bool On)
        {
            foreach(Renderer rndr in controllerRenderers)
            {
                rndr.enabled = On;
            }
        }

        public static bool ShowControllerModel()
        {
            return showControllerModel;
        }

        private void Awake()
        {
            Player.instance.allowToggleTo2D = false;

            showControllerModel = _showControllerModel;
            rotator = _rotator;
            Buttons = _Buttons;
            ButtonScripts = _ButtonScripts;
            ButtonRenderers = _ButtonRenderers;
            PlayerHeadCollision = _PlayerHeadCollision;
            AttachPoint = _AttachPoint;

            RenderButtons(true);
        }

        private void Update()
        {
            DetectHand();
        }

        void DetectHand()
        {
            if (targetHand == null)
            {
                try
                {
                    if (Player.instance.GetHand(0).AttachedObjects.Count > 0) targetHand = Player.instance.GetHand(0).gameObject;
                    else if (Player.instance.GetHand(1).AttachedObjects.Count > 0) targetHand = Player.instance.GetHand(1).gameObject;

                    if (targetHand != null)
                    {                                                      
                        targetHandScript = targetHand.GetComponent<Hand>();
                        
                        //prevent controller disappearing when holding gameobjs
                        HideOnHandFocusLost[] handHiders;
                        handHiders = FindObjectsOfType<HideOnHandFocusLost>();
                        foreach (HideOnHandFocusLost handHider in handHiders) Destroy(handHider);

                        //Move playerHeadCollision to follow the headset.
                        PlayerHead = GameObject.Find("Camera (eye)").gameObject;
                        PlayerHeadCollision.transform.parent = PlayerHead.transform;
                        PlayerHeadCollision.transform.localPosition = Vector3.zero;
                        PlayerHeadCollision.transform.localEulerAngles = Vector3.zero;

                        Destroy(targetHand.transform.Find("ControllerHoverHighlight").gameObject);    //don't allow highlight from steamVR
                        Destroy(GameObject.Find("HeadCollider").gameObject);

                        //Get controller parts to hide the virtual model
                        if (!showControllerModel)
                        {
                            ControllerObjGroup = GameObject.Find("SteamVR_RenderModel").gameObject;
                            StartCoroutine(CollectControllerParts());
                        }
                    }
                }
                catch
                {
                    Debug.LogWarning("Controller not Found");
                }
            }                                                                                                                          
        }
        public static void RenderButtons(bool on)
        {
            foreach (Renderer renderer in ButtonRenderers)
                renderer.enabled = on;
        }
    }
}






