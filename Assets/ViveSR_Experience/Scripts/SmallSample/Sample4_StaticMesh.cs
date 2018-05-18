using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(Sample_SetPlayerController))]
    public class Sample4_StaticMesh : MonoBehaviour
    {      
        enum ActionMode
        {
            StaticMesh = 0,
            ViewCollision,
            ThrowItem,
        }
        int numModes = 2;
        [SerializeField] ActionMode actionMode;
        ActionMode actionMode_old;
                                                                           
        public string mesh_path = "Recons3DAsset/Model.obj";
        public string cld_path = "Recons3DAsset/Model_cld.obj";

        //store loaded mesh
        public GameObject texturedMesh;
        public GameObject collisionMesh;
        public MeshRenderer[] modelRenderers;

        [SerializeField] ViveSR_Experience_Recons3DAssetLoader ReconsLoader;

        bool ShowMeshTexture;     

        [SerializeField] bool isNewMesh = true;

        int percentage = 0;
        int lastPercentage = 0;

        [SerializeField]
        ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr;

        public Text ScanText, StopText, SaveText, LoadText, HintText, ThrowableText, GribText, MidText;

        ViveSR_StaticColliderPool cldPool;
        // 0: show all, 
        // 1: show all horizon, 
        // 2: show largest horizon, 
        // 3: show all vertical, 
        // 4: show largest vertical
        // 5: no show
        private uint showCldMode = 5;
        private MeshRenderer curCldRenderer = null;

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

                SwitchControlModel(controller); //Grip
                SetDartGeneratorUI(controller); //Toy

                if (actionMode == ActionMode.StaticMesh)
                    MeshOperation(controller);
                else if (actionMode == ActionMode.ViewCollision)
                    ColliderOperation(controller);
            }
        }

        private void SwitchControlModel(SteamVR_Controller.Device controller)
        {
            if (!ViveSR_RigidReconstruction.IsExporting && !ViveSR_RigidReconstruction.IsScanning)
            {
                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
                {
                    actionMode = (ActionMode)(((int)actionMode + 1) % numModes);

                    if (actionMode == ActionMode.StaticMesh)
                    {
                        SetStaticMeshUI();
                    }
                    else if (actionMode == ActionMode.ViewCollision)
                    {
                        SwitchShowCollider();
                        SetViewCollisionUI();
                    }
                }
            }
        }

        /*------------------------------mesh-------------------------------------*/
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
                        dartGeneratorMgr.DestroyObjs();

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
       
        //Percentage of the saving process
        IEnumerator SetPercentage()
        {  
            if(!isNewMesh)
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
            modelRenderers = ReconsLoader.meshRnds;
                                                                    
            HintText.text = "Mesh Loaded!";
            ScanText.color = Color.white;            
        }

        /*------------------------------mesh end---------------------------------*/


        /*-------------------------collider display-----------------------------*/
        private void ColliderOperation(SteamVR_Controller.Device controller)
        {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (Vector2.Distance(touchPad, Vector2.zero) < 0.5)
                {
                    ShowMeshTexture = !ShowMeshTexture;
                    SetMeshTexture(ShowMeshTexture);
                }
                else if (touchPad.y > 0.5f)
                {
                    showCldMode = (showCldMode + 1) % 6;
                    SwitchShowCollider();
                }
                else if (touchPad.y < -0.5f)
                {
                    showCldMode = (showCldMode + 5) % 6;
                    SwitchShowCollider();
                } 
            }
        }                    

        void SetMeshTexture(bool show)
        {
            foreach (MeshRenderer renderer in modelRenderers)
            {
                renderer.material.shader = Shader.Find(show ? "ViveSR/Unlit, Textured, Shadowed, Stencil" : "ViveSR/MeshCuller, Shadowed, Stencil");
            }
        }

        void SwitchShowCollider()
        {
            if (collisionMesh == null) return;
            cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
            if (cldPool == null) return;

            if (showCldMode == 0)   // all colliders
            {
                HintText.text = "All Colliders";
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
            }
            else if (showCldMode == 1)   // all horizontal
            {
                HintText.text = "All Horizon";
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL });
            }
            else if (showCldMode == 2)  // largest horizontal
            {
                HintText.text = "Largest Horizon";
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.BOUND_RECT_SHAPE });  // cancel;
                curCldRenderer = null;
                ViveSR_StaticColliderInfo info = cldPool.GetLargestCollider(ColliderShapeType.MESH_SHAPE, PlaneOrientation.HORIZONTAL);
                if (info)
                {
                    curCldRenderer = info.GetComponent<MeshRenderer>();
                    curCldRenderer.enabled = true;
                }
            }
            else if (showCldMode == 3)  // all vertical
            {
                HintText.text = "All Vertical";
                if (curCldRenderer) curCldRenderer.enabled = false;
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL });
            }
            else if (showCldMode == 4)  // largest vertical
            {
                HintText.text = "Largest Vertical";
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.BOUND_RECT_SHAPE });  // cancel;
                curCldRenderer = null;
                ViveSR_StaticColliderInfo info = cldPool.GetLargestCollider(ColliderShapeType.MESH_SHAPE, PlaneOrientation.VERTICAL);
                if (info)
                {
                    curCldRenderer = info.GetComponent<MeshRenderer>();
                    curCldRenderer.enabled = true;
                }
            }
            else if (showCldMode == 5)  // no show
            {
                HintText.text = "No Show CLD";
                if (curCldRenderer) curCldRenderer.enabled = false;
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.BOUND_RECT_SHAPE });
            }
        }
        /*----------------------------------------------------------------------*/


        /*--------------------------------Dart----------------------------------*/   
        void SetDartGeneratorUI(SteamVR_Controller.Device controller)
        {
            if (!ViveSR_RigidReconstruction.IsExporting && !ViveSR_RigidReconstruction.IsScanning)
            {
                if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    actionMode_old = actionMode;
                    actionMode = ActionMode.ThrowItem;
                    SetThrowUI();
                }
                else if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
                {
                    if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
                    {
                        Vector2 touchPad = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                        if (touchPad.y > 0.5)
                            SetDartTypeUI();
                    }
                }
                else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    actionMode = actionMode_old;
                    if (actionMode == ActionMode.ViewCollision)
                        SetViewCollisionUI();
                    else
                        SetStaticMeshUI();
                }
            }
        }

        void SetThrowUI()
        {
            StopText.color = ScanText.color = SaveText.color = LoadText.color = Color.white;

            SetDartTypeUI();

            StopText.text = "<";
            SaveText.text = ">";
            LoadText.text = "[Clear]";
            MidText.text = "";
            ThrowableText.text = "Click on '<' or '>' to Change Toy";
            GribText.text = "";
        }

        void SetDartTypeUI()
        {
            if (HintText.text == "Raycast")
            {
                HintText.text = HintText.text = "Throw";
                ScanText.text = ScanText.text = "[Raycast]";
            }                
            else if(HintText.text == "Throw")
            {
                HintText.text = HintText.text = "Raycast";
                ScanText.text = ScanText.text = "[Throw]";
            }
            else
            {
                if (dartGeneratorMgr.dartPlacementMode == DartPlacementMode.Throwable)
                {
                    HintText.text = HintText.text = "Throw";
                    ScanText.text = ScanText.text = "[Raycast]";
                }
                else if (dartGeneratorMgr.dartPlacementMode == DartPlacementMode.Raycast)
                {
                    HintText.text = HintText.text = "Raycast";
                    ScanText.text = ScanText.text = "[Throw]";
                }
            }
        }

        void SetStaticMeshUI()
        {
            StopText.color = Color.grey;
            SaveText.color = Color.grey;
            LoadText.color = (File.Exists(mesh_path) && File.Exists(cld_path)) ? Color.white : Color.grey;
            ScanText.color = Color.white;

            HintText.text = "Static Mesh";
            StopText.text = "[Stop]";
            SaveText.text = "[Save]";
            LoadText.text = "[Load]";
            ScanText.text = "[Scan]";
            MidText.text = "";
            ThrowableText.text = "Hold Trigger to Throw Item";
            GribText.text = "Grip to View Collider";
        }

        void SetViewCollisionUI()
        {
            ScanText.color = LoadText.color = StopText.color = MidText.color = modelRenderers.Length != 0 ? Color.white : Color.grey;

            Debug.Log(LoadText.color);

            HintText.text = "Colliders";
            ScanText.text = "[Prev]";
            StopText.text = "";
            SaveText.text = "";
            LoadText.text = "[Next]";
            MidText.text = "[Texture]";
            ThrowableText.text = "Hold Trigger to Throw Item";
            GribText.text = "Grip to Operate Mesh";
        }
        /*----------------------------------------------------------------------*/  

    }
}