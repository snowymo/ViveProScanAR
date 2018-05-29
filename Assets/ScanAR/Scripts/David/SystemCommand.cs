using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class SystemCommand {

    public void IssueScan()
    {
        string autoItPath = "\"D:\\Program Files (x86)\\AutoIt3\\AutoIt3.exe\"";
        string autoFilePath = "Assets\\ScanAR\\Scripts\\David\\TakeDavidScan.au3";
        try
        {
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = autoItPath;

            myProcess.StartInfo.Arguments = autoFilePath;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit();
            int ExitCode = myProcess.ExitCode;
            //print(ExitCode);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }
}
