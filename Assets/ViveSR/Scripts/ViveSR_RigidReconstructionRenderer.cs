using System;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace Vive.Plugin.SR
{
    public class ViveSR_RigidReconstructionRenderer : MonoBehaviour
    {
        [Header("Init Config")]
        public string ConfigFilePath = "";        

        [Header("Rendering Control")]
        public ReconstructionQuality FullSceneQuality = ReconstructionQuality.MID;
        public ReconstructionLiveMeshExtractMode LiveMeshMode = ReconstructionLiveMeshExtractMode.VERTEX_WITHOUT_NORMAL;
        private ReconstructionLiveMeshExtractMode LastLiveMeshMode = ReconstructionLiveMeshExtractMode.VERTEX_WITHOUT_NORMAL;
        [Range(100, 1000)]
        public int RefreshIntervalMS = 300;
        
        public static ReconstructionDisplayMode LiveMeshDisplayMode { get; set; }
        private ReconstructionDisplayMode LastLiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
        private static int ThreadPeriod = 15;

        [SerializeField] private Material LiveMeshMaterial;
        [SerializeField] private Material WireframeMaterial;

        [Header("Information")]
        [SerializeField] private int VertexNum;
        [SerializeField] private int IndexNum;
        [SerializeField] private int ProcessedFrame;
        [SerializeField] private int VertStrideInFloat;
        [SerializeField] private int ColliderNum;
        private int LastProcessedFrame = 0;

        // Data
        private float[] VertexData;
        private int[] IndexData;
        private GameObject LiveMeshesGroups;   // put all mesh inside
        private const int ShowGameObjCount = 20;
        private static GameObject[] ShowGameObjs = new GameObject[ShowGameObjCount];
        private static MeshRenderer[] ShowMeshRnds = new MeshRenderer[ShowGameObjCount];
        private static MeshFilter[] ShowMeshFilters = new MeshFilter[ShowGameObjCount];
        private Material UsingMaterial;

        #region Multi-Thread Get Mesh Data
        // Multi-thread Parse Raw Data to Data List for Mesh object 
        private Thread MeshDataThread;
        private Coroutine MeshDataCoroutine = null; // IEnumerator for main thread Mesh
        private int NumSubMeshes = 0;
        private int LastMeshes = 0;
        private int NumLastMeshVert = 0;
        private List<Vector3>[] MeshDataVertices = new List<Vector3>[ShowGameObjCount];
        private List<int>[] MeshDataIndices = new List<int>[ShowGameObjCount];
        private List<Color32>[] MeshDataColors = new List<Color32>[ShowGameObjCount];
        private List<Vector3>[] MeshDataNormals = new List<Vector3>[ShowGameObjCount];
        private bool IsMeshUpdate = false;
        private bool IsCoroutineRunning = false;
        private bool IsThreadRunning = true;
        #endregion

        private bool LastIsScanning = false;
        private bool LastIsExporting = false;

        private ViveSR_RigidReconstructionRenderer() { }
        private static ViveSR_RigidReconstructionRenderer Mgr = null;
        public static ViveSR_RigidReconstructionRenderer Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR_RigidReconstructionRenderer>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("ViveSR_RigidReconstructionRenderer does not be attached on GameObject");
                }
                return Mgr;
            }
        }

        // set self-setting to the static param
        public bool InitRigidReconstructionParam()
        {
            bool result = ViveSR_RigidReconstruction.InitRigidReconstructionParamFromFile(ConfigFilePath);
            if (!result)
            {
                Debug.Log("[ViveSR] [RigidReconstruction] Set Config By Config File");
            }
            else
            {
                Debug.Log("[ViveSR] [RigidReconstruction] Config File Not Found, Set Config From GameObject");
                ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_QUALITY, (int)FullSceneQuality);
                //ViveSR_RigidReconstruction.InitRigidReconstructionParam();
            }

            return result;
        }

        public bool UpdateRuntimeParameter()
        {
            bool result = true;
            int ret = (int)Error.FAILED;

            // live mesh display mode
            if (LiveMeshDisplayMode != LastLiveMeshDisplayMode)
            {
                result = SetMeshDisplayMode(LiveMeshDisplayMode) && result;
                LastLiveMeshDisplayMode = LiveMeshDisplayMode;
            }

            // full scene quality
            ret = ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.CONFIG_QUALITY), (int)FullSceneQuality);
            if ( LiveMeshDisplayMode == ReconstructionDisplayMode.FULL_SCENE )
                LiveMeshMaterial.SetFloat("_PointSizeScaler", (FullSceneQuality == ReconstructionQuality.LOW)? 1.2f : 0.8f);
            result = result && (ret == (int)Error.WORK);

            //float size = 0.0f;
            //ViveSR_Framework.GetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.POINTCLOUD_POINTSIZE, ref size);
            //Debug.Log("point size:" + size);

            // switch shader for full scene point cloud
            if (LiveMeshMode != LastLiveMeshMode)
            {
                ViveSR_RigidReconstruction.SetLiveMeshExtractionMode(LiveMeshMode);
                LastLiveMeshMode = LiveMeshMode;
                if (LiveMeshMode == ReconstructionLiveMeshExtractMode.VERTEX_WITHOUT_NORMAL)
                    Shader.EnableKeyword("RENDER_AS_BILLBOARD");
                else
                    Shader.DisableKeyword("RENDER_AS_BILLBOARD");
            }

            // update live adaptive param
            if (LiveMeshDisplayMode == ReconstructionDisplayMode.ADAPTIVE_MESH)
            {
                ret = ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_MAX_GRID, ViveSR_RigidReconstruction.LiveAdaptiveMaxGridSize * 0.01f);   // cm to m
                result = result && (ret == (int)Error.WORK);
                ret = ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_MIN_GRID, ViveSR_RigidReconstruction.LiveAdaptiveMinGridSize * 0.01f);
                result = result && (ret == (int)Error.WORK);
                ret = ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_ERROR_THRES, ViveSR_RigidReconstruction.LiveAdaptiveErrorThres);
                result = result && (ret == (int)Error.WORK);
            }

            // refresh rate
            ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.MESH_REFRESH_INTERVAL, RefreshIntervalMS);
            return result;
        }

        public bool SetMeshDisplayMode(ReconstructionDisplayMode displayMode)
        {
            int result = (int)Error.FAILED;
            ReconstructionDisplayMode setMode = ReconstructionDisplayMode.FIELD_OF_VIEW;
            
            if (displayMode == ReconstructionDisplayMode.FIELD_OF_VIEW)
            {
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LITE_POINT_CLOUD_MODE), true);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.FULL_POINT_CLOUD_MODE), false);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LIVE_ADAPTIVE_MODE), false);
                LiveMeshMaterial.SetFloat("_PointSizeScaler", 1.2f);
                setMode = ReconstructionDisplayMode.FIELD_OF_VIEW;
                UsingMaterial = LiveMeshMaterial;
                ThreadPeriod = 15;
            }
            else if (displayMode == ReconstructionDisplayMode.FULL_SCENE)
            {
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LITE_POINT_CLOUD_MODE), false);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.FULL_POINT_CLOUD_MODE), true);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LIVE_ADAPTIVE_MODE), false);
                LiveMeshMaterial.SetFloat("_PointSizeScaler", (FullSceneQuality == ReconstructionQuality.LOW) ? 1.3f : 0.8f);
                setMode = ReconstructionDisplayMode.FULL_SCENE;
                UsingMaterial = LiveMeshMaterial;
                ThreadPeriod = 500;
            }
            else if (displayMode == ReconstructionDisplayMode.ADAPTIVE_MESH)
            {
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LITE_POINT_CLOUD_MODE), false);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.FULL_POINT_CLOUD_MODE), false);
                result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionParam.LIVE_ADAPTIVE_MODE), true);
                setMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
                UsingMaterial = WireframeMaterial;
                ThreadPeriod = 500;
            }
            for (int i = 0; i < ShowGameObjCount; ++i) ShowMeshRnds[i].sharedMaterial = UsingMaterial;
            if (result == (int)Error.WORK) { LiveMeshDisplayMode = setMode; }

            return (result == (int)Error.WORK);
        }
        

        // Use this for initialization
        void Start()
        {
            ViveSR_RigidReconstruction.AllocOutputDataMemory();

            // Init Shader Variant
            if (LiveMeshMode == ReconstructionLiveMeshExtractMode.VERTEX_WITHOUT_NORMAL)
                Shader.EnableKeyword("RENDER_AS_BILLBOARD");
            else
                Shader.DisableKeyword("RENDER_AS_BILLBOARD");

            LiveMeshesGroups = new GameObject("LiveMeshes");
            LiveMeshesGroups.transform.SetParent(gameObject.transform, false);
            for (int i = 0; i < ShowGameObjCount; ++i)
            {
                ShowGameObjs[i] = new GameObject("SubMesh_" + i);
                ShowGameObjs[i].transform.SetParent(LiveMeshesGroups.transform, false);

                ShowMeshRnds[i] = ShowGameObjs[i].AddComponent<MeshRenderer>();
                ShowMeshRnds[i].sharedMaterial = LiveMeshMaterial;

                ShowMeshFilters[i] = ShowGameObjs[i].AddComponent<MeshFilter>();
                ShowMeshFilters[i].mesh = new Mesh();
                ShowMeshFilters[i].mesh.MarkDynamic();

                ShowGameObjs[i].SetActive(false);
            }
            //int lod = ViveSR_RigidReconstruction.GetRigidReconstructionIntParameter((int)ReconstructionParam.POINT_CLOUD_LOD);
            //Debug.Log("[ViveSR] [RigidReconstruction] LOD:" + lod);

            LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
            SetMeshDisplayMode(LiveMeshDisplayMode);

            for (int i = 0; i < ShowGameObjCount; ++i)
            {
                MeshDataVertices[i] = new List<Vector3>();
                MeshDataIndices[i] = new List<int>();
                MeshDataColors[i] = new List<Color32>();
                MeshDataNormals[i] = new List<Vector3>();
            }

            MeshDataThread = new Thread(ExtractMeshDataThread);
            MeshDataThread.IsBackground = true;
            MeshDataThread.Start();
        }

        // Update is called once per frame
        void Update()
        {            
            if (ViveSR_RigidReconstruction.IsScanning)
            {
                // when exporting, don't update live extraction parameter
                if (!ViveSR_RigidReconstruction.IsExporting)
                    UpdateRuntimeParameter();

                if (IsMeshUpdate == true)
                    MeshDataCoroutine = StartCoroutine(RenderMeshDataIEnumerator());

                LastIsScanning = ViveSR_RigidReconstruction.IsScanning;
            }
            else if (LastIsScanning)
            {
                HideAllLiveMeshes();
                LastIsScanning = ViveSR_RigidReconstruction.IsScanning;
            }
            
            if (ViveSR_RigidReconstruction.IsExporting) { LastIsExporting = ViveSR_RigidReconstruction.IsExporting; }
            else if (LastIsExporting) { HideAllLiveMeshes(); LastIsExporting = ViveSR_RigidReconstruction.IsExporting; }
        }

        private void OnDisable()
        {
            IsThreadRunning = false;
            MeshDataThread.Join();
            MeshDataThread.Abort();
            if (IsCoroutineRunning == true)
            {
                StopCoroutine(MeshDataCoroutine);
                MeshDataCoroutine = null;
            }
            for (int i = 0; i < ShowGameObjCount; i++)
            {
                MeshDataVertices[i].Clear();
                MeshDataIndices[i].Clear();
                MeshDataColors[i].Clear();
                MeshDataNormals[i].Clear();
            }

            HideAllLiveMeshes();

        }
        private void OnApplicationQuit()
        {
            IsThreadRunning = false;
            MeshDataThread.Join();
            MeshDataThread.Abort();
            if (IsCoroutineRunning == true)
            {
                StopCoroutine(MeshDataCoroutine);
                MeshDataCoroutine = null;
            }
            for (int i = 0; i < ShowGameObjCount; i++)
            {
                MeshDataVertices[i].Clear();
                MeshDataIndices[i].Clear();
                MeshDataColors[i].Clear();
                MeshDataNormals[i].Clear();

            }
            HideAllLiveMeshes();
        }

        private static void HideAllLiveMeshes()
        {
            for (int i = 0; i < ShowGameObjCount; i++)
            {
                ShowGameObjs[i].SetActive(false);
            }
        }

        //IEnumerator//
        private IEnumerator RenderMeshDataIEnumerator()
        {
            IsCoroutineRunning = true;
            if (IsMeshUpdate == true)
            {
                for (int i = 0; i < NumSubMeshes; i++)
                {
                    ShowMeshFilters[i].sharedMesh.Clear();
                    ShowMeshFilters[i].sharedMesh.SetVertices(MeshDataVertices[i]);
                    ShowMeshFilters[i].sharedMesh.SetColors(MeshDataColors[i]);
                    ShowMeshFilters[i].sharedMesh.SetIndices(MeshDataIndices[i].ToArray(), (IndexNum > 0)? MeshTopology.Triangles : MeshTopology.Points, 0);
                    if(MeshDataNormals[i].Count >0)
                    {
                        ShowMeshFilters[i].sharedMesh.SetNormals(MeshDataNormals[i]);
                    }
                    ShowGameObjs[i].SetActive(true);
                }

                for (int i = NumSubMeshes; i < ShowGameObjCount; i++)
                {
                    ShowGameObjs[i].SetActive(false);
                }
                IsMeshUpdate = false;

            }
            IsCoroutineRunning = false;

            yield return 0;

        }
        private void ExtractMeshDataThread()
        {
            while (IsThreadRunning == true)
            {
                try
                {
                    if (IsMeshUpdate == false && ViveSR_RigidReconstruction.IsScanning == true)
                    {
                        bool result = ViveSR_RigidReconstruction.GetRigidReconstructionFrame(ref ProcessedFrame);
                        if (ProcessedFrame != LastProcessedFrame && result == true)
                        {
                            LastProcessedFrame = ProcessedFrame;
                            float[] _camPose;
                            result = ViveSR_RigidReconstruction.GetRigidReconstructionData(ref ProcessedFrame, out _camPose, ref VertexNum, out VertexData, ref VertStrideInFloat, ref IndexNum, out IndexData);
                            if (result == true )
                            {
                                LastProcessedFrame = ProcessedFrame;
                                if (LiveMeshDisplayMode != ReconstructionDisplayMode.ADAPTIVE_MESH)
                                    UpdatePointCloudDataList();
                                else if (IndexNum > 0)    
                                    UpdateMeshesDataList();
                            }
                        }
                        //else Debug.Log("Same Frame");
                    }
                }
                catch (System.Exception e)
                {
                    NumSubMeshes = 0;
                    Debug.LogWarning(e.Message);
                }

                Thread.Sleep(ThreadPeriod); //Avoid too fast get data from SR SDK DLL 
            }
        }

        private void UpdateMeshesDataList()
        {
            List<int> idMapping = Enumerable.Repeat(-1, VertexNum).ToList();

            int triNum = IndexNum / 3;
            int numSubVert = 0;
	        int numSubTri = 0;
            NumSubMeshes = 0;

            MeshDataVertices[NumSubMeshes].Clear();
            MeshDataIndices[NumSubMeshes].Clear();
            MeshDataColors[NumSubMeshes].Clear();
            MeshDataNormals[NumSubMeshes].Clear();

	        for (uint triID = 0; triID < triNum; ++triID)
	        {
		        // if this iteration will exceed the limitation, output to a new geometry first
                if ((numSubVert + 3) > 65000 || (numSubTri + 1) > 65000)
		        {
			        // clear the counter etc
                    idMapping = Enumerable.Repeat(-1, VertexNum).ToList();
                    ++NumSubMeshes;
                    MeshDataVertices[NumSubMeshes].Clear();
                    MeshDataIndices[NumSubMeshes].Clear();
                    MeshDataColors[NumSubMeshes].Clear();
                    MeshDataNormals[NumSubMeshes].Clear();
                    numSubVert = numSubTri = 0;
		        }

		        for (uint i = 0; i < 3; ++i)
		        {
			        // insert vertices and get new ID
                    int vertID = IndexData[triID * 3 + i];
                    //if (vertID >= VertexNum) 
                    //{
                    //    Debug.LogWarning("vertID:" + vertID + ", vert Num:" + VertexNum);    // a known bug, caught by exception
                    //    NumSubMeshes = 0; return;   
                    //}
			        if (idMapping[vertID] == -1)				// haven't added this vertex yet
			        {
                        idMapping[vertID] = MeshDataVertices[NumSubMeshes].Count;	// old ID -> new ID
                        float x = VertexData[vertID * VertStrideInFloat + 0];
                        float y = VertexData[vertID * VertStrideInFloat + 1];
                        float z = VertexData[vertID * VertStrideInFloat + 2];
                        MeshDataVertices[NumSubMeshes].Add(new Vector3(x, y, z));
				        ++numSubVert;
			        }
                    MeshDataIndices[NumSubMeshes].Add(idMapping[vertID]);
		        }
                ++numSubTri;
	        }

            ++NumSubMeshes;
            IsMeshUpdate = true;
        }

        private void UpdatePointCloudDataList()
        {
            NumSubMeshes = (int)Math.Ceiling((float)VertexNum / 65000);
            LastMeshes = (NumSubMeshes * 65000 == VertexNum) ? NumSubMeshes + 1 : NumSubMeshes;
            NumLastMeshVert = VertexNum - (65000 * (LastMeshes - 1));
            for (int i = 0; i < NumSubMeshes; i++)
            {
                int numVerts = (i == NumSubMeshes - 1) ? NumLastMeshVert : 65000;
                // vertStrideInFloat > 4 ==> has normal component
                UpdateSinglePointCloudData(i, numVerts, (VertStrideInFloat > 4));
            }

            IsMeshUpdate = true;
        }

        private void UpdateSinglePointCloudData(int meshID, int numVert, bool withNormal)
        {
            Vector3 vertexDst = new Vector3();
            Color32 colorDst = new Color32();
            Vector3 normalDst = new Vector3();

            MeshDataVertices[meshID].Clear();
            MeshDataIndices[meshID].Clear();
            MeshDataColors[meshID].Clear();
            MeshDataNormals[meshID].Clear();

            for (int i = 0; i < numVert; ++i)
            {
                int vertID = meshID * 65000 + i;
                int startOffset = vertID * VertStrideInFloat;
                float x = VertexData[startOffset + 0];
                float y = VertexData[startOffset + 1];
                float z = VertexData[startOffset + 2];
                vertexDst.Set(x, y, z);
                MeshDataVertices[meshID].Add(vertexDst);

                byte[] bits = BitConverter.GetBytes(VertexData[startOffset + 3]);
                colorDst.r = bits[0];
                colorDst.g = bits[1];
                colorDst.b = bits[2];
                colorDst.a = bits[3];
                MeshDataColors[meshID].Add(colorDst);

                if (withNormal)
                {
                    float norX = VertexData[startOffset + 4];
                    float norY = VertexData[startOffset + 5];
                    float norZ = VertexData[startOffset + 6];
                    normalDst.Set(norX, norY, norZ);
                    MeshDataNormals[meshID].Add(normalDst);
                }

                MeshDataIndices[meshID].Add(i);
            }
        }
    }
}
