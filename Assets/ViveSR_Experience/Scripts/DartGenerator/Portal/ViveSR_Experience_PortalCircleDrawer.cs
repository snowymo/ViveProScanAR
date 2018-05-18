using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_PortalCircleDrawer : ViveSR_Experience_IPortalDrawer
    {
        [Range(0.1f, 1.0f)] public float minRadius = 0.1f;
        [Range(0.1f, 1.0f)] public float maxRadius = 1.0f;
        [Range(0.01f, 0.1f)] public float coplanarDistThresh = 0.03f;
        [Range(0.0f, 0.05f)] public float spawnHeightOffset = 0.02f;

        private RaycastHit hitInfo;
        private Vector3 startSpawnPos;

        private bool isHolding = false;
        private float placementOffset;
        private Vector3 originScale;

        const float TEMP_SHIFT = 0.03f;

        // try to find a best hit-info and portal size
        public bool CheckValidHit(RaycastHit hitInfo, ViveSR_StaticColliderInfo cldInfo)
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
                transform.forward = (portalMgr.viewerInWorld == WorldMode.RealWorld) ? hitNormal : -hitNormal;
                transform.position = hitPos -hitNormal * placementOffset;
                
                originScale = transform.localScale;
                originScale.Scale(new Vector3(currentTestRadius * 2, currentTestRadius * 2, 1.0f ));
                transform.localScale = originScale;
            }

            return success;
        }

        protected override void StartToDo()
        {
            isHolding = true;

            Vector3 fwd = RaycastStartPoint.transform.forward;

            startSpawnPos = RaycastStartPoint.transform.position + fwd * 0.3f;
        }

        protected override void TriggerHold()
        {         
            Vector3 fwd = RaycastStartPoint.transform.forward;

            if (isHolding)
            {
                Vector3 curPos = RaycastStartPoint.transform.position + fwd * 0.3f;

                float scale = 1.0f + Vector3.Distance(startSpawnPos, curPos) * 3.0f;
                Vector3 newScale = new Vector3(originScale.x * scale, originScale.y * scale, 1.0f);
                transform.localScale = newScale;
            }

            lineRenderer.SetPosition(0, RaycastStartPoint.transform.position);
            lineRenderer.SetPosition(1, fwd * 10.0f + RaycastStartPoint.transform.position);
        }

        protected override void TriggerRelease()
        {
            FinishDrawing();
        }

        public override void FinishDrawing()
        {
            isHolding = false;
            lineRenderer.startColor = lineRenderer.endColor = Color.white;

            portalMgr.AddPortal(gameObject);
            lineRenderer.enabled = false;
            Destroy(this);
        }
    }
}