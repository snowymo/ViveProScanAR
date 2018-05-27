using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class PLYPathLoader : MonoBehaviour {

    public class PLYObj
    {
        public Vector3[] originalVertices;
        public Color32[] origianlColors;
        public int[] originalIndices;
        public Vector2[] originalUVs;
    }

    public Matrix4x4 originalMatrix;

    public client zmqMeshClient, zmqMatrixClient;

    public Transform steamTracker;

    List<GameObject> gos;
    List<PLYObj> plyObjs;
    //List<GameObject> pointGOs;

    public GameObject meshPrefab;

    //Mesh mesh;

    public Shader shader;
    public Shader textureShader;

    float vivescale = 0.001f;//1f;//0.001f;

    public enum PLY_COORD { VIVE, TRACKER};

    public PLY_COORD plyCoordType;

    public string davidPLYFileName;

    // Use this for initialization
    void Start () {

        
    }
	
	// Update is called once per frame
	void Update () {
        // update based on tracker's matrix

        //Vector3[] normals = mesh.normals;
        initLists();

        if (steamTracker.gameObject.GetComponent<SteamVR_TrackedObject>().isValid)
        {
            Matrix4x4 curTracker = Matrix4x4.TRS(steamTracker.position, steamTracker.rotation, Vector3.one);
            for(int plyObji = 0; plyObji < plyObjs.Count; plyObji++)
            {
                Mesh mesh = gos[plyObji].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                //print("originialVertices:" + originialVertices[0]);
                int i = 0;
                while (i < Mathf.Min(65535, vertices.Length))
                {
                    Vector3 VviveScale = plyObjs[plyObji].originalVertices[i] * vivescale;
                    // looks like points don't need that?
                    //if (zmqMeshClient.msgType == client.MsgType.MESHES)
                        VviveScale.z = -VviveScale.z;
                    if (plyCoordType == PLY_COORD.VIVE)
                        vertices[i] = (curTracker * originalMatrix.inverse).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TRACKER)
                        vertices[i] = (curTracker * originalMatrix).MultiplyPoint(VviveScale);
                    i++;
                }
                print("after :" + vertices[0]);
                mesh.vertices = vertices;
                //mesh.RecalculateNormals();
            }
        }
        
    }

    public void LoadMesh()
    {
        PLYObj plyObj = new PLYObj();

        // read from file
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(zmqMeshClient.meshPath);

        plyObj.originalVertices = PlyLoaderDll.GetVertices(plyIntPtr);
        plyObj.origianlColors = PlyLoaderDll.GetColors(plyIntPtr);
        plyObj.originalIndices = PlyLoaderDll.GetIndexs(plyIntPtr);

        print("LoadMesh ing:" + plyObj.originalVertices.Length + " vertices and " + plyObj.originalIndices.Length + " faces");

        PlyLoaderDll.UnLoadPly(plyIntPtr);

        Mesh mesh = new Mesh();
        // assign to mesh
        if (plyObj.originalVertices != null)
            if (plyObj.originalVertices.Length > 65000)
            {
                Vector3[] tempVertices = new Vector3[65000];
                Array.Copy(plyObj.originalVertices, tempVertices, 65000);
                mesh.vertices = tempVertices;
            }
            else
                mesh.vertices = plyObj.originalVertices;
        if (plyObj.origianlColors != null)
            if (plyObj.origianlColors.Length > 65000)
            {
                Color32[] tempColors = new Color32[65000];
                Array.Copy(plyObj.origianlColors, tempColors, 65000);
                mesh.colors32 = tempColors;
            }
            else
                mesh.colors32 = plyObj.origianlColors;
        if (plyObj.originalIndices != null)
        {
            if(plyObj.originalIndices.Length > 65000*3)
            {
                int[] tempIndices = new int[65000 * 3];
                Array.Copy(plyObj.originalIndices, tempIndices, 65000 * 3);
                mesh.SetIndices(tempIndices, MeshTopology.Triangles, 0, true);
            }
            else
                mesh.SetIndices(plyObj.originalIndices, MeshTopology.Triangles, 0, true);
        }
            
        mesh.name = "mesh";
        //mesh.RecalculateNormals();

        // assign mesh to object itself
        transform.gameObject.name = zmqMeshClient.meshPath.Substring(zmqMeshClient.meshPath.Length - 14, 14);
        MeshFilter mf = transform.gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = transform.gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);

        print("LoadMesh ing:" + plyObj.originalVertices.Length + " vertices and " + plyObj.originalIndices.Length + " faces");
    }

    void createMesh(int startIdx, int verticeCnt, ref Vector3[] vertex, ref Vector2[] uv, ref Texture2D texture)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, startIdx * Utility.limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Vector2[] curUV = new Vector2[verticeCnt];
        Array.Copy(uv, startIdx * Utility.limitCount, curUV, 0, verticeCnt);
        mesh.uv = curUV;

        //         for (int i = 0; i < verticeCnt; i++)
        //         {
        //             mesh.vertices[i] /= 1000f;       
        //         }
        if (Utility.indices.Length > verticeCnt)
        {
            int[] subindices = new int[verticeCnt];
            Array.Copy(Utility.indices, subindices, verticeCnt);
            mesh.SetIndices(subindices, MeshTopology.Points, 0);
        }
        else
            mesh.SetIndices(Utility.indices, MeshTopology.Points, 0);

        PLYObj plyObj = new PLYObj();
        plyObj.originalVertices = curV;
        plyObj.originalUVs = curUV;
        plyObjs.Add(plyObj);
        print("LoadPoint ing:" + plyObj.originalVertices[0] + " pos");

        mesh.name = "mesh" + startIdx.ToString();

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = transform;
        gos.Add(go);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(textureShader);
        mr.material.mainTexture = texture;
    }

    void createMesh(int startIdx, int verticeCnt, ref Vector3[] vertex, ref Color32[] color)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, startIdx * Utility.limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Color32[] curC = new Color32[verticeCnt];
        Array.Copy(color, startIdx * Utility.limitCount, curC, 0, verticeCnt);
        mesh.colors32 = curC;

        if (Utility.indices.Length > verticeCnt)
        {
            int[] subindices = new int[verticeCnt];
            Array.Copy(Utility.indices, subindices, verticeCnt);
            mesh.SetIndices(subindices, MeshTopology.Points, 0);
        }
        else
            mesh.SetIndices(Utility.indices, MeshTopology.Points, 0);
        mesh.name = "mesh" + startIdx.ToString();

        PLYObj plyObj = new PLYObj();

        plyObj.originalVertices = curV;
        plyObj.origianlColors = curC;

        plyObjs.Add(plyObj);

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = transform;
        gos.Add(go);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    void initLists()
    {
        if (gos == null)
            gos = new List<GameObject>();
        if (plyObjs == null)
            plyObjs = new List<PLYObj>();
    }

    public void LoadMeshes()
    {
        initLists();

        // if we load from David, then load as point cloud
        if (zmqMeshClient.msgType == client.MsgType.POINTS)
        {
            // new version, we load from rply dll, so that we have color rather than texture
            string pathname = zmqMeshClient.pcPath;
            IntPtr plyIntPtr = PlyLoaderDll.LoadPly(pathname);

            if (plyIntPtr == null)
                return;

            Mesh mesh = new Mesh();
            Vector3[] vertices = PlyLoaderDll.GetRVertices(plyIntPtr);
            Color32[] colors = PlyLoaderDll.GetRColors(plyIntPtr);
            //int[] indices = PlyLoaderDll.GetRIndexs(plyIntPtr);
            PlyLoaderDll.UnLoadPly(plyIntPtr);

            int meshCount = vertices.Length / Utility.limitCount + 1;
            for (int i = 0; i < meshCount; i++)
            {
                createMesh(i, Math.Min(Utility.limitCount, vertices.Length - i * Utility.limitCount), ref vertices, ref colors);
            }
        }
        // else if we load from OSR, then load as multiply vertices and colors pairs
        else if (zmqMeshClient.msgType == client.MsgType.MESHES)
        {
            for (int i = 0; i < zmqMeshClient.meshPaths.Length; i++)
            {
                // read from file
                IntPtr plyIntPtr = PlyLoaderDll.LoadPly(zmqMeshClient.meshPaths[i]);

                PLYObj plyObj = new PLYObj();

                plyObj.originalVertices = PlyLoaderDll.GetVertices(plyIntPtr);
                plyObj.origianlColors = PlyLoaderDll.GetColors(plyIntPtr);
                plyObj.originalIndices = PlyLoaderDll.GetIndexs(plyIntPtr);

                plyObjs.Add(plyObj);

                print("LoadMesh ing:" + plyObj.originalVertices[0] + " vertices and " + plyObj.originalIndices[0] + " faces");

                PlyLoaderDll.UnLoadPly(plyIntPtr);

                // create gameobject and mesh to assign
                GameObject go = Instantiate<GameObject>(meshPrefab, transform);
                gos.Add(go);

                Mesh mesh = new Mesh();
                // assign to mesh
                if (plyObj.originalVertices != null)
                    mesh.vertices = plyObj.originalVertices;
                if (plyObj.origianlColors != null)
                    mesh.colors32 = plyObj.origianlColors;
                if (plyObj.originalIndices != null)
                    mesh.SetIndices(plyObj.originalIndices, MeshTopology.Triangles, 0, true);

                mesh.name = "mesh";
                //mesh.RecalculateNormals();

                // assign mesh to object itself
                go.name = zmqMeshClient.meshPaths[i].Substring(zmqMeshClient.meshPaths[i].Length - 14, 14);
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = new Material(shader);

                print("LoadMesh ing:" + plyObj.originalVertices.Length + " vertices and " + plyObj.originalIndices.Length + " faces");
            }
        }
        
        
    }

    public void LoadMatrix()
    {
        if (zmqMatrixClient.fm.Length == 0)
            return;

        for(int i = 0; i < 16; i++)
        {
            originalMatrix[i % 4, i / 4] = zmqMatrixClient.fm[i];
        }
        // from david scale to unity scale
        for (int i = 0; i < 3; i++)
        {

            originalMatrix[i, 3] /= 1000f;
        }
        // right hand to left hand
        originalMatrix[0, 2] = -originalMatrix[0, 2];
        originalMatrix[1, 2] = -originalMatrix[1, 2];
        originalMatrix[2, 0] = -originalMatrix[2, 0];
        originalMatrix[2, 1] = -originalMatrix[2, 1];
        originalMatrix[2, 3] = -originalMatrix[2, 3];

        print(originalMatrix.ToString());
    }
}
