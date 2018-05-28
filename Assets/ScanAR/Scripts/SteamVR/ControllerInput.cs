using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.IO;

public class ControllerInput : MonoBehaviour {
    public GameObject[] hands;
    public GameObject scannerController, secondController;

    public ScanARCtrl scanARCtrl;

    SystemCommand sc;

    // Use this for initialization
    void Start () {
        sc = new SystemCommand();
       
	}
	
	// Update is called once per frame
	void Update () {

        identifyControllers();

        HandleEvents();

    }

    void identifyControllers()
    {
        if (scannerController != null)
            return; 
        if(hands.Length == 2)
        {
            Vector3 ctrlPos0 = hands[0].transform.position;
            Vector3 ctrlPos1 = hands[1].transform.position;

            if(ctrlPos0.y > ctrlPos1.y)
            {
                scannerController = hands[0];
                secondController = hands[1];
            }
            else
            {
                scannerController = hands[1];
                secondController = hands[0];
            }
        }
    }

    void HandleEvents()
    {
        if (scannerController == null
            || scannerController.GetComponent<Valve.VR.InteractionSystem.Hand>() == null)
            return;

        // issue scan with scannerController.trigger or secondController.application
        if (scannerController.GetComponent<Valve.VR.InteractionSystem.Hand>().controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)
            || secondController.GetComponent<Valve.VR.InteractionSystem.Hand>().controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            // remove that scanfile
            if (File.Exists(Utility.scanPath))
            {
                File.Delete(Utility.scanPath);
            }
            // mark I did a scan
            scanARCtrl.isJustIssueScan = true;
            sc.IssueScan();
        }
    }
}
