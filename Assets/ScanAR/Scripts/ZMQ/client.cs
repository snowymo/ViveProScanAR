using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class client : MonoBehaviour {
    public string msg, meshPath;
    public string[] meshPaths;
    public string pcPath;
    [SerializeField]
    float[] cmd, test;
    public float[] fm;

    public bool bNewMsg;

    private NetMqListener _netMqListener;

    public string topic;

    public int currentId;

    public enum MsgType { MTX, MESHES, POINTS};
    public MsgType msgType;

    private void HandleMessage(string message)
    {
        msg = message;
    }

    void ParseMatrix(byte[] b)
    {
        currentId = int.Parse(msg.Substring(1, 1));
        int matrixLen = int.Parse(msg.Substring(2));
        fm = new float[matrixLen];
        Buffer.BlockCopy(b, 0, fm, 0, 4 * matrixLen);
    }

    void ParseMeshPath(byte[] b)
    {
        currentId = int.Parse(msg.Substring(2, 1));// "nm[id]"
        int meshNameLen = int.Parse(msg.Substring(3));
        string curmeshPath = Encoding.UTF8.GetString(b, 0, meshNameLen);
        int pathAmt = int.Parse(Encoding.UTF8.GetString(b, meshNameLen, b.Length - meshNameLen));
        meshPaths = new string[pathAmt];
        for(int i = 0; i < pathAmt; i++)
        {
            meshPaths[i] = curmeshPath.Substring(0, curmeshPath.IndexOf('.')) + i.ToString() + ".ply";
            print(meshPaths[i]);
        }
        msgType = MsgType.MESHES;
    }

    void ParsePointCloud(byte[] b)
    {
        currentId = int.Parse(msg.Substring(4, 1));// "nmpc[id]"
        int meshNameLen = int.Parse(msg.Substring(5));
        pcPath = Encoding.UTF8.GetString(b, 0, meshNameLen);
        msgType = MsgType.POINTS;
    }

    private void HandleFMessage(byte[] b)
    {
        if (msg[0].Equals('s'))
        {
            Buffer.BlockCopy(b, 0, cmd, 0, 4);
        }
        else if (msg[0].Equals('m'))
        {
            //             int len = int.Parse(msg.Substring(1));
            //             fm = new float[len];
            //             Buffer.BlockCopy(b, 0, fm, 0, 4 * len);
            ParseMatrix(b);
        }
        else if (msg.Equals("mesh"))
        {
            // receive mesh
            parseMesh(b);
        }
        else if (msg.Contains("nm"))
        {
            // receive mesh path
            //             int len = int.Parse(msg.Substring(2));
            //             meshPath = Encoding.UTF8.GetString(b, 0, len);
            //             print(meshPath);
            if (msg.Contains("pc"))
            {
                ParsePointCloud(b);
            }
            else
                ParseMeshPath(b);
            //print(s[0]);
            //testLoadFunc(s);
        }
        else
        {
            Buffer.BlockCopy(b, 0, test, 0, 4);
        }
        bNewMsg = true;
    }

    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage, HandleFMessage, topic);
        _netMqListener.Start();

        cmd = new float[1];
        fm = new float[0];
        test = new float[1];

        bNewMsg = false;
        currentId = -1;
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }

    private void parseMesh(byte[] b)
    {
        int index = 0;
        // vCnt
        int[] vertexCnt = new int[0];
        Buffer.BlockCopy(b, index, vertexCnt, 0, 4);
        index += 4;
        int[] faceCnt = new int[0];
        Buffer.BlockCopy(b, index, faceCnt, 0, 4);
        index += 4;
        print("receive mesh with point:" + vertexCnt[0] + " and faces: " + faceCnt[0]);
        // fCnt
        // points[float*3] + colors[float*3] * vCnt
        // faces[int*3] * fCnt
    }

    void testLoadFunc(string path)
    {
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(path);
        
        Mesh mesh = new Mesh();
        mesh.vertices = PlyLoaderDll.GetVertices(plyIntPtr);
        //mesh.uv = PlyLoaderDll.GetUvs(plyIntPtr);
        //mesh.normals = PlyLoaderDll.GetNormals(plyIntPtr);
        mesh.colors32 = PlyLoaderDll.GetColors(plyIntPtr);
        mesh.SetIndices(PlyLoaderDll.GetIndexs(plyIntPtr), MeshTopology.Triangles, 0, true);
        mesh.name = "mesh";
        mesh.RecalculateNormals();

        GameObject go = new GameObject();
        go.name = "meshNew";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Unlit/VertexColor"));
        //string textureName = PlyLoaderDll.GetTextureName(plyIntPtr);
        //mr.material = defaultMat;
    }
}
