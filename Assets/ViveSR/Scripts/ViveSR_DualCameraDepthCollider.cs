using UnityEngine;
using Vive.Plugin.SR;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

public class ViveSR_DualCameraDepthCollider : MonoBehaviour {

    private static bool _UpdateDepthCollider = false;
    private static bool _UpdateDepthColliderRange = false;
    private float[] VertexData;
    private int[] CldIdxData;
    private int NumCldVertData;
    private int NumCldIdxData;

    private const int ShowGameObjCount = 20;
    private int LastDepthColliderUpdateTime = 0;

    private static GameObject ColliderObjs;
    private static MeshFilter ColliderMeshes = new MeshFilter();
    private static MeshCollider MeshClds = new MeshCollider();
    private static MeshRenderer ColliderMeshRenderer;
    private static int QualityScale = 1;
    private static double _ColliderNearDistance = 0.2;
    private static double _ColliderFarDistance = 10.0;
    public static float UpdateColliderNearDistance
    {
        get { return (float)_ColliderNearDistance; }
        set { if (value != _ColliderNearDistance) _ColliderNearDistance = (float)value; SetDepthColliderNearDistance(_ColliderNearDistance); }
    }
    public static float UpdateColliderFarDistance
    {
        get { return (float)_ColliderFarDistance; }
        set { if (value != _ColliderFarDistance) _ColliderFarDistance = (float)value; SetDepthColliderFarDistance(_ColliderFarDistance); }
    }

    #region Multi-Thread Get Mesh Data
    // Multi-thread Parse Raw Data to Data List for Mesh object 
    private Thread MeshDataThread;
    private Coroutine MeshDataCoroutine = null; // IEnumerator for main thread Mesh
    private List<Vector3> MeshDataVertices = new List<Vector3>();
    private List<int> MeshDataIndices = new List<int>();
    private bool IsMeshUpdate = false;
    private bool IsCoroutineRunning = false;
    private bool IsThreadRunning = true;
    private static int ThreadPeriod = 10;
    #endregion

    public static bool UpdateDepthCollider
    {
        get { return _UpdateDepthCollider; }
        set { if (value != _UpdateDepthCollider) _UpdateDepthCollider = value; SetColliderProcessEnable(_UpdateDepthCollider); }
    }

    public static bool UpdateDepthColliderRange
    {
        get { return _UpdateDepthColliderRange; }
        set { if (value != _UpdateDepthColliderRange) _UpdateDepthColliderRange = value; SetColliderRangeEnable(_UpdateDepthColliderRange); }
    }

    public static bool ColliderMeshVisibility { get; private set; }
    public static Material ColliderDefaultMaterial {
        get
        {
            return new Material(Shader.Find("ViveSR/Wireframe"))
            {
                color = new Color(0.51f, 0.94f, 1.0f)
            };
        }
    }

    private void Start()
    {
        ColliderObjs = new GameObject("Depth Collider");
        ColliderObjs.transform.SetParent(gameObject.transform, false);

        ColliderMeshes = ColliderObjs.AddComponent<MeshFilter>();
        ColliderMeshes.mesh = new Mesh();
        ColliderMeshes.mesh.MarkDynamic();

        ChangeColliderMaterial(ColliderDefaultMaterial);

        MeshClds = ColliderObjs.AddComponent<MeshCollider>();
        SetLiveMeshVisibility(true);

        SetQualityScale(QualityScale);

        MeshDataThread = new Thread(ExtractMeshDataThread);
        MeshDataThread.IsBackground = true;
        MeshDataThread.Start();
    }

    public static bool ChangeColliderMaterial(Material mat)
    {
        if (ColliderObjs == null) return false;
        else if (ColliderMeshRenderer == null) ColliderMeshRenderer = ColliderObjs.AddComponent<MeshRenderer>();

        ColliderMeshRenderer.material = mat;
        return true;
    }

    private void Update()
    {
        if ((_UpdateDepthCollider))
        {
            if (IsMeshUpdate == true)
                MeshDataCoroutine = StartCoroutine(RenderMeshDataIEnumerator());

        }
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
      
        MeshDataVertices.Clear();
        MeshDataIndices.Clear();
        ColliderObjs.SetActive(false);

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
     
        MeshDataVertices.Clear();
        MeshDataIndices.Clear();
        ColliderObjs.SetActive(false);
    }

