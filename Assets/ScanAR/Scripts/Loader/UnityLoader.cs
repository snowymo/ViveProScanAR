using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// child of the tracker
// parent of the meshes
public class UnityLoader : MonoBehaviour {

    public ScanARData curInstance;  // maintain mesh info

    public List<GameObject> updateGOs;  // a list of meshes since it might be large mesh

    public Shader shader;

    Vector3[] rawScanVertices;
    Color32[] rawScanColors;
    PlyLoaderDll.LABCOLOR[] rawScanLabColors;
    uint[] rawScanFaces;

    IntPtr OSRdata, curAddedScan;

    // Use this for initialization
    void Start () {
        updateGOs = new List<GameObject>();
        OSRdata = OSRDLL.GetOSRData();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Register(Matrix4x4 mtx)
    {
        curInstance.registerMtx = mtx;
        curInstance.AfterRegister();    // change vertex pieces, which will be used for integration

        for(int i = 0; i < updateGOs.Count; i++)
        {
            Mesh mesh = updateGOs[i].GetComponent<MeshFilter>().mesh;
            Vector3[] registeredVerts = curInstance.curData[(int)curInstance.curState].verticesPieces[i];
            mesh.vertices = registeredVerts;
        }
    }

    public IntPtr AddScan()
    {
        if(OSRdata == IntPtr.Zero)
            OSRdata = OSRDLL.GetOSRData();
        // add scan
        float prevTime = Time.realtimeSinceStartup;
        curAddedScan = OSRDLL.OSRAddScan(OSRdata,
            curInstance.getCurrentMeshData().vertices,
            curInstance.getCurrentMeshData().labColors,
            curInstance.getCurrentMeshData().faces, Matrix4x4.identity);
        //curAddedScan = OSRDLL.OSRAddOldScan(OSRdata, ppl.rawScanVertices, ppl.rawScanColors, ppl.rawScanFaces, Matrix4x4.identity);
        float curTime = Time.realtimeSinceStartup;
        print("curAddedScan address:" + curAddedScan + " took :" + (curTime - prevTime) + "s");
        return curAddedScan;
    }

    public void LoadMeshesDirectly()
    {
        IntPtr plyIntPtr = PlyLoaderDll.LoadPly(Utility.scanPath);

        if (plyIntPtr == null)
            return;

        Mesh mesh = new Mesh();
        rawScanVertices = PlyLoaderDll.GetRVertices(plyIntPtr);
        rawScanColors = PlyLoaderDll.GetRColors(plyIntPtr);
        rawScanLabColors = PlyLoaderDll.GetRColorsLAB(plyIntPtr);
        rawScanFaces = PlyLoaderDll.GetRIndexs(plyIntPtr);
        PlyLoaderDll.UnLoadPly(plyIntPtr);
        
        if (curInstance == null)
            curInstance = new ScanARData();
        curInstance.curState = ScanARData.ScanState.DAVIDSYSTEM;
        curInstance.SetData(rawScanVertices, rawScanColors, rawScanLabColors, rawScanFaces);
        // state update to vive and verts turn to unity scale and left handed
        curInstance.SetDavidToViveTransform();

        int meshCount = rawScanVertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            int vertCnt = Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount);
            Utility.createMesh(i, vertCnt, ref curInstance, ref updateGOs, transform, shader);
        }
    }
}
