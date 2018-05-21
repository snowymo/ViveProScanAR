using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient, zmqMatrixClient;

    public Transform steamTracker;

    public GameObject loader;

    List<GameObject> scans;

    // Use this for initialization
    void Start () {
        scans = new List<GameObject>();

    }
	
	// Update is called once per frame
	void Update () {
		if(zmqMatrixClient.bNewMsg && zmqMeshClient.bNewMsg)
        {
            zmqMatrixClient.bNewMsg = false;
            zmqMeshClient.bNewMsg = false;
            
            GameObject newscan = GameObject.Instantiate(loader);
            newscan.transform.parent = transform;
            newscan.transform.GetComponent<PLYPathLoader>().zmqMeshClient = zmqMeshClient;
            newscan.transform.GetComponent<PLYPathLoader>().zmqMatrixClient = zmqMatrixClient;
            newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker;
            newscan.transform.GetComponent<PLYPathLoader>().LoadMatrix();
            newscan.transform.GetComponent<PLYPathLoader>().LoadMesh();
            scans.Add(newscan);
        }
	}
}
