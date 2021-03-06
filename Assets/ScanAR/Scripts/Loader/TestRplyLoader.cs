﻿using System;
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

    public List<GameObject> gos = new List<GameObject>();

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
        if (Utility.indices.Length > 65000)
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

    UpdateCtrl curController;

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
    public PlyLoaderDll.LABCOLOR[] rawScanLabColors;
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
        rawScanLabColors = PlyLoaderDll.GetRColorsLAB(plyIntPtr);
        rawScanFaces = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);

        // pass to UpdateCtrl.ScanARData to do
        if(curController == null)
            curController = GetComponent<UpdateCtrl>();
        curController.AddDavidData(rawScanVertices, rawScanColors, rawScanLabColors, rawScanFaces);
        

    }

    public void createMesh(int verticeCnt, ref Vector3[] vertex, ref Color32[] color, int faceCnt, ref uint[] faces)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, 0, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Color32[] curC = new Color32[verticeCnt];
        Array.Copy(color, 0, curC, 0, verticeCnt);
        mesh.colors32 = curC;

//         int[] curF = new int[faceCnt];
//         Array.Copy(faces, 0, curF, 0, faceCnt);
//         mesh.SetIndices(curF, MeshTopology.Triangles, 0);

        int[] ifaces = new int[faceCnt];
        /*Array.Copy(plyObj.originalIndices, faces, faces.Length);*/
        for (int i = 0; i < faces.Length; i++)
            ifaces[i] = Convert.ToInt32(faces[i]);
        if (faceCnt > 65000 * 3)
        {
            int[] tempFaces = new int[65000 * 3];
            Array.Copy(ifaces, tempFaces, 65000 * 3);
            mesh.SetIndices(tempFaces, MeshTopology.Triangles, 0, true);
        }
        else
            mesh.SetIndices(ifaces, MeshTopology.Triangles, 0, true);

        mesh.name = "mesh";

        GameObject go = new GameObject("go");
        go.transform.parent = transform;
        gos.Add(go);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    

    // Use this for initialization
    void Awake () {
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

        if (curController == null)
            curController = GetComponent<UpdateCtrl>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
