using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Vive.Plugin.SR.Experience
{
    public class Sample_SetPlayerController : MonoBehaviour
    {
        static private Sample_SetPlayerController _instance;
        static public Sample_SetPlayerController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Sample_SetPlayerController>();
                }
                return _instance;
            }
        }

        [SerializeField] GameObject PlayerHeadCollision;
        public GameObject AttachPoint;

        private void Awake()
        {
            ViveSR_Experience.PlayerHeadCollision = PlayerHeadCollision;
            ViveSR_Experience.AttachPoint = AttachPoint;
        }
        void Update()
        {
            if (ViveSR_Experience.targetHand == null)
            {
                try
                {
                    if (Player.instance.GetHand(0).AttachedObjects.Count > 0) ViveSR_Experience.targetHand = Player.instance.GetHand(0).gameObject;
                    else if (Player.instance.GetHand(1).AttachedObjects.Count > 0) ViveSR_Experience.targetHand = Player.instance.GetHand(1).gameObject;

                    if (ViveSR_Experience.targetHand != null)
                    {
                        ViveSR_Experience.targetHandScript = ViveSR_Experience.targetHand.GetComponent<Hand>();

                        //prevent controller disappearing when holding gameobjs
                        HideOnHandFocusLost[] handHiders;
                        handHiders = FindObjectsOfType<HideOnHandFocusLost>();
                        foreach (HideOnHandFocusLost handHider in handHiders) Destroy(handHider);

                        //Move playerHeadCollision to follow the headset.
                        GameObject PlayerHead = GameObject.Find("Camera (eye)").gameObject;
                        PlayerHeadCollision.transform.parent = PlayerHead.transform;
                        PlayerHeadCollision.transform.localPosition = Vector3.zero;
                        PlayerHeadCollision.transform.localEulerAngles = Vector3.zero;
                                                                                 
                        AttachPoint.transform.parent = ViveSR_Experience.targetHand.transform.Find("Attach_ControllerTip").transform;
                        AttachPoint.transform.localPosition = new Vector3(0f, 0.015f, 0.02f);
                        AttachPoint.transform.localEulerAngles = new Vector3(60f, 0f, 0f);

                        Destroy(GameObject.Find("HeadCollider").gameObject);

                        Destroy(ViveSR_Experience.targetHand.transform.Find("ControllerHoverHighlight").gameObject);    //don't allow highlight from steamVR
                    }
                }
                catch
                {
                    Debug.LogWarning("Controller not Found");
                }
            }
        }
    }
}