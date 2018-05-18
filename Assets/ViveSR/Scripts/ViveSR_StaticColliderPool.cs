using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    [ExecuteInEditMode]
    public class ViveSR_StaticColliderPool : MonoBehaviour
    {
        private List<ViveSR_StaticColliderInfo> allColliders = new List<ViveSR_StaticColliderInfo>();
        private int numClds;

        // belows: helper
        //private List<ViveSR_StaticColliderInfo> horizontal = new List<ViveSR_StaticColliderInfo>();
        //private List<ViveSR_StaticColliderInfo> vertical = new List<ViveSR_StaticColliderInfo>();

        private float largestBBAreaH = -1.0f;
        private float largestBBAreaV = -1.0f;
        private ViveSR_StaticColliderInfo largestHorizontalBB;
        private ViveSR_StaticColliderInfo largestVerticalBB;

        private float largestConvexAreaH = -1.0f;
        private float largestConvexAreaV = -1.0f;
        private ViveSR_StaticColliderInfo largestHorizontalConvex;
        private ViveSR_StaticColliderInfo largestVerticalConvex;

        private float largestMeshAreaH = -1.0f;
        private float largestMeshAreaV = -1.0f;
        private ViveSR_StaticColliderInfo largestHorizontalMesh;
        private ViveSR_StaticColliderInfo largestVerticalMesh;
        //

        private List<ViveSR_StaticColliderInfo> tempList = new List<ViveSR_StaticColliderInfo>();

#if UNITY_EDITOR
        public ColliderShapeType queriedShape;
        public PlaneOrientation queriedOrient;
        public bool doingQuery = false;
        private bool lastDoingQuery = false;
#endif

        // data pre-proc
        static public bool ProcessDataAndGenColliderInfo(GameObject go)
        {
            // organize and category collider type
            bool hasCollider = false;
            MeshFilter[] mFilters = go.GetComponentsInChildren<MeshFilter>();
            int numRnds = mFilters.Length;
            for (int id = 0; id < numRnds; ++id)
            {
                ViveSR_StaticColliderInfo cldInfo = mFilters[id].gameObject.AddComponent<ViveSR_StaticColliderInfo>();
                string meshName = mFilters[id].name;
                bool thisIsCLD = false;

                if (meshName.Contains("PlaneConvexCollider"))
                {
                    mFilters[id].gameObject.name = "PlaneConvexCollider";
                    cldInfo.SetBit((int)ColliderShapeType.CONVEX_SHAPE);
                    thisIsCLD = true;
                }
                else if (meshName.Contains("PlaneBBCollider"))
                {
                    mFilters[id].gameObject.name = "PlaneBBCollider";
                    cldInfo.SetBit((int)ColliderShapeType.BOUND_RECT_SHAPE);
                    thisIsCLD = true;
                }
                else if (meshName.Contains("PlaneMeshCollider"))
                {
                    mFilters[id].gameObject.name = "PlaneMeshCollider";
                    cldInfo.SetBit((int)ColliderShapeType.MESH_SHAPE);
                    thisIsCLD = true;
                }

                if (meshName.Contains("Horizontal")) cldInfo.SetBit((int)PlaneOrientation.HORIZONTAL);
                else if (meshName.Contains("Vertical")) cldInfo.SetBit((int)PlaneOrientation.VERTICAL);
                else cldInfo.SetBit((int)PlaneOrientation.OBLIQUE);

                hasCollider = (hasCollider || thisIsCLD);
                if (!thisIsCLD)
                {
                    Component.DestroyImmediate(cldInfo);
                }
                else
                {
                    // parse area
                    int areaStringStartIdx = meshName.LastIndexOf("Area_");
                    if (areaStringStartIdx != -1)
                    {
                        areaStringStartIdx = areaStringStartIdx + 5;
                        string curString = meshName.Substring(areaStringStartIdx);
                        int areaStringEndIdx = curString.IndexOf("_");
                        cldInfo.ApproxArea = float.Parse(curString.Substring(0, areaStringEndIdx));
                    }
                    else
                    {
                        cldInfo.SetBit((int)PlaneOrientation.FRAGMENT);
                    }

                    // parse normal
                    int normalStringStartIdx = meshName.LastIndexOf("Normal_");
                    if (normalStringStartIdx != -1)
                    {
                        normalStringStartIdx = normalStringStartIdx + 7;
                        string curString = meshName.Substring(normalStringStartIdx);
                        int normalXEndIdx = curString.IndexOf("_");
                        cldInfo.GroupNormal.x = float.Parse(curString.Substring(0, normalXEndIdx));

                        curString = curString.Substring(normalXEndIdx + 1);
                        int normalYEndIdx = curString.IndexOf("_");
                        cldInfo.GroupNormal.y = float.Parse(curString.Substring(0, normalYEndIdx));

                        curString = curString.Substring(normalYEndIdx + 1);
                        int normalZEndIdx = curString.IndexOf("_");
                        cldInfo.GroupNormal.z = float.Parse(curString.Substring(0, normalZEndIdx));
                    }
                }
            }

            return hasCollider;
        }

        public void OrganizeHierarchy()
        {
            ViveSR_StaticColliderInfo[] infos = GetComponentsInChildren<ViveSR_StaticColliderInfo>(true);
            int len = infos.Length;

            GameObject meshCldGroup = new GameObject("PlaneMeshColliderGroup");
            {
                meshCldGroup.transform.SetParent(transform);
                GameObject HorizontalGroup = new GameObject("Horizontal");
                GameObject VerticalGroup = new GameObject("Vertical");
                GameObject ObliqueGroup = new GameObject("Oblique");
                GameObject FragmentGroup = new GameObject("Fragment");
                {
                    HorizontalGroup.transform.SetParent(meshCldGroup.transform);
                    VerticalGroup.transform.SetParent(meshCldGroup.transform);
                    ObliqueGroup.transform.SetParent(meshCldGroup.transform);
                    FragmentGroup.transform.SetParent(meshCldGroup.transform);
                }                
            }
            meshCldGroup.SetActive(true);

            GameObject convexCldGroup = new GameObject("PlaneConvexColliderGroup");
            {
                convexCldGroup.transform.SetParent(transform);
                GameObject HorizontalGroup = new GameObject("Horizontal");
                GameObject VerticalGroup = new GameObject("Vertical");
                GameObject ObliqueGroup = new GameObject("Oblique");
                {
                    HorizontalGroup.transform.SetParent(convexCldGroup.transform);
                    VerticalGroup.transform.SetParent(convexCldGroup.transform);
                    ObliqueGroup.transform.SetParent(convexCldGroup.transform);
                }
            }
            convexCldGroup.SetActive(false);

            GameObject bbCldGroup = new GameObject("PlaneBoundingRectColliderGroup");
            {
                bbCldGroup.transform.SetParent(transform);
                GameObject HorizontalGroup = new GameObject("Horizontal");
                GameObject VerticalGroup = new GameObject("Vertical");
                GameObject ObliqueGroup = new GameObject("Oblique");
                {
                    HorizontalGroup.transform.SetParent(bbCldGroup.transform);
                    VerticalGroup.transform.SetParent(bbCldGroup.transform);
                    ObliqueGroup.transform.SetParent(bbCldGroup.transform);
                }
            }
            bbCldGroup.SetActive(false);

            for (int i = 0; i < len; ++i)
            {
                Transform parent = transform;
                ViveSR_StaticColliderInfo cldInfo = infos[i];
                if (cldInfo.CheckHasAllBit((uint)ColliderShapeType.MESH_SHAPE)) parent = parent.Find("PlaneMeshColliderGroup");
                else if (cldInfo.CheckHasAllBit((uint)ColliderShapeType.CONVEX_SHAPE)) parent = parent.Find("PlaneConvexColliderGroup");
                else if (cldInfo.CheckHasAllBit((uint)ColliderShapeType.BOUND_RECT_SHAPE)) parent = parent.Find("PlaneBoundingRectColliderGroup");

                if (cldInfo.CheckHasAllBit((uint)PlaneOrientation.HORIZONTAL)) parent = parent.Find("Horizontal");
                else if (cldInfo.CheckHasAllBit((uint)PlaneOrientation.VERTICAL)) parent = parent.Find("Vertical");
                else if (cldInfo.CheckHasAllBit((uint)PlaneOrientation.OBLIQUE)) parent = parent.Find("Oblique");
                else parent = parent.Find("Fragment"); // this should only appear in PlaneMesh

                infos[i].transform.SetParent(parent, true);
                infos[i].gameObject.AddComponent<MeshCollider>();

                MeshRenderer rnd = infos[i].gameObject.GetComponent<MeshRenderer>();
                if (rnd)
                {
                    Material wireframe = new Material(Shader.Find("ViveSR/Wireframe"));
                    wireframe.SetFloat("_ZTest", 0);
                    wireframe.SetFloat("_Thickness", 0);
                    rnd.sharedMaterial = wireframe;
                    rnd.enabled = false;
                }
            }
        }

        // Unity 
        void Start()
        {
            ViveSR_StaticColliderInfo[] infoArray = GetComponentsInChildren<ViveSR_StaticColliderInfo>(true);
            for (int i = 0; i < infoArray.Length; ++i)
                this.AddColliderInfo(infoArray[i]);
        }

        public void AddColliderInfo(ViveSR_StaticColliderInfo info)
        {
            if (!allColliders.Contains(info))
                allColliders.Add(info);

            if (info.CheckHasAllBit((uint)PlaneOrientation.HORIZONTAL))
            {
                //if (!horizontal.Contains(info))
                //    horizontal.Add(info);

                if (info.CheckHasAllBit((uint)ColliderShapeType.BOUND_RECT_SHAPE))
                {
                    if (largestBBAreaH < info.ApproxArea)
                    {
                        largestBBAreaH = info.ApproxArea;
                        largestHorizontalBB = info;
                    }
                }
                else if (info.CheckHasAllBit((uint)ColliderShapeType.CONVEX_SHAPE))
                {
                    if (largestConvexAreaH < info.ApproxArea)
                    {
                        largestConvexAreaH = info.ApproxArea;
                        largestHorizontalConvex = info;
                    }
                }
                else if (info.CheckHasAllBit((uint)ColliderShapeType.MESH_SHAPE))
                {
                    if (largestMeshAreaH < info.ApproxArea)
                    {
                        largestMeshAreaH = info.ApproxArea;
                        largestHorizontalMesh = info;
                    }
                }
            }
            else if (info.CheckHasAllBit((uint)PlaneOrientation.VERTICAL))
            {
                //if (!vertical.Contains(info))
                //    vertical.Add(info);

                if (info.CheckHasAllBit((uint)ColliderShapeType.BOUND_RECT_SHAPE))
                {
                    if (largestBBAreaV < info.ApproxArea)
                    {
                        largestBBAreaV = info.ApproxArea;
                        largestVerticalBB = info;
                    }
                }
                else if (info.CheckHasAllBit((uint)ColliderShapeType.CONVEX_SHAPE))
                {
                    if (largestConvexAreaV < info.ApproxArea)
                    {
                        largestConvexAreaV = info.ApproxArea;
                        largestVerticalConvex = info;
                    }
                }
                else if (info.CheckHasAllBit((uint)ColliderShapeType.MESH_SHAPE))
                {
                    if (largestMeshAreaV < info.ApproxArea)
                    {
                        largestMeshAreaV = info.ApproxArea;
                        largestVerticalMesh = info;
                    }
                }
            }

            numClds = allColliders.Count;
        }

        public ViveSR_StaticColliderInfo GetClosestCollider(Vector3 testPos)
        {
            return null;
        }

        public ViveSR_StaticColliderInfo GetFurthestCollider(Vector3 testPos)
        {
            return null;
        }

        public ViveSR_StaticColliderInfo GetLargestCollider(ColliderShapeType shapeType, PlaneOrientation orient)
        {
            if (shapeType == ColliderShapeType.MESH_SHAPE)
            {
                if (orient == PlaneOrientation.HORIZONTAL)
                    return largestHorizontalMesh;
                else if (orient == PlaneOrientation.VERTICAL)
                    return largestVerticalMesh;
            }
            else if (shapeType == ColliderShapeType.CONVEX_SHAPE)
            {
                if (orient == PlaneOrientation.HORIZONTAL)
                    return largestHorizontalConvex;
                else if (orient == PlaneOrientation.VERTICAL)
                    return largestVerticalConvex;
            }
            else if (shapeType == ColliderShapeType.BOUND_RECT_SHAPE)
            {
                if (orient == PlaneOrientation.HORIZONTAL)
                    return largestHorizontalBB;
                else if (orient == PlaneOrientation.VERTICAL)
                    return largestVerticalBB;
            }

            return null;
        }

        public ViveSR_StaticColliderInfo[] GetAllColliderHasProps(uint[] props)
        {
            int numProps = props.Length;
            tempList.Clear();

            uint bits = 0;
            for (int i = 0; i < numProps; ++i)
                bits |= props[i];

            for (int j = 0; j < numClds; ++j)
            {
                if (allColliders[j].CheckHasAllBit(bits))
                    tempList.Add(allColliders[j]);
            }     
            return tempList.ToArray();
        }

#if UNITY_EDITOR
        void Update()
        {
            if (doingQuery && !lastDoingQuery)
            {
                ShowAllColliderWithProps(new uint[] { (uint)queriedShape, (uint)queriedOrient });
            }

            lastDoingQuery = doingQuery;
        }
#endif

        public void ShowAllColliderWithProps(uint[] props)
        {
            int num = tempList.Count;
            for (int i = 0; i < num; ++i)
            {
                MeshRenderer rnd = tempList[i].GetComponent<MeshRenderer>();
                if (rnd) rnd.enabled = false;
            }

            GetAllColliderHasProps(props);

            num = tempList.Count;
            for (int i = 0; i < num; ++i)
            {
                MeshRenderer rnd = tempList[i].GetComponent<MeshRenderer>();
                if (rnd == null)
                    rnd = tempList[i].gameObject.AddComponent<MeshRenderer>();

                rnd.enabled = true;
            }
        }

    }
}


