using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OSRDllTest : MonoBehaviour {

    IntPtr OSRdata, curAddedScan;

    UpdateCtrl curController;

    // Use this for initialization
    void Start () {
        integratedVerts = new Vector3[0];
        integratedColors = new Color32[0];
        integratedFaces = new uint[0];

        print("before create OSRdata");
        OSRdata = OSRDLL.GetOSRData();
        print("OSRdata addr:" + OSRdata);

        splitIntVerts = new List<Vector3[]>();
        splitIntColors = new List<Color32[]>();
        splitIntFaces = new List<uint[]>();

        curController = GetComponent<UpdateCtrl>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddScan()
    {
        //TestRplyLoader ppl = GetComponent<TestRplyLoader>();
        print("uCtrl.iniScanVertices:" + curController.iniScanVertices.Length);
        print("uCtrl.iniScanLabColors:" + curController.iniScanLabColors.Length);
        print("uCtrl.iniScanFaces:" + curController.iniScanFaces.Length);
        float prevTime = Time.realtimeSinceStartup;
        curAddedScan = OSRDLL.OSRAddScan(OSRdata, curController.curInstance.getCurrentMeshData().vertices, 
            curController.curInstance.getCurrentMeshData().labColors, curController.curInstance.getCurrentMeshData().faces, Matrix4x4.identity);
        //curAddedScan = OSRDLL.OSRAddOldScan(OSRdata, ppl.rawScanVertices, ppl.rawScanColors, ppl.rawScanFaces, Matrix4x4.identity);
        float curTime = Time.realtimeSinceStartup;
        print("curAddedScan address:" + curAddedScan + " took :" + (curTime - prevTime) + "s");
    }

    Vector3[] integratedVerts;
    Color32[] integratedColors;
    uint[] integratedFaces;

    List<Vector3[]> splitIntVerts;
    List<Color32[]> splitIntColors;
    List<uint[]> splitIntFaces;
    public void IntegrateScan()
    {
        float prevTime = Time.realtimeSinceStartup;
        curController.curInstance.curState = ScanARData.ScanState.INTEG;

        //OSRDLL.OSRIntegrate(OSRdata, ref curAddedScan, ref integratedVerts, ref integratedColors, ref integratedFaces);// later it will become vectors of the data for each mesh
        OSRDLL.OSRIntegrate(OSRdata, ref curAddedScan, ref curController.curInstance.getCurrentMeshData().verticesPieces, 
            ref curController.curInstance.getCurrentMeshData().colorsPieces, 
            ref curController.curInstance.getCurrentMeshData().facesPieces);// later it will become vectors of the data for each mesh

                                                                                                                 //--scanAmount;
                                                                                                                 // modify the data of current scan

        float curTime = Time.realtimeSinceStartup;
        print("IntegrateScan:" + (curTime - prevTime) + "s");

        prevTime = Time.realtimeSinceStartup;

        //TestRplyLoader ppl = GetComponent<TestRplyLoader>();
        curController.updateGOs = new List<GameObject>();// discard the control of meshes before integration, so they are still there, but never updated
        for (int i = 0; i < curController.curInstance.getCurrentMeshData().colorsPieces.Count; i++)
        {
            Vector3[] curVerts = curController.curInstance.getCurrentMeshData().verticesPieces[i];
            Color32[] curColors = curController.curInstance.getCurrentMeshData().colorsPieces[i];
            uint[] curFaces = curController.curInstance.getCurrentMeshData().facesPieces[i];
            Utility.createMesh(curVerts.Length, ref curVerts, ref curColors, curFaces.Length, ref curFaces,
                ref curController.updateGOs, curController.gameObject.transform, curController.shader);
        }
        
        curTime = Time.realtimeSinceStartup;
        print("load meshes:" + (curTime - prevTime) + "s");
    }

    public void RegisterScan()
    {
        //Matrix4x4 resTrans = Matrix4x4.identity;
        if(curAddedScan != IntPtr.Zero)
        {
            float prevTime = Time.realtimeSinceStartup;
            OSRDLL.OSRRegister(OSRdata, curAddedScan, ref curController.curInstance.registerMtx);
            float curTime = Time.realtimeSinceStartup;
            print("after Register() " + (curTime-prevTime) + "s\t" + curController.curInstance.registerMtx.ToString("F3"));

            prevTime = Time.realtimeSinceStartup;
            curController.curInstance.AfterRegister();
            curTime = Time.realtimeSinceStartup;
            print("apply Register() transformation " + (curTime - prevTime) + "s");
        }

        // need to apply to all vertices
    }
}
