using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient;

    public client zmqMatrixClient;

    public Transform steamTracker, secondController;

    public GameObject loader;

    List<GameObject> scans;

    int packetId;

    public bool isJustIssueScan;

    // Use this for initialization
    void Start () {
        scans = new List<GameObject>();
        packetId = -1;
        Utility.InitialIndices();
        isJustIssueScan = false;
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
            newscan.transform.GetComponent<PLYPathLoader>().plyCoordType = PLYPathLoader.PLY_COORD.TEST;
            if (steamTracker.gameObject.GetComponent<Trackers>().secondTracker != null)
                newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker.gameObject.GetComponent<Trackers>().secondTracker.transform;
            if (secondController != null)
                newscan.transform.GetComponent<PLYPathLoader>().secondaryController = secondController;

            newscan.transform.GetComponent<PLYPathLoader>().LoadMeshesDirectly();
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
        dllHandle();
	}
}
