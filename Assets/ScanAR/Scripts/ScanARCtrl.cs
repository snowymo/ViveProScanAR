using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient, zmqMatrixClient;

    public Transform steamTracker;

    public GameObject loader;

    List<GameObject> scans;

    int packetId;

    // Use this for initialization
    void Start () {
        scans = new List<GameObject>();
        packetId = -1;
    }
	
	// Update is called once per frame
	void Update () {
		if(zmqMatrixClient.bNewMsg && zmqMeshClient.bNewMsg)
        {


            // check packet id, they should be adjacent and larger than current one
            if (((Mathf.Abs( zmqMatrixClient.currentId - zmqMeshClient.currentId) == 1) || (Mathf.Abs(zmqMatrixClient.currentId - zmqMeshClient.currentId) == 9))
                && (Mathf.Max( zmqMatrixClient.currentId ,zmqMeshClient.currentId )> packetId))
            {
                GameObject newscan = GameObject.Instantiate(loader);
                newscan.transform.parent = transform;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMeshClient = zmqMeshClient;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMatrixClient = zmqMatrixClient;
                newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker;
                newscan.transform.GetComponent<PLYPathLoader>().LoadMatrix();
                newscan.transform.GetComponent<PLYPathLoader>().LoadMeshes();

                // we dont need the old one
                if (scans.Count > 0)
                    scans[scans.Count - 1].SetActive(false);

                scans.Add(newscan);
                
                zmqMatrixClient.bNewMsg = false;
                zmqMeshClient.bNewMsg = false;

                packetId = Mathf.Max(zmqMatrixClient.currentId, zmqMeshClient.currentId);
            }            
        }
	}
}
