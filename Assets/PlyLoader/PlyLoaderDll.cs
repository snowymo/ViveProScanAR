using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlyLoaderDll : MonoBehaviour
{
    [DllImport("PlyLoader", CharSet = CharSet.Ansi)]
    public static extern IntPtr LoadPly(string fileName);

    [DllImport("PlyLoader", CharSet = CharSet.Ansi)]
    public static extern IntPtr LoadPlyDownSample(string fileName, int downsample);

    [DllImport("PlyLoader")]
    public static extern void UnLoadPly(IntPtr plyIntPtr);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyVerts(IntPtr plyIntPtr, out int count);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyNormals(IntPtr plyIntPtr, out int count);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyColors(IntPtr plyIntPtr, out int count);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyIndexs(IntPtr plyIntPtr, out int count);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyUvs(IntPtr plyIntPtr, out int count);

    [DllImport("PlyLoader")]
    public static extern IntPtr GetPlyTextureName(IntPtr plyIntPtr);

    public static Vector3[] GetVertices(IntPtr plyIntPtr)
    {
        List<Vector3> resultList = new List<Vector3>();
        int count;
        IntPtr datPtr = GetPlyVerts(plyIntPtr, out count);
        if (count == 0)
            return null;

        float[] verts = new float[count];
        Marshal.Copy(datPtr, verts, 0, count);
        for (int i = 0; i < count/3; i++)
            resultList.Add(new Vector3(verts[i*3], verts[i*3 + 1], verts[i*3 + 2]));
        return resultList.ToArray();
    }

    public static Vector3[] GetRVertices(IntPtr plyIntPtr)
    {
        List<Vector3> resultList = new List<Vector3>();
        int count;
        IntPtr datPtr = GetPlyVerts(plyIntPtr, out count);
        print("in Dll wrapper:" + count + " vertices");
        if (count == 0)
            return null;

        float[] verts = new float[count*3];
        Marshal.Copy(datPtr, verts, 0, count*3);
        for (int i = 0; i < count; i++)
            resultList.Add(new Vector3(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]));
        return resultList.ToArray();
    }

    public static Vector3[] GetNormals(IntPtr plyIntPtr)
    {
        List<Vector3> resultList = new List<Vector3>();
        int count;
        IntPtr datPtr = GetPlyNormals(plyIntPtr, out count);
        if (count == 0)
            return null;

        float[] normals = new float[count];
        Marshal.Copy(datPtr, normals, 0, count);
        for (int i = 0; i < count/3; i++)
            resultList.Add(new Vector3(normals[i*3], normals[i*3 + 1], normals[i*3 + 2]));
        return resultList.ToArray();
    }

    public static Color32[] GetColors(IntPtr plyIntPtr)
    {
        List<Color32> resultList = new List<Color32>();
        int count;
        IntPtr datPtr = GetPlyColors(plyIntPtr, out count);
        if (count == 0)
            return null;

        byte[] colors = new byte[count];
        Marshal.Copy(datPtr, colors, 0, count);
        for (int i = 0; i < count/4; i++)
            resultList.Add(new Color32(colors[i*4], colors[i*4 + 1], colors[i*4 + 2], colors[i*4 + 3]));
        return resultList.ToArray();
    }

    public static Color32[] GetRColors(IntPtr plyIntPtr)
    {
        List<Color32> resultList = new List<Color32>();
        int count;
        IntPtr datPtr = GetPlyColors(plyIntPtr, out count);
        print("in Dll wrapper:" + count + " colors");
        if (count == 0)
            return null;

        byte[] colors = new byte[count*3];
        Marshal.Copy(datPtr, colors, 0, count*3);
        for (int i = 0; i < count; i++)
            resultList.Add(new Color32(colors[i * 3], colors[i * 3 + 1], colors[i * 3 + 2], 255));
        return resultList.ToArray();
    }

    public static int[] GetIndexs(IntPtr plyIntPtr)
    {
        List<int> resultList = new List<int>();
        int count;
        IntPtr datPtr = GetPlyIndexs(plyIntPtr, out count);
        print("in Dll wrapper:" + count + " indices");
        if (count == 0)
            return null;

        int[] indexs = new int[count];
        Marshal.Copy(datPtr, indexs, 0, count);
        for (int i = 0; i < count; i++)
            resultList.Add(indexs[i]);
        return resultList.ToArray();
    }

    public static int[] GetRIndexs(IntPtr plyIntPtr)
    {
        List<int> resultList = new List<int>();
        int count;
        IntPtr datPtr = GetPlyIndexs(plyIntPtr, out count);
        print("in Dll wrapper:" + count + " indices");
        if (count == 0)
            return null;

        int[] indexs = new int[count*3];
        Marshal.Copy(datPtr, indexs, 0, count*3);
        for (int i = 0; i < count*3; i++)
            resultList.Add(indexs[i]);
        return resultList.ToArray();
    }

    public static Vector2[] GetUvs(IntPtr plyIntPtr)
    {
        List<Vector2> resultList = new List<Vector2>();
        int count;
        IntPtr datPtr = GetPlyUvs(plyIntPtr, out count);
        if (count == 0)
            return null;

        float[] faceuvs = new float[count];
        Marshal.Copy(datPtr, faceuvs, 0, count);
        for (int i = 0; i < count/2; i++)
            resultList.Add(new Vector2(faceuvs[i*2], faceuvs[i*2 + 1]));

        return resultList.ToArray();
    }

    public static void GetDownSample(IntPtr plyIntPtr, ref Vector3[] vertices, ref Vector2[] uvs, float downsample)
    {
        
        List<Vector3> vertresultList = new List<Vector3>();
        List<Vector2> uvresultList = new List<Vector2>();

        int count, count2;
        IntPtr vertdatPtr = GetPlyVerts(plyIntPtr, out count);
        if (count == 0)
            return;
        float[] verts = new float[count];
        Marshal.Copy(vertdatPtr, verts, 0, count);

        IntPtr uvdatPtr = GetPlyUvs(plyIntPtr, out count2);
        float[] faceuvs = new float[count2];
        Marshal.Copy(uvdatPtr, faceuvs, 0, count2);

        for (int i = 0; i < count / 3; i++)
        {
            if(UnityEngine.Random.value >= downsample)
            {
                vertresultList.Add(new Vector3(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]));
                uvresultList.Add(new Vector2(faceuvs[i * 2], faceuvs[i * 2 + 1]));
            }
        }

        vertices = vertresultList.ToArray();
        uvs = uvresultList.ToArray();

    }

    public static string GetTextureName(IntPtr plyIntPtr)
    {
        return Marshal.PtrToStringAnsi(GetPlyTextureName(plyIntPtr));
    }
}