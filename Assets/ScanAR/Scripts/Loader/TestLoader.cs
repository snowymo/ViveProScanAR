using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TestLoader : MonoBehaviour
{
    public string fileName;
    public bool isAbsolute;
    public string absFilePath;

    public enum FuncType { LOADPLY, DOWNSP };
    public FuncType funcType;

    public Material defaultMat, defaultMat2;

    public Transform secondController;

    bool isFirstPositionCaught, isReady;

    Matrix4x4 ToriginalSecondController;
    public Matrix4x4 TsecondController;


    GameObject go,go2,go3;

    Vector3[] originialVertices, originialVertices2;

    public Shader shader;

    public int limitCount;
    int[] indices;

    List<GameObject> gos = new List<GameObject>();

    float curTime, prevTime;

    public float downsample;

    //public Transform test;

    //public GameObject cube;

    void loadPLYLocally()
    {
        if(!isAbsolute)
            absFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(absFilePath);

        originialVertices = PlyLoaderDll.GetVertices(plyIntPtr);
        print(originialVertices[0]);
        Mesh mesh = new Mesh();
        mesh.vertices = PlyLoaderDll.GetVertices(plyIntPtr);
        //mesh.uv = PlyLoaderDll.GetUvs(plyIntPtr);
        //mesh.normals = PlyLoaderDll.GetNormals(plyIntPtr);
        mesh.colors32 = PlyLoaderDll.GetColors(plyIntPtr);
        mesh.SetIndices(PlyLoaderDll.GetIndexs(plyIntPtr), MeshTopology.Triangles, 0, true);
        mesh.name = "mesh";
        mesh.RecalculateNormals();

        go = new GameObject();
        go.name = "meshNew";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.material = new Material(Shader.Find("Unlit/VertexColor"));
        mr.material = new Material(shader);
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
        mr2.material = new Material(Shader.Find("Unlit/VertexColor"));
        originialVertices2 = mesh.vertices;

        PlyLoaderDll.UnLoadPly(plyIntPtr);
    }

    void createMesh(int startIdx, int verticeCnt, ref Vector3[] vertex, ref Vector2[] uv, ref Texture2D texture)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];
        
        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, startIdx * limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Vector2[] curUV = new Vector2[verticeCnt];
        Array.Copy(uv, startIdx * limitCount, curUV, 0, verticeCnt);
        mesh.uv = curUV;

        //         for (int i = 0; i < verticeCnt; i++)
        //         {
        //             mesh.vertices[i] /= 1000f;       
        //         }
        if (indices.Length > verticeCnt)
        {
            int[] subindices = new int[verticeCnt];
            Array.Copy(indices, subindices, verticeCnt);
            mesh.SetIndices(subindices, MeshTopology.Points, 0);
        }
        else
            mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.name = "mesh" + startIdx.ToString();

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = transform;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
        mr.material.mainTexture = texture;
    }

    void loadPLYDownSample()
    {
        if (!isAbsolute)
            absFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);



        IntPtr plyIntPtr = PlyLoaderDll.LoadPlyDownSample(absFilePath, (int)(downsample*100));

        curTime = Time.realtimeSinceStartup;
        print("LoadPlyDownSample took " + (curTime - prevTime) + "s");
        prevTime = curTime;

        string textPrefix = absFilePath.Substring(0, absFilePath.LastIndexOf('\\')+1);

        Vector3[] vo = PlyLoaderDll.GetVertices(plyIntPtr);
        Vector2[] uvo = PlyLoaderDll.GetUvs(plyIntPtr);

        curTime = Time.realtimeSinceStartup;
        print("GetVertices and UVs took " + (curTime - prevTime) + "s");
        prevTime = curTime;

        string textureName = "file://" + textPrefix + PlyLoaderDll.GetTextureName(plyIntPtr);
        WWW www = new WWW(textureName);
        while (!www.isDone)
        {
        }
        Texture2D texture = www.texture;
        
//         Vector3[] vo = new Vector3[0];
//         Vector2[] uvo = new Vector2[0];
//         PlyLoaderDll.GetDownSample(plyIntPtr, ref vo, ref uvo, downsample);

        PlyLoaderDll.UnLoadPly(plyIntPtr);

        int meshCount = vo.Length / limitCount + 1;
        for(int i = 0; i < meshCount; i++)
        {
            createMesh(i, Math.Min(limitCount, vo.Length - i * limitCount), ref vo, ref uvo, ref texture);
        }
        curTime = Time.realtimeSinceStartup;
        print("createMesh took " + (curTime - prevTime) + "s");
        prevTime = curTime;
    }

    // Use this for initialization
    void Start()
    {
        float startTime = Time.realtimeSinceStartup;
        prevTime = Time.realtimeSinceStartup;
        isReady = false;

        isFirstPositionCaught = false;

        indices = new int[65000];
        for (int i = 0; i < 65000; i++)
        {
            indices[i] = i;
        }

        curTime = Time.realtimeSinceStartup;
        print("create indices took " + (curTime - prevTime) + "s");
        prevTime = curTime;

        switch (funcType)
        {
            case FuncType.LOADPLY:
                loadPLYLocally();
                isReady = true;
                break;
            case FuncType.DOWNSP:
                loadPLYDownSample();
                curTime = Time.realtimeSinceStartup;
                print("whole took " + (curTime- startTime) + "s");
                break;
            default:
                break;
        }
        

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