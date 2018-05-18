using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    public enum WorldMode
    {
        RealWorld = 0,
        VRWorld = 1
    }

    public class ViveSR_PortalMgr : MonoBehaviour
    {
        public int realWorldStencilValue = 0;
        public int virtualWorldStencilValue = 1;

        public MeshRenderer stencilCleaner;
        public MeshRenderer depthCleaner;        

        public WorldMode viewerInWorld = WorldMode.RealWorld;
        public WorldMode controllerInWorld = WorldMode.RealWorld;

        public Material controllerMaterial;

        private List<ViveSR_Portal> portals = new List<ViveSR_Portal>();

        private Material leftSeeThruMat;
        private Material rightSeeThruMat;
        private Shader oriSeeThruShader;
        private Shader stencilSeeThruShader;
        private Shader ctrllerOriShader;

        private Camera cam;

        // Use this for initialization
        void Start()
        {
            cam = GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogError("No portal camera found!");
                return;
            }
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("VirtualWorldLayer"));

            cam.depthTextureMode = DepthTextureMode.Depth;
            Transform mainCamParent = Camera.main.transform.parent;
            cam.transform.SetParent(mainCamParent);            

            MeshRenderer leftSeeThru = ViveSR_DualCameraRig.Instance.TrackedCameraLeft.ImagePlane.GetComponent<MeshRenderer>();
            MeshRenderer rightSeeThru = ViveSR_DualCameraRig.Instance.TrackedCameraRight.ImagePlane.GetComponent<MeshRenderer>();
            leftSeeThruMat = leftSeeThru.sharedMaterial;
            rightSeeThruMat = rightSeeThru.sharedMaterial;
            oriSeeThruShader = rightSeeThruMat.shader;
 
            stencilSeeThruShader = Shader.Find("ViveSR/Unlit, Textured, Stencil");
            leftSeeThruMat.shader = stencilSeeThruShader;
            leftSeeThruMat.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
            leftSeeThruMat.SetFloat("_ZWrite", 0);
            leftSeeThruMat.shader = oriSeeThruShader;

            rightSeeThruMat.shader = stencilSeeThruShader;
            rightSeeThruMat.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
            rightSeeThruMat.SetFloat("_ZWrite", 0);
            rightSeeThruMat.shader = oriSeeThruShader;

            if (controllerMaterial != null)
            {
                ctrllerOriShader = controllerMaterial.shader;
                controllerMaterial.shader = Shader.Find("ViveSR/Standard, Stencil");
            }
        }

        void OnDisable()
        {
            if (controllerMaterial != null) controllerMaterial.shader = ctrllerOriShader;
            leftSeeThruMat.shader = oriSeeThruShader;
            rightSeeThruMat.shader = oriSeeThruShader;
        }

        public void AddPortal(GameObject portalGO)
        {
            ViveSR_Portal portal = portalGO.GetComponent<ViveSR_Portal>();
            if ( portal )
            {
                portals.Add(portal);
                portal.SetRenderRule(viewerInWorld, realWorldStencilValue, virtualWorldStencilValue);
                portal.UpdatePlaneNormal();
            }
        }

        public void ClearPortal(GameObject portalGO)
        {
            ViveSR_Portal portal = portalGO.GetComponent<ViveSR_Portal>();
            if (portal)
            {
                // if it is in the list
                if (portals.Remove(portal))
                {
                    Destroy(portal.gameObject);
                }
            }
        }

        public void ClearAllPortals()
        {
            foreach (ViveSR_Portal portal in portals)
            {
                Destroy(portal.gameObject);
            }
            portals.Clear();
        }


        public void UpdateViewerWorld()
        {
            if (viewerInWorld == WorldMode.VRWorld)
            {
                cam.depth = -1;
                cam.clearFlags = CameraClearFlags.Color;
                // do some modification of SR Camera
                ViveSR_DualCameraRig.Instance.DualCameraLeft.clearFlags = CameraClearFlags.Nothing;
                ViveSR_DualCameraRig.Instance.DualCameraRight.clearFlags = CameraClearFlags.Nothing;
                ViveSR_DualCameraRig.Instance.VirtualCamera.clearFlags = CameraClearFlags.Nothing;
                leftSeeThruMat.shader = stencilSeeThruShader;                
                rightSeeThruMat.shader = stencilSeeThruShader;
                // ----

                // clear the screen stencil to the virtual world stencil
                stencilCleaner.material.SetFloat("_StencilValue", virtualWorldStencilValue);
                stencilCleaner.enabled = true;
                depthCleaner.material.SetFloat("_StencilValue", realWorldStencilValue);
                depthCleaner.material.renderQueue = 3999;
            }
            else
            {
                cam.depth = 3;
                cam.clearFlags = CameraClearFlags.Nothing;
                // restore SR Camera to it's default
                ViveSR_DualCameraRig.Instance.DualCameraLeft.clearFlags = CameraClearFlags.Skybox;
                ViveSR_DualCameraRig.Instance.DualCameraRight.clearFlags = CameraClearFlags.Skybox;
                ViveSR_DualCameraRig.Instance.VirtualCamera.clearFlags = CameraClearFlags.Depth;
                leftSeeThruMat.shader = oriSeeThruShader;
                rightSeeThruMat.shader = oriSeeThruShader;
                // ----

                // clear the screen stencil to the real world stencil ( 0 = exactly the default value)
                //stencilCleaner.material.SetFloat("_StencilValue", realWorldStencilValue);
                stencilCleaner.enabled = false;                
                depthCleaner.material.SetFloat("_StencilValue", virtualWorldStencilValue);
                depthCleaner.material.renderQueue = 999;
            }

            foreach (ViveSR_Portal portal in portals)
                portal.SetRenderRule(viewerInWorld, realWorldStencilValue, virtualWorldStencilValue);
        }
    }
}

