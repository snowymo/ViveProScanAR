using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class OSRDLL : MonoBehaviour {

    [DllImport("OSR"/*, CharSet = CharSet.Ansi, EntryPoint= "CreateOSRData"*/)]
    public static extern IntPtr CreateOSRData();

    [DllImport("OSR")]
    public static extern void DestroyOSRData(IntPtr osrIntPtr);

    [DllImport("OSR")]
    public static extern IntPtr AddScan(IntPtr osrData, Vector3[] vertices, PlyLoaderDll.LABCOLOR[] colors, uint[] faces, float[] fTransform, int verCnt, int faceCnt);
    [DllImport("OSR")]
    public static extern IntPtr AddOldScan(IntPtr osrData, Vector3[] vertices, Color32[] colors, uint[] faces, float[] fTransform, int verCnt, int faceCnt);

    [DllImport("OSR")]
    public static extern void Integrate(IntPtr osrData, IntPtr scan);
    [DllImport("OSR")]
    public static extern IntPtr GetIntegratedVerts(IntPtr osrData, out int count);
    [DllImport("OSR")]
    public static extern IntPtr GetIntegratedColors(IntPtr osrData, out int count);
    [DllImport("OSR")]
    public static extern IntPtr GetIntegratedIndices(IntPtr osrData, out int count);

    [DllImport("OSR")]
    public static extern IntPtr Register(IntPtr osrData, IntPtr scan);

    public static IntPtr _osrInstance = IntPtr.Zero;
    public static IntPtr GetOSRData()
    {
        if(_osrInstance == IntPtr.Zero)
        {
            _osrInstance = CreateOSRData();
        }
        return _osrInstance;
    }

    public static IntPtr OSRAddScan(IntPtr osrData, Vector3[] vertices, PlyLoaderDll.LABCOLOR[] colors, uint[] faces, Matrix4x4 mTransform)
    {
        int vertCnt = vertices.Length;
        int faceCnt = faces.Length / 3;
        float[] transformation = new float[16];
        for (int i = 0; i < transformation.Length; i++)
            transformation[i] = mTransform[i % 4, i / 4];
        return AddScan(osrData, vertices, colors, faces, transformation, vertCnt, faceCnt);
    }
    public static IntPtr OSRAddOldScan(IntPtr osrData, Vector3[] vertices, Color32[] colors, uint[] faces, Matrix4x4 mTransform)
    {
        int vertCnt = vertices.Length;
        int faceCnt = faces.Length / 3;
        float[] transformation = new float[16];
        for (int i = 0; i < transformation.Length; i++)
            transformation[i] = mTransform[i % 4, i / 4];
        return AddOldScan(osrData, vertices, colors, faces, transformation, vertCnt, faceCnt);
    }

    public static void OSRRegister(IntPtr osrData, IntPtr scan, ref Matrix4x4 newTrans)
    {
        IntPtr mPointer = Register(osrData, scan);
        float[] newFloats = new float[16];
        Marshal.Copy(mPointer, newFloats, 0, 16);
        for(int i = 0; i < 16; i++)
        {
            newTrans[i % 4, i / 4] = newFloats[i];
        }
    }

    

    public static void OSRIntegrate(IntPtr osrData, ref IntPtr scan, ref Vector3[] vertices, ref Color32[] colors, ref uint[] faces)
    {
        
        int vAmt, cAmt, fAmt;
        Debug.Log("before OSRIntegrate");

        float prevTime = Time.realtimeSinceStartup;
        Integrate( osrData,  scan);
        float curTime = Time.realtimeSinceStartup;
        Debug.Log("Integrate + extract took:" + (curTime - prevTime) + "s");

        // need to know the amount
        /*Marshal.Copy(fVerts, verts, 0, count);*/
        prevTime = Time.realtimeSinceStartup;
        IntPtr vPointer = GetIntegratedVerts(osrData, out vAmt);
        IntPtr cPointer = GetIntegratedColors(osrData, out cAmt);
        curTime = Time.realtimeSinceStartup;
        Debug.Log("OSRIntegrate verts/colors: " + vAmt + " verts :" + (curTime-prevTime) + "s" );

        prevTime = Time.realtimeSinceStartup;
        vertices = new Vector3[vAmt];
        colors = new Color32[cAmt];
        /*Marshal.Copy(vPointer, vertices, 0, vAmt);*/
        IntPtr vp = vPointer, cp = cPointer;
        for (int i = 0; i < vAmt; i++)
        {
            //Marshal.PtrToStructure(p, vertices[i]);
            vertices[i] = (Vector3)Marshal.PtrToStructure(vp, typeof(Vector3));
            vp += Marshal.SizeOf(typeof(Vector3)); // move to next structure

            //Marshal.PtrToStructure(p, colors[i]);
            colors[i] = (Color32)Marshal.PtrToStructure(cp, typeof(Color32));
            cp += Marshal.SizeOf(typeof(Color32)); // move to next structure
        }
        curTime = Time.realtimeSinceStartup;
        Debug.Log("OSRIntegrate verts/colors copy: " + (curTime - prevTime) + "s");

        IntPtr fPointer = GetIntegratedIndices(osrData, out fAmt);
        Debug.Log("OSRIntegrate: " + fAmt + " faces");
        faces = new uint[fAmt*3];
        //Marshal.Copy(fPointer, faces, 0, fAmt * 3);
        IntPtr p = fPointer;
        for (int i = 0; i < fAmt*3; i++)
        {
            //Marshal.PtrToStructure(p, faces[i]);
            faces[i] = (uint)Marshal.PtrToStructure(p, typeof(uint));
            p += Marshal.SizeOf(typeof(uint)); // move to next structure
        }

        // there is no more seperate control of this scan, actually it is remove in dll side
        scan = IntPtr.Zero;
    }
}
