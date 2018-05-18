using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TestLoader : MonoBehaviour
{
    public string fileName;

    public Material defaultMat, defaultMat2;

    public Transform secondController;

    bool isFirstPositionCaught, isReady;

    Matrix4x4 ToriginalSecondController;

    public Matrix4x4 TsecondController;

    GameObject go,go2,go3;

    Vector3[] originialVertices, originialVertices2;

    //public Transform test;

    //public GameObject cube;

    void loadPLYLocally()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(filePath);

        originialVertices = PlyLoaderDll.GetVertices(plyIntPtr);
        print(originialVertices[0]);
        Mesh mesh = new Mesh();
        mesh.vertices = PlyLoaderDll.GetVertices(plyIntPtr);
        mesh.uv = PlyLoaderDll.GetUvs(plyIntPtr);
        mesh.normals = PlyLoaderDll.GetNormals(plyIntPtr);
        mesh.colors32 = PlyLoaderDll.GetColors(plyIntPtr);
        mesh.SetIndices(PlyLoaderDll.GetIndexs(plyIntPtr), MeshTopology.Triangles, 0, true);
        mesh.name = "mesh";
        mesh.RecalculateNormals();

        go = new GameObject();
        go.name = "meshNew";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Unlit/Texture"));
        string textureName = PlyLoaderDll.GetTextureName(plyIntPtr);
        if (textureName != null && textureName.Length > 0)
        {
            string texturePath = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, textureName);
            WWW www = new WWW(texturePath);
            while (!www.isDone)
            {
            }
            mr.material.mainTexture = www.texture;
        }
        else
            mr.material = defaultMat;

//         go3 = GameObject.Instantiate(go);
//         go3.name = "original";
// 
        go2 = new GameObject();
        go2.name = "meshNewReverse";
        MeshFilter mf2 = go2.AddComponent<MeshFilter>();
        for(int i = 0;i < mesh.triangles.Length/3; i++)
        {
            int temp = mesh.triangles[i * 3 + 1];
            mesh.triangles[i * 3 + 1] = mesh.triangles[i * 3 + 2];
            mesh.triangles[i * 3 + 2] = temp;
        }
        mf2.mesh = mesh;
        MeshRenderer mr2 = go2.AddComponent<MeshRenderer>();
        mr2.material = defaultMat2;
        originialVertices2 = mesh.vertices;

        PlyLoaderDll.UnLoadPly(plyIntPtr);
    }

    // Use this for initialization
    void Start()
    {
        isReady = false;

        isFirstPositionCaught = false;

        loadPLYLocally();

        isReady = true;

        //ApplyScale(0.001f);

    }

    void ApplyScale(float s)
    {
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int i = 0;
        print("originialVertices:" + originialVertices[0]);
        while (i < vertices.Length)
        {
            //Vector3 Vvive = Tvivetodavid.MultiplyPoint(originialVertices[i]);
            if (i == 0)
                print("after Tvivetodavid:" + originialVertices[i]);
            vertices[i] = originialVertices[i] * s;
            vertices[i].z = -vertices[i].z;
            if (i == 0)
                print("after 1/1000:" + vertices[i].ToString("F3"));
//             if(i < 10)
//             {
//                 GameObject newgo = GameObject.Instantiate(cube);
//                 newgo.transform.position = vertices[i];
//                 newgo.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
//             }

            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void Update()
    {
        if (!isReady)
            return;

        if (!isFirstPositionCaught)
        {
            //if (secondController.isValid)
            //{
                //ToriginalSecondController = Matrix4x4.TRS(secondController.gameObject.transform.position, secondController.gameObject.transform.rotation, Vector3.one);
                for(int i = 0; i < 3; i++)
                {
                    
                    TsecondController[i, 3] /= 1000f;
                }
                TsecondController[0, 2] = -TsecondController[0, 2];
                TsecondController[1, 2] = -TsecondController[1, 2];
                TsecondController[2, 0] = -TsecondController[2, 0];
                TsecondController[2, 1] = -TsecondController[2, 1];
                TsecondController[2, 3] = -TsecondController[2, 3];
                ToriginalSecondController = TsecondController;
                isFirstPositionCaught = true;
            //}
        }

        // update mesh all the time based on updated 2nd controller * original controller.inv * mesh.vertices
        if (isFirstPositionCaught)
        {
            Mesh mesh = go.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int i = 0;
            Matrix4x4 curSecondController = Matrix4x4.TRS(secondController.position, secondController.rotation, Vector3.one);
            print("originialVertices:" + originialVertices[0]);
            while (i < vertices.Length)
            {
                //Vector3 Vvive = Tvivetodavid.MultiplyPoint(originialVertices[i]);
                //if(i == 0)
                    //print("after Tvivetodavid:" + originialVertices[i]);
                Vector3 VviveScale = originialVertices[i] * 0.001f;
                VviveScale.z = -VviveScale.z;
                //if (i == 0)
                    //print("after 1/1000:" + VviveScale);
                vertices[i] = (curSecondController * ToriginalSecondController.inverse).MultiplyPoint(VviveScale);
                i++;
            }
            print("after diff:" + vertices[0]);
            mesh.vertices = vertices;
            mesh.RecalculateNormals();

            mesh = go2.GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            normals = mesh.normals;
            i = 0;
            while (i < vertices.Length)
            {
                //Vector3 VviveInv = Tvivetodavid.inverse.MultiplyPoint(originialVertices[i]);
                if (i == 0)
                    print("after Tvivetodavid inv:" + originialVertices2[0]);
                Vector3 VviveInvScale = originialVertices2[i] * 0.001f;
                VviveInvScale.z = -VviveInvScale.z;
                if (i == 0)
                    print("after 1/1000:" + VviveInvScale);
                vertices[i] = (curSecondController * ToriginalSecondController.inverse).MultiplyPoint(VviveInvScale);
                i++;
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            Matrix4x4 diff = curSecondController * ToriginalSecondController.inverse;
            //test.position = (curSecondController * ToriginalSecondController.inverse).MultiplyPoint(Vector3.one);
        }
        
    }
}