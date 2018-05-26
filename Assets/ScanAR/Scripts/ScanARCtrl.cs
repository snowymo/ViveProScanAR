using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanARCtrl : MonoBehaviour {

    public client zmqMeshClient;

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
            // check packet id, they should be adjacent and larger than current one
            if ((zmqMeshClient.currentId> packetId) || (zmqMeshClient.currentId <= 1))
            {
                packetId = zmqMeshClient.currentId;

                GameObject newscan = GameObject.Instantiate(loader);
                newscan.transform.parent = transform;
                newscan.transform.GetComponent<PLYPathLoader>().zmqMeshClient = zmqMeshClient;
                newscan.transform.GetComponent<PLYPathLoader>().LoadMeshes();
                if (steamTracker != null)
                    newscan.transform.GetComponent<PLYPathLoader>().steamTracker = steamTracker;
                

                // we dont need the old one
                if (scans.Count > 0)
                    scans[scans.Count - 1].SetActive(false);

                scans.Add(newscan);
                
                zmqMeshClient.bNewMsg = false;

                
            }            
        }
	}
}
