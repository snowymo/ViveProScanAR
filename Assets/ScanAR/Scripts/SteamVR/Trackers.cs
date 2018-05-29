using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Trackers : MonoBehaviour {

    public int TrackerAmount;

    public GameObject secondTracker;

    List<GameObject> trackers;

	// Use this for initialization
	void Start () {
        trackers = new List<GameObject>();

    }
	
	// Update is called once per frame
	void Update () {
        
        identifySecondTracker();

        setupTrackers();
    }

    // identify two trackers
    void identifySecondTracker()
    {
        if (secondTracker != null)
            return;
        // choose the lowest as the second tracker
        float trackerHeight = 2f;
        int secondTrackerIdx = -1;
        for (int i = 0; i < trackers.Count; i++)
        {
            trackers[i].name = "scannerTracker";
            if(trackers[i].GetComponent<SteamVR_TrackedObject>().isValid)
            {
                if (trackerHeight > trackers[i].transform.position.y)
                {
                    trackerHeight = trackers[i].transform.position.y;
                    secondTrackerIdx = i;
                }
            } 
        }
        if(secondTrackerIdx != -1)
        {
            secondTracker = trackers[secondTrackerIdx];
            secondTracker.name = "secondTracker";
        }
        

    }
    // setup two trackers
    void setupTrackers()
    {
        if (trackers.Count > 0)
            return;
        // get the device index
        uint[] index = new uint[TrackerAmount];
        int idxIndex = 0;
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            if (result.ToString().Contains("tracker"))
            {
                index[idxIndex++] = i;
                if (idxIndex == TrackerAmount)
                    break;
            }
        }
        // create objs and attach them as children
        
        for (int i = 0; i < TrackerAmount; i++)
        {
            GameObject goTracker = new GameObject();
            goTracker.transform.parent = transform;
            SteamVR_TrackedObject theTracker = goTracker.AddComponent<SteamVR_TrackedObject>();
            theTracker.index = (SteamVR_TrackedObject.EIndex)index[i];
            trackers.Add(goTracker);
            // then check the height

        }
    }
}
