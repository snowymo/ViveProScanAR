using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vive.Plugin.SR
{
    public class ViveSR : MonoBehaviour
    {
        public static FrameworkStatus FrameworkStatus { get; protected set; }
        public static string LastError { get; protected set; }
        public bool EnableAutomatically;
        public ViveSR_DualCameraRig DualCameraRig;
        [SerializeField] protected ViveSR_RigidReconstructionRenderer RigidReconstruction;

        [HideInInspector] public List<UnityAction> OnStartFailed = new List<UnityAction>();
        [HideInInspector] public List<UnityAction> OnStartComplete = new List<UnityAction>();

        private static ViveSR Mgr = null;
        public static ViveSR Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("ViveSR does not be attached on GameObject");
                }
                return Mgr;
            }
        }

        // Use this for initialization
        void Start()
        {
            FrameworkStatus = FrameworkStatus.STOP;
            if (EnableAutomatically)
            {
                StartFramework();
            }
        }


        // Update is called once per frame
        void OnDestroy()
        {
            StopFramework();
        }

        public virtual void StartFramework()
        {
            if (FrameworkStatus == FrameworkStatus.WORKING) return;
            StartCoroutine(StartFrameworkCoroutine());
        }

        protected virtual IEnumerator StartFrameworkCoroutine()
        {
            int result = -1;
            // Before initialize framework
            yield return new WaitForEndOfFrame();
            do
            {
                if (DualCameraRig != null)
                {
                    if (ViveSR_DualCameraImageCapature.CheckNecessayFile()) result = 0;
                    else break;
                }
            } while (false);
            yield return new WaitForEndOfFrame();

            if (result == (int)Error.WORK) result = ViveSR_InitialFramework();
            if (result == (int)Error.WORK) Debug.Log("[ViveSR] Initial Framework : " + result);
            else
            {
                SetLastError("[ViveSR] Initial Framework : " + result);
                for (int i = 0; i < OnStartFailed.Count; i++) if (OnStartFailed[i] != null) OnStartFailed[i]();
                yield break;
            }
            yield return new WaitForEndOfFrame();

            if (RigidReconstruction != null) RigidReconstruction.InitRigidReconstructionParam();
            yield return new WaitForEndOfFrame();

            // Start framework
            if (result == (int)Error.WORK) result = ViveSR_StartFramework();
            if (result == (int)Error.WORK)
            {
                FrameworkStatus = FrameworkStatus.WORKING;
                Debug.Log("[ViveSR] Start Framework : " + result);
            }
            else
            {
                SetLastError("[ViveSR] Start Framework : " + result);
                for (int i = 0; i < OnStartFailed.Count; i++) if (OnStartFailed[i] != null) OnStartFailed[i]();
                yield break;
            }
            yield return new WaitForEndOfFrame();


            if (FrameworkStatus == FrameworkStatus.WORKING)
            {
                if (DualCameraRig != null)
                {
                    DualCameraRig.gameObject.SetActive(true);
                    DualCameraRig.Initial();
                }
                if (RigidReconstruction != null) RigidReconstruction.gameObject.SetActive(true);
            }
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < OnStartComplete.Count; i++) if (OnStartComplete[i] != null) OnStartComplete[i]();
        }

        public virtual void StopFramework()
        {
            if (DualCameraRig != null)
            {
                DualCameraRig.Release();
                DualCameraRig.gameObject.SetActive(false);
            }
            if (RigidReconstruction != null) RigidReconstruction.gameObject.SetActive(false);

            if (FrameworkStatus == FrameworkStatus.WORKING)
            {
                int result = ViveSR_StopFramework();
                if (result == (int)Error.WORK) FrameworkStatus = FrameworkStatus.STOP;
                else
                {
                    SetLastError("[ViveSR] Stop Framework : " + result);
                }
            }
            else
            {
                Debug.Log("[ViveSR] Stop Framework : not open");
            }
        }

        protected virtual int ViveSR_InitialFramework()
        {
            int result = (int)Error.FAILED;
            result = ViveSR_Framework.Initial();
            //result = ViveSR_SetLogLevel(10);

            result = ViveSR_Framework.CreateModule((int)ModuleDictionary.DEVICE_VIVE2_MODE2, ref ViveSR_Framework.MODULE_ID_DISTORTED);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.CreateModule((int)ModuleDictionary.ENGINE_UNDISTORTED, ref ViveSR_Framework.MODULE_ID_UNDISTORTED);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.CreateModule((int)ModuleDictionary.ENGINE_DEPTH, ref ViveSR_Framework.MODULE_ID_DEPTH);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.CreateModule((int)ModuleDictionary.ENGINE_RIGID_RECONSTRUCTION, ref ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }
            return result;
        }

        protected virtual int ViveSR_StartFramework()
        {
            int result = (int)Error.FAILED;

            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_DISTORTED);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_UNDISTORTED);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_DEPTH);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.ModuleLink(ViveSR_Framework.MODULE_ID_DISTORTED, ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)WorkLinkMethod.ACTIVE);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] ModuleLink Error " + result); return result; }

            //result = ViveSR_ModuleLink(MODULE_ID_UNDISTORTED, MODULE_ID_DEPTH, (int)WorkLinkMethod.ACTIVE);
            //if (result != (int)Error.WORK) { Debug.Log("ViveSR_ModuleLink Error " + result); return result; }

            result = ViveSR_Framework.ModuleLink(ViveSR_Framework.MODULE_ID_DEPTH, ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)WorkLinkMethod.ACTIVE);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] ModuleLink Error " + result); return result; }

            return result;
        }

        protected virtual int ViveSR_StopFramework()
        {
            return ViveSR_Framework.Stop();
        }

        protected void SetLastError(string errMsg)
        {
            FrameworkStatus = FrameworkStatus.ERROR;
            LastError = errMsg;
            Debug.LogError(errMsg);
        }
    }
}