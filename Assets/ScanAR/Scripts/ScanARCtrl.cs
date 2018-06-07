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

        //OSRDLL.OSROldIntegrate(OSRdata, ref curAddedScan, ref integratedVerts, ref integratedColors, ref integratedFaces);// later it will become vectors of the data for each mesh
        // TODO
        OSRDLL.OSRIntegrate(OSRdata, ref curAddedScan, ref splitIntVerts, ref splitIntColors, ref splitIntFaces);// later it will become vectors of the data for each mesh
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
    void LoadScan()
    {
        // check if the file exist
        if (isJustIssueScan && File.Exists(Utility.scanPath))
        {
            isJustIssueScan = false;

            GameObject newscan = GameObject.Instantiate(loader);
            // assign to secondTracker's child
            newscan.transform.parent = trackers.Find("secondTracker");
            // child's transform should be secT.inv * ST * ST2D, secT should be the transform when issued the scan
            Matrix4x4 curSecondTracker = Matrix4x4.TRS(newscan.transform.parent.transform.position, newscan.transform.parent.transform.rotation, Vector3.one);
            Transform scanTrackerTransform = trackers.Find("scanTracker");
            Matrix4x4 curScanTracker = Matrix4x4.TRS(scanTrackerTransform.position, scanTrackerTransform.rotation, Vector3.one);
            Matrix4x4 childMtx = curSecondTracker.inverse * curScanTracker * matrixST2David;
            newscan.transform.localPosition = childMtx.GetPosition();
            newscan.transform.localRotation = childMtx.GetRotation();
            print("newscan:" + newscan.transform.localPosition.ToString("F3") + "\t" + newscan.transform.localRotation.ToString("F3"));

            // load the ply
            float prevTime = Time.realtimeSinceStartup;
            newscan.GetComponent<UnityLoader>().LoadMeshesDirectly();
            float curTime = Time.realtimeSinceStartup;
            print("load meshes:" + (curTime - prevTime) + "s");

            newscan.GetComponent<UnityLoader>().AddScan();

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
