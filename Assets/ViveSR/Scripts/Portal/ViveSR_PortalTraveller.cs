using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    [RequireComponent(typeof(Collider))]
    public class ViveSR_PortalTraveller : MonoBehaviour
    {
        public bool isController = false;
        public bool isPlayer = false;

        [SerializeField] bool isInRealWorld = true;
        [SerializeField] bool isInVirtualWorld = false;
        [SerializeField] bool isTransitioning = false;
        bool prevIsTransitioning = false;
        private WorldMode currentWorld;

        private ViveSR_PortalMgr portalMgr;
        private MeshRenderer[] renderers = null;
        private MeshRenderer[] dupRenderers = null;

        private Collider selfCld;
        private bool originIsTrigger;
        
        private MeshRenderer hitPortalRnd;
        private Vector3 originalScale;
        private ViveSR_Portal hitPortal;
        

        void Start()
        {
            portalMgr = FindObjectOfType<ViveSR_PortalMgr>();
            if (!isController && !isPlayer && portalMgr != null)
            {
                if (portalMgr.controllerInWorld == WorldMode.RealWorld)
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");
                    isInRealWorld = true;
                }
                else
                {
                    gameObject.layer = LayerMask.NameToLayer("VirtualWorldLayer");
                    isInRealWorld = false;
                }

                if (renderers == null) renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                SwitchMaterials( renderers, (isInRealWorld? WorldMode.RealWorld : WorldMode.VRWorld ) );
            }

            isInVirtualWorld = !isInRealWorld;
            selfCld = GetComponent<Collider>();
            originIsTrigger = selfCld.isTrigger;
        }

        void InitDuplicatedRenderer()
        {
            if (renderers == null) renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            int numRnds = renderers.Length;
            List<MeshRenderer> dupRndList = new List<MeshRenderer>();
            for (int i = 0; i < numRnds; ++i )
            {
                MeshRenderer rnd = renderers[i];
                GameObject dupGO = new GameObject(rnd.name + "_dup", typeof(MeshFilter), typeof(MeshRenderer));
                dupGO.transform.SetParent(rnd.transform, false);
                dupGO.GetComponent<MeshFilter>().mesh = rnd.GetComponent<MeshFilter>().mesh;

                MeshRenderer dupRnd = dupGO.GetComponent<MeshRenderer>();
                dupRnd.materials = rnd.materials;
                foreach (Material mat in dupRnd.materials)
                {
                    mat.shader = Shader.Find("ViveSR/Wireframe");
                    mat.SetFloat("_Thickness", 0.0f);
                    mat.SetColor("_Color", Color.white);
                }
                
                dupRnd.enabled = false;
                dupRndList.Add(dupRnd);
            }

            dupRenderers = dupRndList.ToArray();
        }

        public void SwitchMaterials( MeshRenderer[] targetRnds, WorldMode toWorld )
        {
            if (toWorld == WorldMode.RealWorld)
            {
                foreach (Renderer rnd in targetRnds)
                {
                    rnd.gameObject.layer = LayerMask.NameToLayer("Default");
                    foreach (Material mat in rnd.materials)
                    {
                        mat.SetFloat("_StencilValue", portalMgr.realWorldStencilValue);
                        mat.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                    }
                }                
            }
            else // ( VRWorld )
            {
                foreach (Renderer rnd in targetRnds)
                {
                    rnd.gameObject.layer = LayerMask.NameToLayer("VirtualWorldLayer");
                    foreach (Material mat in rnd.materials)
                    {
                        mat.SetFloat("_StencilValue", portalMgr.virtualWorldStencilValue);
                        mat.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                    }
                }            
            }
        }

        public void SetClippingPlaneEnable(MeshRenderer[] targetRnds, bool flag, WorldMode clipInWorld)
        {
            if ( flag == false )
            {
                foreach (Renderer rnd in targetRnds)
                {
                    foreach (Material mat in rnd.materials)
                        mat.DisableKeyword("CLIP_PLANE");
                }
                return;
            }

            Vector4 planeEquation = (clipInWorld == WorldMode.RealWorld) ? hitPortal.planeEquation : -hitPortal.planeEquation;
            foreach (Renderer rnd in targetRnds)
            {
                foreach (Material mat in rnd.materials)
                {
                    mat.EnableKeyword("CLIP_PLANE");
                    mat.SetVector("_ClipPlane", planeEquation);
                }
            }
        }

        private void CheckWorldSide(ViveSR_Portal portal)
        {
            Vector3 selfPos = transform.position;
            Vector4 testPoint = new Vector4(selfPos.x, selfPos.y, selfPos.z, 1.0f);

            isInRealWorld = (Vector4.Dot(portal.planeEquation, testPoint) >= 0);
            isInVirtualWorld = !isInRealWorld;

            currentWorld = (isInRealWorld ? WorldMode.RealWorld : WorldMode.VRWorld);
        }

        public void OnTriggerEnter(Collider other)
        {
            ViveSR_Portal otherPortal = other.transform.root.GetComponent<ViveSR_Portal>();
            if (otherPortal)
            {
                hitPortal = otherPortal;
                hitPortalRnd = hitPortal.GetComponentInChildren<MeshRenderer>();
                CheckWorldSide(hitPortal);  // check which world is now
                isTransitioning = true;
                selfCld.isTrigger = true;

                if (renderers == null) renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                SetClippingPlaneEnable(renderers, true, currentWorld);

                CheckTransitioningBehavious();
            }
        }

        public void OnTriggerExit(Collider other)
        {
            ViveSR_Portal otherPortal = other.transform.root.GetComponent<ViveSR_Portal>();
            if (otherPortal)
            {
                hitPortal = otherPortal;
                hitPortalRnd = hitPortal.GetComponentInChildren<MeshRenderer>();

                CheckWorldSide(hitPortal);  // check which world is now
                isTransitioning = false;
                selfCld.isTrigger = originIsTrigger;

                if (isPlayer)
                {
                    portalMgr.viewerInWorld = currentWorld;
                    portalMgr.UpdateViewerWorld();
                    gameObject.layer = LayerMask.NameToLayer( (currentWorld == WorldMode.RealWorld)? "Default" : "VirtualWorldLayer");
                }
                else
                {
                    if (isController) portalMgr.controllerInWorld = currentWorld;
                    if (renderers == null) renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                    SwitchMaterials(renderers, currentWorld);
                    SetClippingPlaneEnable(renderers, false, currentWorld);
                }

                CheckTransitioningBehavious();
            }
        }

        private void CheckTransitioningBehavious()
        {
            if (isTransitioning != prevIsTransitioning)
            {
                prevIsTransitioning = isTransitioning;

                // viewer is changing transitioning....this fixes flickering issue
                if (isPlayer)
                {
                    if (isTransitioning)
                    {
                        originalScale = hitPortalRnd.transform.localScale;
                        hitPortalRnd.transform.localPosition = new Vector3(0, 0, isInRealWorld ? -4 : 4);
                        hitPortalRnd.transform.localScale = new Vector3(30, 3.9f, 30);
                    }
                    else
                    {
                        hitPortalRnd.transform.localPosition = Vector3.zero;
                        hitPortalRnd.transform.localScale = originalScale;
                    }
                }
                // item is changing transitioning....
                else
                {
                    // rendering duplicated go: 
                    if (dupRenderers == null || dupRenderers.Length == 0) InitDuplicatedRenderer();

                    // when become transitioning, render duplicated item in other world
                    if (isTransitioning)
                    {
                        WorldMode dupItemWorld = (isInVirtualWorld ? WorldMode.RealWorld : WorldMode.VRWorld);
                        SwitchMaterials(dupRenderers, dupItemWorld);
                        SetClippingPlaneEnable(dupRenderers, true, dupItemWorld);
                    }

                    foreach (MeshRenderer dupRnd in dupRenderers) dupRnd.enabled = isTransitioning;
                }
            }
            // end if status is switched
        }

    }
}


