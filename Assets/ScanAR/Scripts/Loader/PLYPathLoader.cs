using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class PLYPathLoader : MonoBehaviour {

    public client zmqMeshClient, zmqMatrixClient;

    public Transform steamTracker;

    Mesh mesh;

    Vector3[] originialVertices;
    Color32[] origianlColors;
    int[] originalIndices;
    Matrix4x4 originalMatrix;

    // Use this for initialization
    void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
        // update based on tracker's matrix
        mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        int i = 0;
        Matrix4x4 curTracker = Matrix4x4.TRS(steamTracker.position, steamTracker.rotation, Vector3.one);
        //print("originialVertices:" + originialVertices[0]);
        while (i < vertices.Length)
        {
            Vector3 VviveScale = originialVertices[i] * 0.001f;
            VviveScale.z = -VviveScale.z;
            vertices[i] = (curTracker * originalMatrix.inverse).MultiplyPoint(VviveScale);
            i++;
        }
        //print("after diff:" + vertices[0]);
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    public void LoadMesh()
    {
        // read from file
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(zmqMeshClient.meshPath);

        originialVertices = PlyLoaderDll.GetVertices(plyIntPtr);
        origianlColors = PlyLoaderDll.GetColors(plyIntPtr);
        originalIndices = PlyLoaderDll.GetIndexs(plyIntPtr);

        PlyLoaderDll.UnLoadPly(plyIntPtr);

        mesh = new Mesh();
        // assign to mesh
        if (originialVertices != null)
            mesh.vertices = originialVertices;
        if (origianlColors != null)
            mesh.colors32 = origianlColors;
        if (originalIndices != null)
            mesh.SetIndices(originalIndices, MeshTopology.Triangles, 0, true);
        mesh.name = "mesh";
        //mesh.RecalculateNormals();

        // assign mesh to object itself
        transform.gameObject.name = zmqMeshClient.meshPath.Substring(zmqMeshClient.meshPath.Length - 14, 14);
        MeshFilter mf = transform.gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = transform.gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Unlit/VertexColor"));
    }

    public void LoadMatrix()
    {
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
    }
}
