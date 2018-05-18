using UnityEngine;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_DartPortalGenerator : ViveSR_Experience_IDartGenerator
    {                                       
        RaycastHit hitInfo;
        LineRenderer lineRenderer;
        [SerializeField] ViveSR_PortalMgr portalMgr;
        ViveSR_Experience_IPortalDrawer portalDrawer;

        protected override void AwakeToDo()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        public override void TriggerPress()
        {
            GenerateDart();
            InstantiatedDarts.Add(currentGameObj);

            isHolding = true;
        }

        protected override void TriggerHold()
        {
        }

        public override void TriggerRelease()
        {                    
            isHolding = false;
        }

        protected override void GenerateDart()
        {
            lineRenderer.enabled = true;
            currentGameObj = Instantiate(dart_prefabs[currentDartPrefeb]);
            currentGameObj.transform.eulerAngles = Vector3.zero;

            portalDrawer = currentGameObj.GetComponent<ViveSR_Experience_IPortalDrawer>();
            portalDrawer.lineRenderer = lineRenderer;
            portalDrawer.portalMgr = portalMgr;
            portalDrawer.RaycastStartPoint = gameObject;
        }

        IEnumerator LineRenderer_CirclePortal()
        {
            while (currentDartPrefeb == 1)
            {
                Vector3 fwd = transform.forward;
                Physics.Raycast(transform.position, fwd, out hitInfo); ;

                if (hitInfo.collider != null)
                {
                    ViveSR_StaticColliderInfo cldInfo = hitInfo.collider.GetComponent<ViveSR_StaticColliderInfo>();
                    if (cldInfo)
                    {
                        if (((ViveSR_Experience_PortalCircleDrawer)portalDrawer).CheckValidHit(hitInfo, cldInfo) == false)
                            lineRenderer.startColor = lineRenderer.endColor = Color.red;    // invalid plane
                        else
                            lineRenderer.startColor = lineRenderer.endColor = Color.blue;  // valid hit
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
                yield return new WaitForEndOfFrame();
            }
        }


    }
}