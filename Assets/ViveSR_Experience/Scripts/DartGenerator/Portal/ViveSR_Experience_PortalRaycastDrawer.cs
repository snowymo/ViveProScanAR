using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_PortalRaycastDrawer : ViveSR_Experience_IPortalDrawer
    {
        [SerializeField] GameObject SnapBase;
        [SerializeField] GameObject RealWTrigger, VirtualWTrigger;

        [SerializeField] MeshFilter meshFilter;
        [SerializeField] List<Vector3> Vertices;
        [SerializeField] List<int> Triangles;
        Vector3 previousGizmoPos;
        RaycastHit hitInfo;

        ViveSR_Portal portalScript;

        Vector4 planeEquation;
                
        protected override void AwakeToDo()
        {
            portalScript = GetComponent<ViveSR_Portal>();
        }

        protected override void StartToDo()
        {
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.Clear();

            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray();

            Vector3 fwd = RaycastStartPoint.transform.forward * 5f;
            Physics.Raycast(RaycastStartPoint.transform.position, fwd, out hitInfo);

            if (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject.name == "Model_cld")
            {
                SnapBase.SetActive(true);
                SnapBase.transform.forward = -RaycastStartPoint.transform.forward;
                SnapBase.transform.position = hitInfo.point;
            }
            else
            {
                SnapBase.SetActive(true);
                fwd = RaycastStartPoint.transform.forward * 0.3f;
                SnapBase.transform.position = fwd + RaycastStartPoint.transform.position;
                SnapBase.transform.LookAt(RaycastStartPoint.transform);
            }
            planeEquation = -RaycastStartPoint.transform.forward;
        }

        protected override void TriggerHold()
        {
            Vector3 fwd = RaycastStartPoint.transform.forward * 5f;
            Physics.Raycast(RaycastStartPoint.transform.position, fwd, out hitInfo);

            lineRenderer.SetPosition(0, RaycastStartPoint.transform.position);

            if (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject.name == SnapBase.name)
            {
                SnapBase.SetActive(true);
                lineRenderer.SetPosition(1, hitInfo.point);
                if (Vector3.Distance(previousGizmoPos, hitInfo.point) > 0.01) //need to filter distance
                {
                    DrawingPortal();
                }
            }
            else
            {
                lineRenderer.SetPosition(1, fwd + RaycastStartPoint.transform.position);
            }
        }

        protected override void TriggerRelease()
        {
            if (meshFilter.mesh.vertices.Length < 10 ||
            meshFilter.mesh.bounds.size.x * meshFilter.mesh.bounds.size.y * meshFilter.mesh.bounds.size.z < 0.0001f)
            {
                lineRenderer.enabled = false;
                Destroy(gameObject);
            }
            else
            {
                //setPlaneNormal
                Destroy(SnapBase);
                lineRenderer.enabled = false;
                
                float planeD = -Vector3.Dot(planeEquation, meshFilter.mesh.bounds.center);
                portalScript.planeEquation = new Vector4(planeEquation.x, planeEquation.y, planeEquation.z, planeD);

                RealWTrigger.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
                VirtualWTrigger.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;

                Destroy(this);
            }
        }                  
        public void DrawingPortal()
        {
            Vector3 currentPos = ViveSR_Experience.AttachPoint.transform.position;
                  
            Mesh mesh = meshFilter.mesh;

            Vertices.Add(hitInfo.point);

            if (Vertices.Count > 2)
            {
                Triangles.Add(0);
                Triangles.Add(Vertices.Count - 2);
                Triangles.Add(Vertices.Count - 1);
            }

            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray(); 

            previousGizmoPos = hitInfo.point;
        }      
    }
}