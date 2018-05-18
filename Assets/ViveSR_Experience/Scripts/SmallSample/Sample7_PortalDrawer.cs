using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample7_PortalDrawer : MonoBehaviour
    {
        enum ActionMode
        {
            StaticMesh = 0,            
            ThrowDart,
            Portal,
            ViewCollision,
        }
        int numModes = 4;

        [SerializeField] ActionMode actionMode;
        [SerializeField] ViveSR_Experience_DartThrowGenerator dartGenerator;
        [SerializeField] ViveSR_Experience_PortalSpawner portalSpawner;

        public string mesh_path = "Recons3DAsset/Model.obj";
        public string cld_path = "Recons3DAsset/Model_cld.obj";

        //store loaded mesh
        public GameObject texturedMesh;
        public GameObject collisionMesh;

        [SerializeField] ViveSR_Experience_Recons3DAssetLoader ReconsLoader;
        [SerializeField] bool ShowMeshTexture;
        [SerializeField] bool isNewMesh = true;

        int percentage = 0;
        int lastPercentage = 0;
        bool isLoading = false;

        //[SerializeField]
        public Text ScanText, StopText, SaveText, LoadText, HintText, ThrowableText;
        Color oldColor_l, oldColor_r, oldColor_d;

        ViveSR_StaticColliderPool cldPool;

        private void Awake()
        {
            ThrowableText.text = "Grip To Throw Item";
        }
        //Percentage of the saving process
        IEnumerator SetPercentage()
        {
            if (!isNewMesh)
            {
                Destroy(texturedMesh);
                Destroy(collisionMesh);
            }

            LoadText.color = Color.grey;

            while (percentage < 100)
            {
                ViveSR_RigidReconstruction.GetExportProgress(ref percentage);
                HintText.text = "Saving..." + percentage.ToString() + "%";
                // wait until saving is really processing then we disable others
                if (lastPercentage == 0 && percentage > 0)
                    ViveSR_DualCameraImageCapature.EnableDepthProcess(false);
                lastPercentage = percentage;
                yield return new WaitForEndOfFrame();
            }

            isNewMesh = true;
            percentage = 0;
            lastPercentage = 0;

            HintText.text = "Mesh Saved!";
            ScanText.color = Color.white;
            LoadText.color = Color.white;
        }

        IEnumerator waitForMeshLoad()
        {
            while (!ReconsLoader.isMeshReady || !ReconsLoader.isColliderReady)
            {
                yield return new WaitForEndOfFrame();
            }

            isLoading = false;
            foreach (MeshRenderer renderer in ReconsLoader.meshRnds)
            {
                renderer.sharedMaterial.SetFloat("_StencilValue", 0);
                renderer.sharedMaterial.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
            }

            foreach (MeshRenderer renderer in ReconsLoader.cldRnds)
            {
                renderer.sharedMaterial.SetFloat("_StencilValue", 0);
                renderer.sharedMaterial.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
            }

            HintText.text = "Mesh Loaded!";
            ScanText.color = Color.white;
        }

        private void Start()
        {
            if (File.Exists(mesh_path) && File.Exists(cld_path))
            {
                LoadText.color = Color.white;
            }
        }

        private void Update()
        {
            if (ViveSR_Experience.targetHand != null)
            {
                SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;
                SwitchControlMode(controller);

                if (actionMode == ActionMode.StaticMesh)
                    MeshOperation(controller);
                else if (actionMode == ActionMode.Portal)
                    PortalOperation(controller);
            }
        }

        private void MeshOperation(SteamVR_Controller.Device controller)
        {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (touchPad.x > 0.5f)// [Save]
                {
                    if (ViveSR_RigidReconstruction.IsScanning)
                    {
                        ViveSR_RigidReconstruction.ExportAdaptiveMesh = true;
                        ViveSR_RigidReconstruction.ExportModel("Model"); //Export scanned mesh

                        StartCoroutine(SetPercentage()); //Read saving percentage

                        ScanText.color = Color.grey;
                        StopText.color = Color.grey;
                        SaveText.color = Color.grey;
                    }
                }
                else if (touchPad.x < -0.5f)//[Stop]
                {
                    if (ViveSR_RigidReconstruction.IsScanning)
                    {
                        ViveSR_RigidReconstruction.StopScanning(); //Stop scanning
                        ViveSR_DualCameraImageCapature.EnableDepthProcess(false); //Turn off depth engine

                        ScanText.color = Color.white;
                        StopText.color = Color.grey;
                        SaveText.color = Color.grey;
                    }
                }
                else if (touchPad.y > 0.5f)//[Scan]
                {
                    if (!ViveSR_RigidReconstruction.IsScanning)
                    {
                        foreach (GameObject obj in dartGenerator.InstantiatedDarts) Destroy(obj);
                        dartGenerator.InstantiatedDarts.Clear();

                        if (!isNewMesh)
                        {
                            texturedMesh.SetActive(false);
                            collisionMesh.SetActive(false);
                            LoadText.color = Color.white;
                        }

                        HintText.text = "";

                        ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
                        ViveSR_DualCameraImageCapature.EnableDepthProcess(true); //Turn on depth engine
                        ViveSR_RigidReconstruction.StartScanning(); //Start scanning

                        ScanText.color = Color.gray;
                        SaveText.color = Color.white;
                        StopText.color = Color.white;
                    }
                }
                else if (touchPad.y < 0.5f)//[Load]
                {
                    if (ViveSR_RigidReconstruction.IsScanning)
                    {
                        ViveSR_RigidReconstruction.StopScanning(); //Stop scanning
                        ViveSR_DualCameraImageCapature.EnableDepthProcess(false); //Turn off depth engine

                        ScanText.color = Color.white;
                        SaveText.color = Color.grey;
                        StopText.color = Color.grey;
                    }

                    if (isNewMesh)
                    {
                        if (File.Exists(mesh_path) && File.Exists(cld_path))
                        {
                            HintText.text = "Loading..";
                            isLoading = true;

                            texturedMesh = ReconsLoader.LoadMeshObj(mesh_path); //Load textured mesh
                            collisionMesh = ReconsLoader.LoadColliderObj(cld_path); //Load collision

                            StartCoroutine(waitForMeshLoad());

                            isNewMesh = false;

                            LoadText.color = Color.grey;
                        }
                    }
                    else
                    {
                        texturedMesh.SetActive(true);
                        collisionMesh.SetActive(true);
                    }
                }
            }
        }
        private void PortalOperation(SteamVR_Controller.Device controller)
        {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (touchPad.x > 0.5f)// [Draw]
                {
                    HintText.text = "Draw Portal";
                    portalSpawner.actionMode = PortalActionMode.DrawAndScale;
                }
                else if (touchPad.x < -0.5f)//[Del]
                {
                    HintText.text = "Delete A Portal";
                    portalSpawner.actionMode = PortalActionMode.ShootAndKill;
                }
                else if (touchPad.y > 0.5f)//[Cast]
                {
                    HintText.text = "Cast A Portal";
                    portalSpawner.actionMode = PortalActionMode.ShootAndSpawn;
                }
                else if (touchPad.y < -0.5f)//[Clear]
                {
                    HintText.text = "Cast A Portal";
                    portalSpawner.ClearAll();
                }
                
            }
        }

        private void SwitchControlMode(SteamVR_Controller.Device controller)
        {
            if (!ViveSR_RigidReconstruction.IsExporting && !ViveSR_RigidReconstruction.IsScanning && !isLoading )
            {
                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
                {
                    actionMode = (ActionMode)(((int)actionMode + 1) % numModes);

                    if (actionMode == ActionMode.StaticMesh)
                    {
                        ToggleShowCollider(false);

                        StopText.color = oldColor_l;
                        SaveText.color = oldColor_r;
                        LoadText.color = oldColor_d;

                        HintText.text = "Static Mesh";
                        StopText.text = "[Stop]";
                        SaveText.text = "[Save]";
                        LoadText.text = "[Load]";
                        ScanText.text = "[Scan]";
                        ThrowableText.text = "Grip To Throw Item";
                    }
                    else if (actionMode == ActionMode.ThrowDart)
                    {
                        oldColor_l = StopText.color;
                        oldColor_r = SaveText.color;
                        oldColor_d = LoadText.color;

                        StopText.color = Color.white;
                        SaveText.color = Color.white;
                        LoadText.color = Color.white;

                        dartGenerator.enabled = true;

                        HintText.text = "Throw Dart";
                        ScanText.text = "Hold";
                        StopText.text = "Trigger";
                        SaveText.text = "To";
                        LoadText.text = "Throw";                        
                        ThrowableText.text = "Grip To Portal";
                    }
                    else if (actionMode == ActionMode.Portal)
                    {
                        dartGenerator.enabled = false;
                        portalSpawner.enabled = true;
                        portalSpawner.actionMode = PortalActionMode.ShootAndSpawn;

                        HintText.text = "Cast A Portal";
                        ScanText.text = "[Cast]";
                        StopText.text = "[Del]";
                        SaveText.text = "[Draw]";
                        LoadText.text = "[ClearAll]";
                        ThrowableText.text = "Grip To View Collider";
                    }
                    else if (actionMode == ActionMode.ViewCollision)
                    {
                        portalSpawner.enabled = false;

                        ScanText.text = "";
                        StopText.text = "";
                        SaveText.text = "";
                        LoadText.text = "";
                        ThrowableText.text = "Grip To Mesh Scanning";

                        HintText.text = "Show Colliders";
                        ToggleShowCollider(true);
                    }
                }
            }            
        }

        void ToggleShowCollider(bool show)
        {
            if (collisionMesh == null) return;
            cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
            if (cldPool == null) return;

            if (show) cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
            else      cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.BOUND_RECT_SHAPE });
        }
    }
}
