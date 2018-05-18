using UnityEngine;
namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(LineRenderer))]
    public class ViveSR_Experience_DartRaycastGenerator : ViveSR_Experience_IDartGenerator
    {
        RaycastHit hitInfo;
        LineRenderer lineRenderer;

        protected override void AwakeToDo()
        {
            lineRenderer = GetComponent<LineRenderer>();                                    
        }

        private void OnEnable()
        {
            lineRenderer.enabled = true;
        }
        private void OnDisable()
        {
            lineRenderer.enabled = false;
        }

        public override void TriggerPress()
        {
            lineRenderer.enabled = true;
            GenerateDart();
            InstantiatedDarts.Add(currentGameObj);

            isHolding = true;
        }

        protected override void TriggerHold()
        {
            Vector3 fwd = transform.forward;
            Physics.Raycast(transform.position, fwd, out hitInfo);
            lineRenderer.SetPosition(0, transform.position);
            if (hitInfo.rigidbody != null)
            {
                lineRenderer.endColor = Color.green;
                currentGameObj.SetActive(true);
                currentGameObj.transform.position = hitInfo.point;
                currentGameObj.transform.up = hitInfo.normal;
                lineRenderer.SetPosition(1, hitInfo.point);

            }
            else
            {   
                lineRenderer.endColor = Color.red;
                currentGameObj.SetActive(false);
                lineRenderer.SetPosition(1, fwd * 0.5f + transform.position);
            }
        }

        public override void TriggerRelease()
        {
            lineRenderer.endColor = Color.white;
            lineRenderer.enabled = false;
            if (hitInfo.rigidbody == null) Destroy(currentGameObj);

            ViveSR_Experience.targetHandScript.DetachObject(currentGameObj);

            currentGameObj.transform.parent = null;
            isHolding = false;
        }

        protected override void GenerateDart()
        {
            currentGameObj = Instantiate(dart_prefabs[currentDartPrefeb]);
            currentGameObj.transform.eulerAngles = Vector3.zero;
            currentGameObj.GetComponent<ViveSR_Experience_Dart>().dartGeneratorMgr = dartGeneratorMgr;
        }
    }
}