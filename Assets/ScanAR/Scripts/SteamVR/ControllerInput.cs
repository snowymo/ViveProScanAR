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

            scanARCtrl.secondController = secondController.transform;
            scanARCtrl.scanController = scannerController.transform;
        }
    }

    void HandleEvents()
    {
        if (scannerController == null
            || scannerController.GetComponent<Valve.VR.InteractionSystem.Hand>() == null)
            return;

        Valve.VR.InteractionSystem.Hand scannerhand = scannerController.GetComponent<Valve.VR.InteractionSystem.Hand>();
        Valve.VR.InteractionSystem.Hand secondhand = secondController.GetComponent<Valve.VR.InteractionSystem.Hand>();
        // issue scan with scannerController.trigger or secondController.application
        if ((Input.GetKeyDown(KeyCode.Space)) || (scannerhand != null && scannerhand.controller != null && scannerhand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            || (
            (secondhand != null && secondhand.controller != null
                        && secondhand.controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)))
            )
        {
            print("issue a scan");
            // remove that scanfile
            if (File.Exists(Utility.scanPath))
            {
                File.Delete(Utility.scanPath);
            }
            // mark I did a scan
            scanARCtrl.isJustIssueScan = true;
            sc.IssueScan();
        }

        // for integration
        if ((Input.GetKeyDown(KeyCode.I))||(
           secondhand != null && secondhand.controller != null
                      && secondhand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
           )
        {
            print("integrate a scan");
            // second controller trigger for integration
            scanARCtrl.IntegrateScan();
        }

        // for register
        if ((Input.GetKeyDown(KeyCode.R)) || (
         secondhand != null && secondhand.controller != null
                  && secondhand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
         )
        {
            print("register a scan");
            // second controller touchpad for register
            scanARCtrl.RegisterScan();
        }


    }
}
