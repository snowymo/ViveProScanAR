using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Utility {

    public static int[] indices;

    public static int limitCount = 65000;

    public static float downsample;

    public static void InitialIndices()
    {
        if(indices == null)
        {
            indices = new int[65000];
            for (int i = 0; i < 65000; i++)
            {
                indices[i] = i;
            }
        }
        
    }

    public static void createMesh(int startIdx, int verticeCnt, ref Vector3[] vertex, ref Color32[] color, ref List<GameObject> gos, Transform parent, Shader shader, ref List<Vector3[]> iniVertices)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(vertex, startIdx * Utility.limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Color32[] curC = new Color32[verticeCnt];
        Array.Copy(color, startIdx * Utility.limitCount, curC, 0, verticeCnt);
        mesh.colors32 = curC;

        if (indices.Length > verticeCnt)
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
        go.transform.parent = parent;
        gos.Add(go);
        iniVertices.Add(curV);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    public static void createMesh(int startIdx, int verticeCnt, ref ScanARData curInstance, ref List<GameObject> gos, Transform parent, Shader shader)
    {
        Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[verticeCnt];

        Vector3[] curV = new Vector3[verticeCnt];
        Array.Copy(curInstance.curData[(int)curInstance.curState].vertices, startIdx * Utility.limitCount, curV, 0, verticeCnt);
        mesh.vertices = curV;

        Color32[] curC = new Color32[verticeCnt];
        Array.Copy(curInstance.curData[(int)curInstance.curState].colors, startIdx * Utility.limitCount, curC, 0, verticeCnt);
        mesh.colors32 = curC;

        if (indices.Length > verticeCnt)
        {
            int[] subindices = new int[verticeCnt];
            Array.Copy(Utility.indices, subindices, verticeCnt);
            mesh.SetIndices(subindices, MeshTopology.Points, 0);
        }
        else
            mesh.SetIndices(Utility.indices, MeshTopology.Points, 0);
        mesh.name = "mesh" + startIdx.ToString();

        GameObject go = new GameObject("go" + startIdx.ToString());
        go.transform.parent = parent;
        gos.Add(go);
        curInstance.curData[(int)curInstance.curState].verticesPieces.Add(curV);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);

        Debug.Log("ScanVerticesPieces:" + curInstance.curData[(int)curInstance.curState].verticesPieces.Count);
    }

    // for integrated results
    public static void createMesh(int verticeCnt, ref Vector3[] vertex, ref Color32[] color, int faceCnt, ref uint[] faces,
        ref List<GameObject> gos, Transform parent, Shader shader)
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
        go.transform.parent = parent;
        gos.Add(go);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(shader);
    }

    public static string calibrationFilePath = "D:\\Projects\\OnlineSurfaceReconstruction\\build_msvc14_64\\Calibration.aln";
    public static string calibrationIndicator = "scan.ply";
    public static string calibScanControllerIndicator = "scanControllerToDavid.ply";
    public static string calibScanTrackerIndicator = "scanTrackerToDavid.ply";
    public static string scanPath = "D:\\Scans\\currentScan.ply";
}