    public static bool SetColliderProcessEnable(bool value)
    {
        ViveSR_Framework.SetCommandBool(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthCmd.EXTRACT_DEPTH_MESH, value);
        
        SetLiveMeshVisibility(value);
        _UpdateDepthCollider = value;
          
        return true;
    }

    public static bool SetColliderRangeEnable(bool value)
    {
        ViveSR_Framework.SetCommandBool(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthCmd.ENABLE_SELECT_MESH_DISTANCE_RANGE, value);

        _UpdateDepthColliderRange = value;

        return true;
    }

    private void ExtractCurrentColliders()
    {
        ViveSR_DualCameraDepthExtra.GetDepthColliderData(ref NumCldVertData, out VertexData, ref NumCldIdxData, out CldIdxData);
        if (NumCldVertData != 0 && NumCldIdxData != 0)
        {
            GenerateMeshColliders();
        }
    }
    private void GenerateMeshColliders()
    {
        int numVert = NumCldVertData;
        int numIdx = NumCldIdxData;

        MeshDataVertices.Clear();
        MeshDataIndices.Clear();

        for (int i = 0; i < numVert; ++i)
        {
            float x = VertexData[i * 3 ];
            float y = VertexData[i * 3 + 1];
            float z = VertexData[i * 3 + 2];
            MeshDataVertices.Add(new Vector3(x,y,z));
        }

        for (int i = 0; i < numIdx; ++i)
            MeshDataIndices.Add(CldIdxData[i]);

        IsMeshUpdate = true;

    }

    public static bool SetLiveMeshVisibility(bool value)
    {
        if (ColliderMeshVisibility == value) return true;
        else if (ColliderMeshes == null || ColliderObjs == null) return false;
        ColliderMeshVisibility = value;
        if (!ColliderMeshVisibility) ColliderMeshes.sharedMesh.Clear();
        ColliderMeshRenderer.enabled = ColliderMeshVisibility;
        return true;
    }
    public static bool SetColliderEnable(bool value)
    {
        if (MeshClds == null) return false;
        MeshClds.enabled = value;
        return true;
    }


    public static bool GetQualityScale(out int value)
    {
        int result = ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.COLLIDER_QUALITY, ref QualityScale);
        if (result == (int)Error.WORK)
        {
            value = QualityScale;
            return true;
        }
        else
        {
            value = -1;
            return false;
        }
    }
    public static bool SetQualityScale(int value)
    {
        int result = ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.COLLIDER_QUALITY, value);
        if (result == (int)Error.WORK)
        {
            QualityScale = value;
            return true;
        }
        else
            return false;
    }

    public static bool SetDepthColliderNearDistance(double value)
    {
        int result = ViveSR_Framework.SetParameterDouble(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.MESH_NEAR_DISTANCE, value);
        if (result == (int)Error.WORK)
        {
            _ColliderNearDistance = value;
            return true;
        }
        else
            return false;
    }

    public static bool SetDepthColliderFarDistance(double value)
    {
        int result = ViveSR_Framework.SetParameterDouble(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.MESH_FAR_DISTANCE, value);
        if (result == (int)Error.WORK)
        {
            _ColliderFarDistance = value;
            return true;
        }
        else
            return false;
    }

    //IEnumerator//
    private IEnumerator RenderMeshDataIEnumerator()
    {
        IsCoroutineRunning = true;
        if (IsMeshUpdate == true)
        {
            ColliderMeshes.sharedMesh.Clear();
            ColliderMeshes.sharedMesh.SetVertices(MeshDataVertices);
            ColliderMeshes.sharedMesh.SetIndices(MeshDataIndices.ToArray(), MeshTopology.Triangles, 0);
            MeshClds.sharedMesh = ColliderMeshes.sharedMesh;

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
                if (IsMeshUpdate == false && _UpdateDepthCollider == true)
                {
                    ViveSR_DualCameraDepthExtra.GetDepthColliderFrameInfo();
                    int currentDepthColliderTimeIndex = ViveSR_DualCameraDepthExtra.DepthColliderTimeIndex;
                    if (currentDepthColliderTimeIndex != LastDepthColliderUpdateTime)
                    {
                        ExtractCurrentColliders();
                        LastDepthColliderUpdateTime = currentDepthColliderTimeIndex;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            Thread.Sleep(ThreadPeriod); //Avoid too fast get data from SR SDK DLL 
        }
    }
}
