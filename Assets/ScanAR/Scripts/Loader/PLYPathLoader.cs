﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;
using System.IO;

public class PLYPathLoader : MonoBehaviour {

    public class PLYObj
    {
        public Vector3[] originalVertices;
        public Color32[] origianlColors;
        public int[] originalIndices;
        public Vector2[] originalUVs;
    }

    public Vector3[] rawScanVertices;
    public Color32[] rawScanColors;
    public PlyLoaderDll.LABCOLOR[] rawScanLabColors;
    public uint[] rawScanFaces;

    public Matrix4x4 originalMatrix, originalSCtoDMatrix, originalSTtoDMatrix;

    public client zmqMeshClient, zmqMatrixClient;

    public Transform steamTracker, scanTracker;
    public Transform secondaryController,scanController;
    public Matrix4x4 initialSecController, initialSecTracker;

    List<GameObject> gos, integratedGOs;
    List<PLYObj> plyObjs, integratedObjs;
    //List<GameObject> pointGOs;

    public GameObject meshPrefab;

    //Mesh mesh;

    public Shader shader;
    public Shader textureShader;

    float vivescale = 0.001f;//1f;//0.001f;

    public enum PLY_COORD { VIVE, TRACKER, TEST, BOTH};

    public PLY_COORD plyCoordType;

    // Use this for initialization
    void Start () {

        
    }
	
	// Update is called once per frame
	void Update () {
        // update based on tracker's matrix

        //Vector3[] normals = mesh.normals;
        initLists();

        updateMeshEveryFrame();

        //updateMeshDUO();
        
    }

