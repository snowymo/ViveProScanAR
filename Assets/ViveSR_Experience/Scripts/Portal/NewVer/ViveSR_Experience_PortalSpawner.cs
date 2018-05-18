using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public enum PortalActionMode
    {
        DrawAndScale,
        ShootAndSpawn,
        ShootAndKill,
    }
    public class ViveSR_Experience_PortalSpawner : MonoBehaviour
    {
        public PortalActionMode actionMode = PortalActionMode.DrawAndScale;
        [Range(0.1f, 1.0f)]
        public float minRadius = 0.1f;
        [Range(0.1f, 1.0f)]
        public float maxRadius = 1.0f;
        [Range(0.01f, 0.1f)]
        public float coplanarDistThresh = 0.03f;
        [Range(0.0f, 0.05f)]
        public float spawnHeightOffset = 0.02f;

        [SerializeField] ViveSR_PortalMgr portalManager;
        [SerializeField] GameObject RaycastStartPoint;
        [SerializeField] GameObject Prefab;
        [SerializeField] LineRenderer lineRenderer;

        private GameObject currentGameObject;
        private RaycastHit hitInfo;
        private Vector3 startSpawnPos;

        private bool isHolding = false;
        private float placementOffset;
        private Vector3 originScale;

        const float TEMP_SHIFT = 0.03f;

        // try to find a best hit-info and portal size
        private bool CheckValidHit(RaycastHit hitInfo, ViveSR_StaticColliderInfo cldInfo)
        {
            // ignore fragile type
            if (cldInfo.ApproxArea <= 0.0f)
                return false;
            
            //Vector3 hitNormal = hitInfo.normal;
            Vector3 hitNormal = cldInfo.GroupNormal;
            Vector3 hitPos = hitInfo.point;
            Vector3 right = RaycastStartPoint.transform.right;
            Vector3 up = Vector3.Cross(hitNormal, right);
            right = Vector3.Cross(up, hitNormal);

            float currentTestRadius = maxRadius;
            bool success = false;

            while ( !success && (currentTestRadius >= minRadius) )
            {
                placementOffset = -spawnHeightOffset;
                success = true;
                
                // ray cast for the 25 corners to see if the current size is fitting
                for ( int x = -2; x <= 2; x += 1 )
                {
                    for ( int y = -2; y <= 2; y += 1)
                    {
                        RaycastHit tempHit;
                        Vector3 testPos = hitPos + (right * currentTestRadius * x * 0.5f) + (up * currentTestRadius * y * 0.5f);
                        Physics.Raycast(testPos + hitNormal * TEMP_SHIFT, -hitNormal, out tempHit);
                        float testDist = tempHit.distance - TEMP_SHIFT;
                        if ((tempHit.collider == null) || (Mathf.Abs(testDist) > coplanarDistThresh) /*|| (tempHit.collider.gameObject != cldInfo.gameObject) */)
                        {
                            //if (tempHit.collider == null) Debug.Log("No Hit");
                            //else if (Mathf.Abs(testDist) > coplanarDist) Debug.Log("Too Far");
                            success = false;
                            currentTestRadius -= 0.025f;
                            break;
                        }

                        placementOffset = Mathf.Min(placementOffset, testDist);
                    }
                    if ( !success ) break;
                }
            }


            if (success)
            {
                currentGameObject = Instantiate(Prefab);                
                // just make sure the order of (cld_front / cld_back) is correct
                currentGameObject.transform.forward = (portalManager.viewerInWorld == WorldMode.RealWorld) ? hitNormal : -hitNormal;
                currentGameObject.transform.position = hitPos -hitNormal * placementOffset;
                
                originScale = currentGameObject.transform.localScale;
                originScale.Scale(new Vector3(currentTestRadius * 2, currentTestRadius * 2, 1.0f ));
                currentGameObject.transform.localScale = originScale;

                portalManager.AddPortal(currentGameObject);
            }

            return success;
        }

        void DragToScale(SteamVR_Controller.Device controller)
        {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                isHolding = true;
                Vector3 fwd = RaycastStartPoint.transform.forward;
                startSpawnPos = RaycastStartPoint.transform.position + fwd * 0.3f;

                currentGameObject = Instantiate(Prefab);
                // just make sure the order of (cld_front / cld_back) is correct
                currentGameObject.transform.forward = (portalManager.viewerInWorld == WorldMode.RealWorld) ? -fwd : fwd;
                currentGameObject.transform.position = startSpawnPos;
                portalManager.AddPortal(currentGameObject);
            }
            else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                isHolding = false;
                currentGameObject = null;
            }

            if (isHolding)
            {
                Vector3 fwd = RaycastStartPoint.transform.forward;
                Vector3 curPos = RaycastStartPoint.transform.position + fwd * 0.3f;
                float scale = Vector3.Distance(startSpawnPos, curPos) * 5.0f;
                currentGameObject.transform.localScale = new Vector3(scale, scale, 1.0f);
            }
        }

        void ShootAndDrag(SteamVR_Controller.Device controller)
        {
            Vector3 fwd = RaycastStartPoint.transform.forward;

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                isHolding = true;
                startSpawnPos = RaycastStartPoint.transform.position + fwd * 0.3f;
                
                Physics.Raycast(RaycastStartPoint.transform.position, fwd, out hitInfo);
                if (hitInfo.collider != null)
                {
                    ViveSR_StaticColliderInfo cldInfo = hitInfo.collider.GetComponent<ViveSR_StaticColliderInfo>();
                    if (cldInfo)
                    {
                        if (CheckValidHit(hitInfo, cldInfo) == false)
                            lineRenderer.startColor = lineRenderer.endColor = Color.red;    // invalid plane
                        else
                            lineRenderer.startColor = lineRenderer.endColor = Color.white;  // valid hit
                    }
                    else
                    {
                        lineRenderer.startColor = lineRenderer.endColor = Color.green;
                    }
                }
                else
                {
                    // no hit
                    lineRenderer.startColor = lineRenderer.endColor = Color.green;
                }
            }
            else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                isHolding = false;
                currentGameObject = null;
                lineRenderer.startColor = lineRenderer.endColor = Color.white;
            }

            if (isHolding && currentGameObject != null)
            {
                Vector3 curPos = RaycastStartPoint.transform.position + fwd * 0.3f;
                float scale = 1.0f + Vector3.Distance(startSpawnPos, curPos) * 3.0f;
                Vector3 newScale = new Vector3(originScale.x * scale, originScale.y * scale, 1.0f);
                currentGameObject.transform.localScale = newScale;
            }

            lineRenderer.SetPosition(0, RaycastStartPoint.transform.position);
            lineRenderer.SetPosition(1, fwd * 10.0f + RaycastStartPoint.transform.position);
        }

        void Kill(SteamVR_Controller.Device controller)
        {
            Vector3 fwd = RaycastStartPoint.transform.forward;

            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                Physics.Raycast(RaycastStartPoint.transform.position, fwd, out hitInfo);
                if (hitInfo.collider != null && (hitInfo.collider.name == "PortalTrigger"))
                {
                    portalManager.ClearPortal(hitInfo.collider.gameObject.transform.root.gameObject);
                }
                else
                {
                    // no hit
                    lineRenderer.startColor = lineRenderer.endColor = Color.red;
                }
            }
            else if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                lineRenderer.startColor = lineRenderer.endColor = Color.white;
            }

            lineRenderer.SetPosition(0, RaycastStartPoint.transform.position);
            lineRenderer.SetPosition(1, fwd * 10.0f + RaycastStartPoint.transform.position);
        }

        void Start()
        {
            if (lineRenderer)
                lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        }

        void Update()
        {
            if (ViveSR_Experience.targetHand != null)
                HandleTriggerInput();

            lineRenderer.enabled = (actionMode == PortalActionMode.ShootAndSpawn) || (actionMode == PortalActionMode.ShootAndKill);
        }

        void OnDisable()
        {
            lineRenderer.enabled = false;
        }

        public void ClearAll()
        {
            portalManager.ClearAllPortals();
        }

        void HandleTriggerInput()
        {
            SteamVR_Controller.Device controller = ViveSR_Experience.targetHandScript.controller;

            if (actionMode == PortalActionMode.DrawAndScale)
            {
                DragToScale(controller);
            }
            else if (actionMode == PortalActionMode.ShootAndSpawn)    // Shoot And Attach
            {
                ShootAndDrag(controller);
            }
            else
            {
                Kill(controller);
            }

        }

    }
}