using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

    public static int[] indices;

    public static int limitCount = 65000;

    public static float downsample;

    public static void InitialIndices()
    {
        if(indices == null)
        {
            indices = new int[65000];
            for (int i = 0; i < 65000; i++)
            {
                indices[i] = i;
            }
        }
        
    }



    public static string calibrationFilePath = "D:\\Projects\\OnlineSurfaceReconstruction\\build_msvc14_64\\Calibration.aln";
    public static string calibrationIndicator = "scan.ply";
    public static string calibScanControllerIndicator = "scanControllerToDavid.ply";
    public static string calibScanTrackerIndicator = "scanTrackerToDavid.ply";
    public static string scanPath = "D:\\Scans\\currentScan.ply";
}
