using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System;

public class ScanARCtrl : MonoBehaviour {

    public Matrix4x4 matrixST2David;

    public Transform trackers, secondController, scanController;

    public GameObject loader;

    List<GameObject> scans;

    public bool isJustIssueScan;

    bool isIntegrated;

    GameObject integratedScan;

    // osr related
    IntPtr OSRdata, curAddedScan;
    int scanAmount; // increase by one if scan new, decrease by one if integrated
    Vector3[] integratedVerts;
    Color32[] integratedColors;
    uint[] integratedFaces;

    List<Vector3[]> splitIntVerts;
    List<Color32[]> splitIntColors;
    List<uint[]> splitIntFaces;


    // Use this for initialization
    void Start () {
        Utility.InitialIndices();
        isJustIssueScan = false;
        LoadST2DMatrix();

        print("before create OSRdata");
        OSRdata = OSRDLL.GetOSRData();
        print("OSRdata addr:" + OSRdata);

        scans = new List<GameObject>();
        isIntegrated = false;

        scanAmount = 0;
        splitIntVerts = new List<Vector3[]>();
        splitIntColors = new List<Color32[]>();
        splitIntFaces = new List<uint[]>();

    }

    public void LoadST2DMatrix()
    {
        StreamReader reader = new StreamReader(Utility.calibrationFilePath);
        string line;
        // Read and display lines from the file until the end of the file is reached.
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Contains(Utility.calibScanTrackerIndicator))
            {
                line = reader.ReadLine();//#
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    line = reader.ReadLine(); // first line
                    string[] firstRow = line.Split(new string[] { " " }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < firstRow.Length; colIdx++)
                        matrixST2David[rowIdx, colIdx] = float.Parse(firstRow[colIdx]);
                }
            }
        }
        reader.Close();

        // from david scale to unity scale
        for (int i = 0; i < 3; i++)
            matrixST2David[i, 3] /= 1000f;

        // right hand to left hand
        matrixST2David = Utility.LHMatrixFromRHMatrix(matrixST2David);

        print("Matrix ST to David:" + matrixST2David.ToString("F3"));
    }

    public void IntegrateScan()
    {

        float prevTime = Time.realtimeSinceStartup;
        UnityLoader integratedScanLoader = integratedScan.GetComponent<UnityLoader>();
        integratedScanLoader.curInstance = new ScanARData();
        integratedScanLoader.curInstance.curState = ScanARData.ScanState.INTEG;

        // set the data to the only integrated object
        OSRDLL.OSRIntegrate(OSRdata, ref curAddedScan, ref integratedScanLoader.curInstance.getCurrentMeshData().verticesPieces,
            ref integratedScanLoader.curInstance.getCurrentMeshData().colorsPieces,
            ref integratedScanLoader.curInstance.getCurrentMeshData().facesPieces);// later it will become vectors of the data for each mesh

        float curTime = Time.realtimeSinceStartup;
        print("IntegrateScan:" + (curTime - prevTime) + "s");

        prevTime = Time.realtimeSinceStartup;
        
        integratedScanLoader.updateGOs = new List<GameObject>();// 

        // disable all integrated children
        for(int i = 0; i < integratedScan.gameObject.transform.childCount; i++)
        {
            Transform childTrans = integratedScan.gameObject.transform.GetChild(i);
            childTrans.gameObject.SetActive(false);
        }
        

        for (int i = 0; i < integratedScanLoader.curInstance.getCurrentMeshData().colorsPieces.Count; i++)
        {
            Vector3[] curVerts = integratedScanLoader.curInstance.getCurrentMeshData().verticesPieces[i];
            Color32[] curColors = integratedScanLoader.curInstance.getCurrentMeshData().colorsPieces[i];
            uint[] curFaces = integratedScanLoader.curInstance.getCurrentMeshData().facesPieces[i];
            Utility.createMesh(curVerts.Length, ref curVerts, ref curColors, curFaces.Length, ref curFaces,
                ref integratedScanLoader.updateGOs, integratedScan.transform, integratedScanLoader.shader);
        }

        curTime = Time.realtimeSinceStartup;
        print("load meshes:" + (curTime - prevTime) + "s");

        // disable all scans
        for(int i = 0; i < scans.Count; i++)
        {
            scans[i].SetActive(false);
        }

    }

    public void RegisterScan()
    {
        // if current scan amout is > 1 then do it
        if(scans.Count > 1 && curAddedScan != IntPtr.Zero)
        {
            Matrix4x4 registerMtx = Matrix4x4.identity;
                float prevTime = Time.realtimeSinceStartup;
            OSRDLL.OSRRegister(OSRdata, curAddedScan, ref registerMtx);

                float curTime = Time.realtimeSinceStartup;
                Vector3 regPos = registerMtx.GetPosition();
                Quaternion regRot = registerMtx.GetRotation();

                print("after Register() " + (curTime - prevTime) + "s\t" + registerMtx.ToString("F3"));
                print("reg pos:" + regPos.ToString("F3"));
                print("reg rot:" + regRot.eulerAngles.ToString("F3"));

            // right hand to left hand
            registerMtx = Utility.LHMatrixFromRHMatrix(registerMtx);
                
                regPos = registerMtx.GetPosition();
                regRot = registerMtx.GetRotation();
                print(" lf reg pos:" + regPos.ToString("F3"));
                print(" lf reg rot:" + regRot.eulerAngles.ToString("F3"));

            // assign to the mesh to apply that
            GameObject curScan = scans[scans.Count - 1];
            curScan.transform.GetComponent<UnityLoader>().Register(registerMtx);
        }
    }

    // do what DavidLoader did
    void LoadScan()
    {
        // check if the file exist
        if (isJustIssueScan && File.Exists(Utility.scanPath))
        {
            isJustIssueScan = false;

            if (!isIntegrated)
            {
                isIntegrated = true;
                // no integrated before, create the only integration obj under second tracker to maintain all the integration results
                integratedScan = GameObject.Instantiate(loader);
                // assign to secondTracker's child
                integratedScan.transform.parent = trackers.Find("secondTracker");
                // child's transform should be secT.inv * ST * ST2D, secT should be the transform when issued the scan
//                 Matrix4x4 SecondTracker = Matrix4x4.TRS(integratedScan.transform.parent.transform.position, integratedScan.transform.parent.transform.rotation, Vector3.one);
//                 Transform scanTrackerTrans = trackers.Find("scanTracker");
//                 Matrix4x4 ScanTracker = Matrix4x4.TRS(scanTrackerTrans.position, scanTrackerTrans.rotation, Vector3.one);
//                 Matrix4x4 childMatrix = SecondTracker.inverse * ScanTracker * matrixST2David;
                integratedScan.transform.localPosition = Vector3.zero;
                integratedScan.transform.localRotation = Quaternion.identity;
                integratedScan.name = "integrated";
            }

            GameObject newscan = GameObject.Instantiate(loader);
            // assign to secondTracker's child
            newscan.transform.parent = trackers.Find("secondTracker");
            // child's transform should be secT.inv * ST * ST2D, secT should be the transform when issued the scan
            Matrix4x4 curSecondTracker = Matrix4x4.TRS(newscan.transform.parent.transform.position, newscan.transform.parent.transform.rotation, Vector3.one);
            Transform scanTrackerTransform = trackers.Find("scanTracker");
            Matrix4x4 curScanTracker = Matrix4x4.TRS(scanTrackerTransform.position, scanTrackerTransform.rotation, Vector3.one);
            Matrix4x4 childMtx = curSecondTracker.inverse * curScanTracker * matrixST2David;
             newscan.transform.localPosition = Vector3.zero;
             newscan.transform.localRotation = Quaternion.identity;
//            print("newscan:" + newscan.transform.localPosition.ToString("F3") + "\t" + newscan.transform.localRotation.ToString("F3"));

            // load the ply
            float prevTime = Time.realtimeSinceStartup;
            newscan.GetComponent<UnityLoader>().LoadMeshesDirectly(childMtx);
            float curTime = Time.realtimeSinceStartup;
            print("load meshes:" + (curTime - prevTime) + "s");

            curAddedScan = newscan.GetComponent<UnityLoader>().AddScan();

            scans.Add(newscan);

            // move that to session folder, it is fine not to do it now
        }

    }
	
	// Update is called once per frame
	void Update () {

//         if (scans.Count > scanAmount)
//         {
//             
//             ++scanAmount;
//         }

        LoadScan();

        
	}

    private void OnApplicationQuit()
    {
        OSRDLL.DestroyOSRData(OSRdata);
    }
}
