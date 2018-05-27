using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient;

    public client zmqMatrixClient;

    public Transform steamTracker;

    public GameObject loader;

    List<GameObject> scans;

    int packetId;

    // Use this for initialization
    void Start () {
        scans = new List<GameObject>();
        packetId = -1;
        Utility.InitialIndices();
    }
	
	// Update is called once per frame
	void Update () {
		if(zmqMeshClient.bNewMsg)
        {
            if(zmqMeshClient.msgType == client.MsgType.POINTS)
            {
                // then we need to have a matrix for T->D
                if (!zmqMatrixClient.bNewMsg)
                    return;
            }
            // check packet id, they should be adjacent and larger than current one
            if ((zmqMeshClient.currentId> packetId) || (zmqMeshClient.currentId <= 1))
            {
                packetId = zmqMeshClient.currentId;

                GameObject newscan = GameObject.Instantiate(loader);
                newscan.transform.parent = transform;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMeshClient = zmqMeshClient;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMatrixClient = zmqMatrixClient;
                newscan.transform.GetComponent<PLYPathLoader>().LoadMeshes();
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

                if(zmqMeshClient.msgType == client.MsgType.MESHES)
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
}
