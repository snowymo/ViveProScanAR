using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OSRDllTest : MonoBehaviour {

    IntPtr OSRdata, curAddedScan;

    // Use this for initialization
    void Start () {
        integratedVerts = new Vector3[0];
        integratedColors = new Color32[0];
        integratedFaces = new uint[0];

        print("before create OSRdata");
        OSRdata = OSRDLL.GetOSRData();
        print("OSRdata addr:" + OSRdata);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddScan()
    {
        TestRplyLoader ppl = GetComponent<TestRplyLoader>();
        curAddedScan = OSRDLL.OSRAddScan(OSRdata, ppl.rawScanVertices, ppl.rawScanLabColors, ppl.rawScanFaces, Matrix4x4.identity);
        //curAddedScan = OSRDLL.OSRAddOldScan(OSRdata, ppl.rawScanVertices, ppl.rawScanColors, ppl.rawScanFaces, Matrix4x4.identity);
        print("curAddedScan address:" + curAddedScan);
    }

    Vector3[] integratedVerts;
    Color32[] integratedColors;
    uint[] integratedFaces;
    public void IntegrateScan()
    {
        float prevTime = Time.realtimeSinceStartup;

        OSRDLL.OSRIntegrate(OSRdata, curAddedScan, ref integratedVerts, ref integratedColors, ref integratedFaces);// later it will become vectors of the data for each mesh
        //--scanAmount;
        // modify the data of current scan
        
        float curTime = Time.realtimeSinceStartup;
        print("IntegrateScan:" + (curTime - prevTime) + "s");

        prevTime = Time.realtimeSinceStartup;

        TestRplyLoader ppl = GetComponent<TestRplyLoader>();
        ppl.createMesh(integratedVerts.Length, ref integratedVerts, ref integratedColors, integratedFaces.Length, ref integratedFaces);
        curTime = Time.realtimeSinceStartup;
        print("load meshes:" + (curTime - prevTime) + "s");
    }

    public void RegisterScan()
    {
        Matrix4x4 resTrans = Matrix4x4.identity;
        OSRDLL.OSRRegister(OSRdata, curAddedScan, ref resTrans);
        print("after Register() " + resTrans.ToString("F3"));
        // need to apply to all vertices
    }
}
