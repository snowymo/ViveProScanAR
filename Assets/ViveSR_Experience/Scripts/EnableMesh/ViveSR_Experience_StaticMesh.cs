using UnityEngine;
using System.IO;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_StaticMesh : MonoBehaviour
    {
        private static ViveSR_Experience_StaticMesh _instance;
        public static ViveSR_Experience_StaticMesh instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_StaticMesh>();
                }
                return _instance;
            }
        }

        [SerializeField] ViveSR_Experience_Recons3DAssetLoader ReconsLoader;
        [SerializeField] ViveSR_Experience_Tutorial_InputHandler_EnableMesh EnableMeshTutorialInput;
        [SerializeField] ViveSR_Experience_SubBtn_EnableMesh_Static StaticMeshScript;

        bool newMesh = true;
        bool isLoading;
        bool isMeshReady;

        [Header("LoadedMesh")]
        public GameObject texturedMesh;
        public GameObject collisionMesh;

        public MeshRenderer[] modelRenderers;

        bool wasDynamicMeshOn;

        ViveSR_StaticColliderPool cldPool;
        // 0: show all, 
        // 1: show all horizon, 
        // 2: show largest horizon, 
        // 3: show all vertical, 
        // 4: show largest vertical
        // 5: no show
        public uint showCldMode = 4;
        private MeshRenderer curCldRenderer = null;

        public bool IsMeshReady()
        {
            return isMeshReady;
        }

        public bool IsLoading()
        {
            return isLoading;
        }

        public void NewMeshSaved()
        {
            newMesh = true;
            isMeshReady = false;
            ViveSR_Experience_SubBtn_EnableMesh_Static.instance.EnableButton(true);
        }

        IEnumerator waitForMeshLoading()
        {
            while (!ReconsLoader.isMeshReady) {
                yield return new WaitForEndOfFrame();
            }
            MeshReady(ReconsLoader.meshRnds);
        }

        void MeshReady(MeshRenderer[] rnds)
        {
            modelRenderers = rnds;
            isMeshReady = true;

            int numRnds = rnds.Length;
            for (int id = 0; id < numRnds; ++id)
            {
                rnds[id].sharedMaterial.shader = Shader.Find(ViveSR_Experience_SubBtn_EnableMesh_VRMode.instance.isOn ? "ViveSR/Unlit, Textured, Shadowed, Stencil" : "ViveSR/MeshCuller, Shadowed, Stencil");
                rnds[id].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            ViveSR_Experience.RenderButtons(true);
            ViveSR_Experience_SubBtn_EnableMesh_Static.instance.SubMenu.RenderSubBtns(true);
            ViveSR_Experience_SubBtn_EnableMesh_Static.instance.disabled = false;

            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "[Enable Mesh]\nMesh Loaded!", true);

            if (ViveSR_Experience_Button_Tutorial.instance.isOn)
                EnableMeshTutorialInput.MeshReady();

            newMesh = false;
            isLoading = false;
        }

        public void LoadMesh(bool isOn)
        {
            if (isOn)
            {
                if (newMesh)
                {
                    string mesh_path = ViveSR_Experience_SubBtn_EnableMesh_Static.instance.mesh_path;
                    string cld_path = ViveSR_Experience_SubBtn_EnableMesh_Static.instance.cld_path;
                    if (File.Exists(mesh_path) && File.Exists(cld_path))
                    {
                        isLoading = true;

                        ViveSR_Experience.RenderButtons(false);
                        ViveSR_Experience_SubBtn_EnableMesh_Static.instance.SubMenu.RenderSubBtns(false);
                        ViveSR_Experience_SubBtn_EnableMesh_Static.instance.disabled = true;
                        ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "[Enable Mesh]\nLoading...", false);

                        //Load here.
                        texturedMesh = ReconsLoader.LoadMeshObj(mesh_path);
                        collisionMesh = ReconsLoader.LoadColliderObj(cld_path);
                        StartCoroutine(waitForMeshLoading());
                    }
                }
                else
                {
                    SetMesh(true);
                }
            }
            else
            {
                SetMesh(false);
            }
        }

        public void RenderMesh(bool show)
        {
            foreach (MeshRenderer renderer in modelRenderers) renderer.material.shader = Shader.Find(show ? "ViveSR/Unlit, Textured, Shadowed, Stencil" : "ViveSR/MeshCuller, Shadowed, Stencil");
        }
        void SetMesh(bool on)
        {
            if (texturedMesh != null && collisionMesh != null)
            {
                texturedMesh.SetActive(on);
                collisionMesh.SetActive(on);
            }
        }

        public void SwitchShowCollider()
        {
            showCldMode = (showCldMode + 1) % 6;

            if (collisionMesh == null) return;
            cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
            if (cldPool == null) return;

            if (showCldMode == 0)   // all colliders
            {
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
            }
            else if (showCldMode == 1)   // all horizontal
            {
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL });
            }
            else if (showCldMode == 2)  // largest horizontal
            {
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
                if (curCldRenderer) curCldRenderer.enabled = false;
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL });
            }
            else if (showCldMode == 4)  // largest vertical
            {
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
                if (curCldRenderer) curCldRenderer.enabled = false;
                cldPool.ShowAllColliderWithProps(new uint[] { (uint)ColliderShapeType.BOUND_RECT_SHAPE });
            }
        }
    }
}