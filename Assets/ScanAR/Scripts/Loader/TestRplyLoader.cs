using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRplyLoader : MonoBehaviour {
    public string fileName;
    public bool isAbsolute;
    public string absFilePath;

    GameObject go, go2;

    public Shader shader;

    int[] indices;

    List<GameObject> gos = new List<GameObject>();

    float curTime, prevTime;

    public int limitCount;

    public void createMesh(int startIdx, int verticeCnt, ref Vector3[] vertex, ref Color32[] color, int faceCnt, ref uint[] faces)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, startIdx * limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        
        if (color != null) {
            Color32[] curC = new Color32[verticeCnt];
            Array.Copy(color, startIdx * limitCount, curC, 0, verticeCnt);
            mesh.colors32 = curC;
        }
        //         int[] ifaces = new int[faces.Length];
        //         for(int i = 0; i < faces.Length; i++)
        //         {
        //             ifaces[i] = Convert.ToInt32( faces[i]);
        //         }
        //         if(faces != null)
        //         {
        //             mesh.SetIndices(ifaces, MeshTopology.Triangles, 0);
        //         }
        //         else
        //         {
        Utility.InitialIndices();
        if (Utility.indices.Length > verticeCnt)
        {
            int[] subindices = new int[verticeCnt];
            Array.Copy(Utility.indices, subindices, verticeCnt);
            mesh.SetIndices(subindices, MeshTopology.Points, 0);
        }
        else
            mesh.SetIndices(Utility.indices, MeshTopology.Points, 0);
        /*        }*/

        mesh.name = "mesh" + startIdx.ToString();

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = transform;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }


    void loadPLYDownSample()
    {
        if (!isAbsolute)
            absFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        //zhenyi
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(absFilePath);
        
        Mesh mesh = new Mesh();
        Vector3[] vertices = PlyLoaderDll.GetRVertices(plyIntPtr);
        Color32[] colors = PlyLoaderDll.GetRColors(plyIntPtr);
        uint[] indices = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);

        int meshCount = vertices.Length / limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(limitCount, vertices.Length - i * limitCount), ref vertices, ref colors, indices.Length, ref indices);
        }
        
        
    }
    public Vector3[] rawScanVertices;
    public Color32[] rawScanColors;
    public uint[] rawScanFaces;
    public void LoadMeshesDirectly()
    {
        print("LoadMeshesDirectly()");
        if (!isAbsolute)
            absFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(absFilePath);

        if (plyIntPtr == null)
            return;

        Mesh mesh = new Mesh();
        rawScanVertices = PlyLoaderDll.GetRVertices(plyIntPtr);
        rawScanColors = PlyLoaderDll.GetRColors(plyIntPtr);
        rawScanFaces = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);

        int meshCount = rawScanVertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount), ref rawScanVertices, ref rawScanColors);
        }

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

//         PLYObj plyObj = new PLYObj();
// 
//         plyObj.originalVertices = curV;
//         plyObj.origianlColors = curC;
// 
//         plyObjs.Add(plyObj);

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = transform;
        gos.Add(go);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    // Use this for initialization
    void Start () {
        //         indices = new int[limitCount];
        //         for (int i = 0; i < limitCount; i++)
        //         {
        //             indices[i] = i;
        //         }
        Utility.InitialIndices();
        prevTime = Time.realtimeSinceStartup;
        /*loadPLYDownSample();*/
        LoadMeshesDirectly();
        curTime = Time.realtimeSinceStartup;
        print("whole took " + (curTime - prevTime) + "s");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
