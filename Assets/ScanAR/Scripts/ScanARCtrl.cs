using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient;

    public client zmqMatrixClient;

    public Transform steamTracker, secondController;
    public Transform scanController;

    public GameObject loader;

    List<GameObject> scans;

    int packetId;

    public bool isJustIssueScan;

    // osr related
    IntPtr OSRdata, curAddedScan;
    int scanAmount; // increase by one if scan new, decrease by one if integrated
    Vector3[] integratedVerts;
    Color32[] integratedColors;
    uint[] integratedFaces;


        // Use this for initialization
    void Start () {
        scans = new List<GameObject>();
        packetId = -1;
        Utility.InitialIndices();
        isJustIssueScan = false;
        print("before create OSRdata");
        OSRdata = OSRDLL.CreateOSRData();
        print("OSRdata addr:" + OSRdata);
        scanAmount = 0;
    }

    void ZMQhandle()
    {
        if (zmqMeshClient.bNewMsg)
        {
            if (zmqMeshClient.msgType == client.MsgType.POINTS)
            {
                // then we need to have a matrix for T->D
                if (!zmqMatrixClient.bNewMsg)
                    return;
            }
            // check packet id, they should be adjacent and larger than current one
            if ((zmqMeshClient.currentId > packetId) || (zmqMeshClient.currentId <= 1))
            {
                packetId = zmqMeshClient.currentId;

                GameObject newscan = GameObject.Instantiate(loader);
                newscan.transform.parent = transform;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMeshClient = zmqMeshClient;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMatrixClient = zmqMatrixClient;
                float prevTime = Time.realtimeSinceStartup;
                newscan.transform.GetComponent<PLYPathLoader>().LoadMeshes();
                float curTime = Time.realtimeSinceStartup;
                print("load meshes:" + (curTime - prevTime) + "s");
                newscan.transform.GetComponent<PLYPathLoader>().LoadMatrix();
                if (steamTracker != null)
                    newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker;


                // we only need to keep at most one point cloud mesh and one mesh list
                // if the latest info is integrated mesh, then we only show integrated mesh;
                // if it is point cloud, then we show point cloud and integrate mesh(which might be empty at the beginning;

                if (scans.Count >= 2)
                {
                    if (scans[scans.Count - 2].transform.GetComponent<PLYPathLoader>().zmqMeshClient.msgType == client.MsgType.POINTS)
                        scans[scans.Count - 2].SetActive(false);
                }

                if (zmqMeshClient.msgType == client.MsgType.MESHES)
                {
                    // disable previous meshes and latest point cloud
                    if (scans[scans.Count - 1].transform.GetComponent<PLYPathLoader>().zmqMeshClient.msgType == client.MsgType.POINTS)
                        scans[scans.Count - 1].SetActive(false);
                }

                scans.Add(newscan);

                zmqMeshClient.bNewMsg = false;
                zmqMatrixClient.bNewMsg = false;

            }
        }
    }

    public void IntegrateScan()
    {
        float prevTime = Time.realtimeSinceStartup;

        OSRDLL.OSRIntegrate(OSRdata, curAddedScan, ref integratedVerts, ref integratedColors, ref integratedFaces);// later it will become vectors of the data for each mesh
        //--scanAmount;
        // modify the data of current scan
        GameObject curScan = scans[scans.Count - 1];
        float curTime = Time.realtimeSinceStartup;
        print("IntegrateScan:" + (curTime - prevTime) + "s");

        prevTime = Time.realtimeSinceStartup;
        curScan.transform.GetComponent<PLYPathLoader>().UpdateIntegratedMesh(ref integratedVerts,  ref integratedColors,  ref integratedFaces);
        curTime = Time.realtimeSinceStartup;
        print("load meshes:" + (curTime - prevTime) + "s");

    }

    public void RegisterScan()
    {
        Matrix4x4 resTrans = Matrix4x4.identity;
        OSRDLL.OSRRegister(OSRdata, curAddedScan, ref resTrans);
        print("after Register() " + resTrans.ToString("F3"));
    }

    // do what DavidLoader did
    void dllHandle()
    {
        // check if the file exist
        if (isJustIssueScan && File.Exists(Utility.scanPath))
        {
            isJustIssueScan = false;

            // load the ply 
            GameObject newscan = GameObject.Instantiate(loader);
            newscan.transform.parent = transform;
            float prevTime = Time.realtimeSinceStartup;
            //newscan.transform.GetComponent<PLYPathLoader>().plyCoordType = PLYPathLoader.PLY_COORD.TEST;
            if (steamTracker.gameObject.GetComponent<Trackers>().secondTracker != null)
                newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker.gameObject.GetComponent<Trackers>().secondTracker.transform;
            if (steamTracker.gameObject.GetComponent<Trackers>().scanTracker != null)
                newscan.transform.GetComponent<PLYPathLoader>().scanTracker = steamTracker.gameObject.GetComponent<Trackers>().scanTracker.transform;
            if (secondController != null)
                newscan.transform.GetComponent<PLYPathLoader>().secondaryController = secondController;
            if (scanController != null)
                newscan.transform.GetComponent<PLYPathLoader>().scanController = scanController;

            // duplicate for sc and st test
            newscan.transform.GetComponent<PLYPathLoader>().LoadMeshesDirectly();
            //newscan.transform.GetComponent<PLYPathLoader>().LoadMeshesDUO();

            float curTime = Time.realtimeSinceStartup;
            print("load meshes:" + (curTime - prevTime) + "s");
            newscan.transform.GetComponent<PLYPathLoader>().LoadMatrixDirectly();

            scans.Add(newscan);

            // move that to session folder, it is fine not to do it now

            // render it with correct transform, get the calibration elsewhere

            
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (scans.Count > scanAmount)
        {
            // IntPtr OSRAddScan(IntPtr osrData, Vector3[] vertices, Color32[] colors, uint[] faces, Matrix4x4 mTransform)
            PLYPathLoader ppl = scans[scans.Count - 1].transform.GetComponent<PLYPathLoader>();
            print("ppl.rawScanColors:" + ppl.rawScanColors[0].ToString("F3") + " " + ppl.rawScanColors[100].ToString("F3"));
            curAddedScan = OSRDLL.OSRAddScan(OSRdata, ppl.rawScanVertices, ppl.rawScanColors, ppl.rawScanFaces, ppl.originalSCtoDMatrix);
            ++scanAmount;
        }

        dllHandle();

        
	}

    private void OnApplicationQuit()
    {
        OSRDLL.DestroyOSRData(OSRdata);
    }
}