    void updateMeshDUO()
    {
        if (steamTracker.gameObject.GetComponent<SteamVR_TrackedObject>().isValid)
        {
            Matrix4x4 curTracker = Matrix4x4.TRS(steamTracker.position, steamTracker.rotation, Vector3.one);
            Matrix4x4 curSecController = Matrix4x4.TRS(secondaryController.position, secondaryController.rotation, Vector3.one);

            Matrix4x4 curScanController = Matrix4x4.TRS(scanController.position, scanController.rotation, Vector3.one);
            Matrix4x4 curScanTracker = Matrix4x4.TRS(scanTracker.position, scanTracker.rotation, Vector3.one);
            for (int plyObji = 0; plyObji < plyObjs.Count; plyObji++)
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
                        vertices[i] = (curTracker * initialSecTracker.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TEST)
                        vertices[i] = (curSecController * initialSecController.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.BOTH)
                    {
                        // calculate average
                        Vector3 scVertex = (curTracker * initialSecTracker.inverse * curScanController * originalSCtoDMatrix).MultiplyPoint(VviveScale);
                        Vector3 stVertex = (curTracker * initialSecTracker.inverse * curScanTracker * originalSTtoDMatrix).MultiplyPoint(VviveScale);
                        //vertices[i] = (scVertex + stVertex) / 2f;
                        if (plyObji < plyObjs.Count / 2)
                            vertices[i] = scVertex;
                        else
                            vertices[i] = stVertex;
                        if (i == 0)
                        {
                            // calculate the diff
                            Vector3 vC = (curScanController * originalSCtoDMatrix).MultiplyPoint(VviveScale);
                            Vector3 vT = (curScanTracker * originalSTtoDMatrix).MultiplyPoint(VviveScale);
                            print("diff:" + (vC - vT).ToString("F4"));
                        }
                    }

                    i++;
                }
                //print("after :" + vertices[0]);
                mesh.vertices = vertices;
                //mesh.RecalculateNormals();
            }
        }

    }

    void updateMeshEveryFrame()
    {
        if (steamTracker != null && steamTracker.gameObject.GetComponent<SteamVR_TrackedObject>().isValid)
        {
            Matrix4x4 curTracker = Matrix4x4.TRS(steamTracker.position, steamTracker.rotation, Vector3.one);
            Matrix4x4 curSecController = Matrix4x4.TRS(secondaryController.position, secondaryController.rotation, Vector3.one);

            Matrix4x4 curScanController = Matrix4x4.TRS(scanController.position, scanController.rotation, Vector3.one);
            Matrix4x4 curScanTracker = Matrix4x4.TRS(scanTracker.position, scanTracker.rotation, Vector3.one);
            for (int plyObji = 0; plyObji < plyObjs.Count; plyObji++)
            {
                Mesh mesh = gos[plyObji].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                //print("originialVertices:" + originialVertices[0]);
                int i = 0;
                while (i < Mathf.Min(65000, vertices.Length))
                {
                    Vector3 VviveScale = plyObjs[plyObji].originalVertices[i] * vivescale;
                    // looks like points don't need that?
                    //if (zmqMeshClient.msgType == client.MsgType.MESHES)
                    VviveScale.z = -VviveScale.z;
                    if (plyCoordType == PLY_COORD.VIVE)
                        vertices[i] = (curTracker * originalMatrix.inverse).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TRACKER)
                        vertices[i] = (curTracker * initialSecTracker.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TEST)
                        vertices[i] = (curSecController * initialSecController.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.BOTH)
                    {
                        // calculate average
                        Vector3 scVertex = (curTracker * initialSecTracker.inverse * curScanController * originalSCtoDMatrix).MultiplyPoint(VviveScale);
                        Vector3 stVertex = (curTracker * initialSecTracker.inverse * curScanTracker * originalSTtoDMatrix).MultiplyPoint(VviveScale);
                        vertices[i] = stVertex;// (scVertex + stVertex) / 2f;
                        if (i == 0)
                        {
                            // calculate the diff
                            Vector3 vC = (curScanController * originalSCtoDMatrix).MultiplyPoint(VviveScale);
                            Vector3 vT = (curScanTracker * originalSTtoDMatrix).MultiplyPoint(VviveScale);
                            //print("diff:" + (vC - vT).ToString("F4"));
                        }
                    }

                    i++;
                }
                //print("after :" + vertices[0]);
                mesh.vertices = vertices;
                //mesh.RecalculateNormals();
            }

            for (int integratedIdx = 0; integratedIdx < integratedObjs.Count; integratedIdx++)
            {
                Mesh mesh = integratedGOs[integratedIdx].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                //print("originialVertices:" + originialVertices[0]);
                int i = 0;
                while (i < Mathf.Min(65535, vertices.Length))
                {
                    Vector3 VviveScale = integratedObjs[integratedIdx].originalVertices[i];
                    if (plyCoordType == PLY_COORD.VIVE)
                        vertices[i] = (curTracker * originalMatrix.inverse).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TRACKER)
                        vertices[i] = (curTracker * initialSecTracker.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.TEST)
                        vertices[i] = (curSecController * initialSecController.inverse * originalMatrix).MultiplyPoint(VviveScale);
                    else if (plyCoordType == PLY_COORD.BOTH)
                    {
                        // calculate average
                        Vector3 scVertex = (curTracker * initialSecTracker.inverse).MultiplyPoint(VviveScale);
                        Vector3 stVertex = (curTracker * initialSecTracker.inverse).MultiplyPoint(VviveScale);
                        vertices[i] = (scVertex + stVertex) / 2f;
                        if (i == 0)
                        {
                            // calculate the diff
                            Vector3 vC = (curScanController * originalSCtoDMatrix).MultiplyPoint(VviveScale);
                            Vector3 vT = (curScanTracker * originalSTtoDMatrix).MultiplyPoint(VviveScale);
                            //print("diff:" + (vC - vT).ToString("F4"));
                        }
                    }

                    i++;
                }
                //print("after :" + vertices[0]);
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

    void createMesh(int verticeCnt, ref Vector3[] vertex, ref Color32[] color, int faceCnt, ref int[] faces)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, 0, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Color32[] curC = new Color32[verticeCnt];
        Array.Copy(color, 0, curC, 0, verticeCnt);
        mesh.colors32 = curC;

        int[] curF = new int[faceCnt];
        Array.Copy(faces, 0, curF, 0, faceCnt);
        mesh.SetIndices(curF, MeshTopology.Triangles, 0);
        mesh.name = "mesh";

        PLYObj plyObj = new PLYObj();

        plyObj.originalVertices = curV;
        plyObj.origianlColors = curC;
        plyObj.originalIndices = curF;

        plyObjs.Add(plyObj);

        GameObject go = new GameObject("go");
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
        if (integratedGOs == null)
            integratedGOs = new List<GameObject>();
        if (integratedObjs == null)
            integratedObjs = new List<PLYObj>();
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

    public void UpdateIntegratedMesh(ref Vector3[] vertices, ref Color32[] colors, ref uint[] faces)
    {
        // get rid of the old ones
        for(int i = 0; i < gos.Count; i++)
        {
            gos[i].SetActive(false);
        }
        // clear plyobjs and create new because different scale
        plyObjs = new List<PLYObj>();
        gos = new List<GameObject>();

        // maybe there is only one integrated result pair at all, check later

        // keep the raw vars the same for now
        // I have to split the mesh in c++ side and get a list of (vert,color,face), right now let's assume there is only one

        PLYObj plyObj = new PLYObj();

        plyObj.originalVertices = new Vector3[vertices.Length];
        Array.Copy(vertices, plyObj.originalVertices, vertices.Length);
        plyObj.origianlColors = new Color32[colors.Length];
        Array.Copy(colors, plyObj.origianlColors, colors.Length);
        plyObj.originalIndices = new int[faces.Length];
        /*Array.Copy(plyObj.originalIndices, faces, faces.Length);*/
        for (int i = 0; i < faces.Length; i++)
            plyObj.originalIndices[i] = Convert.ToInt32(faces[i]);

        plyObjs.Add(plyObj);

        print("LoadMesh ing:" + plyObj.origianlColors[0] + " color and " + plyObj.originalIndices[0] + " faces");

        // create gameobject and mesh to assign
        GameObject go = Instantiate<GameObject>(meshPrefab, transform);
        gos.Add(go);

        Mesh mesh = new Mesh();
        // assign to mesh
        if (plyObj.originalVertices.Length > 65000)
        {
            Vector3[] tempVertices = new Vector3[65000];
            Array.Copy(plyObj.originalVertices, tempVertices, 65000);
            mesh.vertices = tempVertices;
        }
        else
            mesh.vertices = plyObj.originalVertices;
        if (plyObj.origianlColors.Length > 65000)
        {
            Color32[] tempColors = new Color32[65000];
            Array.Copy(plyObj.origianlColors, tempColors, 65000);
            mesh.colors32 = tempColors;
        }
        else
            mesh.colors32 = plyObj.origianlColors;

        if (plyObj.originalIndices.Length > 65000*3)
        {
            int[] tempFaces = new int[65000*3];
            Array.Copy(plyObj.originalIndices, tempFaces, 65000*3);
            mesh.SetIndices(tempFaces, MeshTopology.Triangles, 0, true);
        }
        else
            mesh.SetIndices(plyObj.originalIndices, MeshTopology.Triangles, 0, true);

        mesh.name = "mesh";
        //mesh.RecalculateNormals();

        // assign mesh to object itself
        go.name = "meshObj" + gos.Count.ToString();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    public void LoadMeshesDirectly()
    {
        initLists();

        // if we load from David, then load as point cloud
        // new version, we load from rply dll, so that we have color rather than texture
        print("LoadMeshesDirectly()");
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(Utility.scanPath);

        if (plyIntPtr == null)
            return;

        Mesh mesh = new Mesh();
        rawScanVertices = PlyLoaderDll.GetRVertices(plyIntPtr);
        rawScanColors = PlyLoaderDll.GetRColors(plyIntPtr);
        rawScanLabColors = PlyLoaderDll.GetRColorsLAB(plyIntPtr);
        rawScanFaces = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);

        int meshCount = rawScanVertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount), ref rawScanVertices, ref rawScanColors);
        }
        // test
        print("rawScanColors 0 and 100 " + rawScanColors[0].ToString("F3") + "\t" + rawScanColors[100].ToString("F3"));
    }

    public void LoadMeshesDUO()
    {
        initLists();

        // if we load from David, then load as point cloud
        // new version, we load from rply dll, so that we have color rather than texture
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(Utility.scanPath);

        if (plyIntPtr == null)
            return;

        Mesh mesh = new Mesh();
        rawScanVertices = PlyLoaderDll.GetRVertices(plyIntPtr);
        rawScanColors = PlyLoaderDll.GetRColors(plyIntPtr);
        rawScanLabColors = PlyLoaderDll.GetRColorsLAB(plyIntPtr);
        rawScanFaces = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);

        int meshCount = rawScanVertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount), ref rawScanVertices, ref rawScanColors);
        }
        // dupilicate the meshes for sc and st tests
        for (int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount), ref rawScanVertices, ref rawScanColors);
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

    // load transformVtoD from *.aln
    // then apply tracker.inverse to save into originalMatrix, which right now is transformTtoD
    public void LoadMatrixDirectly()
    {
        // load from Utility.calibrationFilePath
        StreamReader reader = new StreamReader(Utility.calibrationFilePath);
        string line;
        // Read and display lines from the file until the end of the file is reached.
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine(line);
            if (line.Contains(Utility.calibrationIndicator))
            {
                line = reader.ReadLine();//#
                for(int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    line = reader.ReadLine(); // first line
                    string[] firstRow = line.Split(new string[] { " " }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < firstRow.Length; colIdx++)
                        originalMatrix[rowIdx,colIdx] = float.Parse(firstRow[colIdx]);
                } 
            }else if (line.Contains(Utility.calibScanControllerIndicator))
            {
                line = reader.ReadLine();//#
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    line = reader.ReadLine(); // first line
                    string[] firstRow = line.Split(new string[] { " " }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < firstRow.Length; colIdx++)
                        originalSCtoDMatrix[rowIdx, colIdx] = float.Parse(firstRow[colIdx]);
                }
            }
            else if (line.Contains(Utility.calibScanTrackerIndicator))
            {
                line = reader.ReadLine();//#
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    line = reader.ReadLine(); // first line
                    string[] firstRow = line.Split(new string[] { " " }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < firstRow.Length; colIdx++)
                        originalSTtoDMatrix[rowIdx, colIdx] = float.Parse(firstRow[colIdx]);
                }
            }
        }
        reader.Close();

        // from david scale to unity scale
        for (int i = 0; i < 3; i++)
        {

            originalMatrix[i, 3] /= 1000f;
            originalSCtoDMatrix[i, 3] /= 1000f;
            originalSTtoDMatrix[i, 3] /= 1000f;
        }
        // right hand to left hand
        originalMatrix[0, 2] *= -1f;
        originalMatrix[1, 2] *= -1f;
        originalMatrix[2, 0] *= -1f;
        originalMatrix[2, 1] *= -1f;
        originalMatrix[2, 3] *= -1f;

        originalSCtoDMatrix[0, 2] *= -1f;
        originalSCtoDMatrix[1, 2] *= -1f;
        originalSCtoDMatrix[2, 0] *= -1f;
        originalSCtoDMatrix[2, 1] *= -1f;
        originalSCtoDMatrix[2, 3] *= -1f;

        originalSTtoDMatrix[0, 2] *= -1f;
        originalSTtoDMatrix[1, 2] *= -1f;
        originalSTtoDMatrix[2, 0] *= -1f;
        originalSTtoDMatrix[2, 1] *= -1f;
        originalSTtoDMatrix[2, 3] *= -1f;

        if(secondaryController)
            initialSecController = Matrix4x4.TRS(secondaryController.position, secondaryController.rotation, Vector3.one);
        if(steamTracker)
            initialSecTracker = Matrix4x4.TRS(steamTracker.position, steamTracker.rotation, Vector3.one);
        //print(originalMatrix.ToString());
    }
}
