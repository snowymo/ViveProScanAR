using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class client : MonoBehaviour {

    public string msg;
    public float[] cmd,fm, test;

    private NetMqListener _netMqListener;

    public string topic;

    private void HandleMessage(string message)
    {
        //         var splittedStrings = message.Split(' ');
        //         if (splittedStrings.Length != 3) return;
        //         var x = float.Parse(splittedStrings[0]);
        //         var y = float.Parse(splittedStrings[1]);
        //         var z = float.Parse(splittedStrings[2]);
        //         transform.position = new Vector3(x, y, z);
        msg = message;
    }

    private void HandleFMessage(byte[] b)
    {
        //         var splittedStrings = message.Split(' ');
        //         if (splittedStrings.Length != 3) return;
        //         var x = float.Parse(splittedStrings[0]);
        //         var y = float.Parse(splittedStrings[1]);
        //         var z = float.Parse(splittedStrings[2]);
        //         transform.position = new Vector3(x, y, z);
        if (msg[0].Equals('s'))
        {
            Buffer.BlockCopy(b, 0, cmd, 0, 4);
        }
        else if (msg[0].Equals('m'))
        {
            int len = int.Parse(msg.Substring(1));
            fm = new float[len];
            Buffer.BlockCopy(b, 0, fm, 0, 4 * len);
        }
        else if (msg.Equals("mesh"))
        {
            // receive mesh
            parseMesh(b);
        }
        else if (msg.Contains("nm"))
        {
            // receive mesh path
            int len = int.Parse(msg.Substring(2));
            string s = Encoding.UTF8.GetString(b, 0, len);
            print(s);
            //print(s[0]);
            testLoadFunc(s);
        }
        else
        {
            Buffer.BlockCopy(b, 0, test, 0, 4);
        }
    }

    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage, HandleFMessage, topic);
        _netMqListener.Start();

        cmd = new float[1];
        fm = new float[64];
        test = new float[1];
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
