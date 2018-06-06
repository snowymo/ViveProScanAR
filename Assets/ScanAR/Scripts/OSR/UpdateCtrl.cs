using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class UpdateCtrl : MonoBehaviour {

    public Shader shader;

    // read corresponding ST->D matrix, ST matrix when scanning
    public Matrix4x4 matrixST2David;
    public Matrix4x4 matrixST, matrixSecT;

    // update every frame
    public Matrix4x4 curSecondTracker;
    public Matrix4x4 curScanTracker; // normally not moving quite a lot

    public SteamVR_TrackedObject secondTracker, scanTracker;
    public string pathMatrixST;

    // initial vive system info
    public Vector3[] iniScanVertices;
    public List<Vector3[]> iniScanVerticesPieces;
    public Color32[] iniScanColors;
    public PlyLoaderDll.LABCOLOR[] iniScanLabColors;
    public uint[] iniScanFaces;

    public List<GameObject> updateGOs;

    public Transform parentTracker;

    public ScanARData curInstance;

    public void AddDavidData(Vector3[] rawScanVertices,Color32[] rawScanColors, PlyLoaderDll.LABCOLOR[] rawScanLabColors, uint[] rawScanFaces)
    {
        if(curInstance == null)
            curInstance = new ScanARData();
        curInstance.curState = ScanARData.ScanState.DAVIDSYSTEM;
        curInstance.SetData(rawScanVertices, rawScanColors, rawScanLabColors, rawScanFaces);

        int meshCount = rawScanVertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            Utility.createMesh(i, Math.Min(Utility.limitCount, rawScanVertices.Length - i * Utility.limitCount), ref rawScanVertices, ref rawScanColors, 
                ref updateGOs, transform, shader, ref curInstance.curData[(int)curInstance.curState].verticesPieces);
        }
    }

    // Use this for initialization
    void Start () {
        loadSTnSecT();
        loadST2David();
        // get new initial data in vive system scannerTracker * ST2D matrix * verts
        iniScanColors = new Color32[GetComponent<TestRplyLoader>().rawScanColors.Length];
        Array.Copy(GetComponent<TestRplyLoader>().rawScanColors, iniScanColors, GetComponent<TestRplyLoader>().rawScanColors.Length);
        iniScanFaces = new uint[GetComponent<TestRplyLoader>().rawScanFaces.Length];
        Array.Copy(GetComponent<TestRplyLoader>().rawScanFaces, iniScanFaces, GetComponent<TestRplyLoader>().rawScanFaces.Length);
        iniScanLabColors = new PlyLoaderDll.LABCOLOR[GetComponent<TestRplyLoader>().rawScanLabColors.Length];
        Array.Copy(GetComponent<TestRplyLoader>().rawScanLabColors, iniScanLabColors, GetComponent<TestRplyLoader>().rawScanLabColors.Length);
        iniScanVertices = new Vector3[GetComponent<TestRplyLoader>().rawScanVertices.Length];
        Array.Copy(GetComponent<TestRplyLoader>().rawScanVertices, iniScanVertices, GetComponent<TestRplyLoader>().rawScanVertices.Length);

        updateGOs = new List<GameObject>();
        iniScanVerticesPieces = new List<Vector3[]>();
        if(curInstance == null)
        curInstance = new ScanARData();
    }

    public void TurnToViveSystem()
    {
        curScanTracker = Matrix4x4.TRS(scanTracker.gameObject.transform.position, scanTracker.gameObject.transform.rotation, Vector3.one);
        print("curScanTracker:" + curScanTracker.ToString("F3"));
        print("matrixST2David:" + matrixST2David.ToString("F3"));
        float prevTime = Time.realtimeSinceStartup;
        //         for (int i = 0; i < iniScanVertices.Length; i++)
        //         {
        //             iniScanVertices[i] *= 0.001f;
        //             iniScanVertices[i].z *= -1f;
        //             iniScanVertices[i] = (curScanTracker * matrixST2David).MultiplyPoint(iniScanVertices[i]);
        //         }
        curInstance.curState = ScanARData.ScanState.VIVESYSTEM;
        curInstance.SetDavidToViveTransform(matrixST2David, curScanTracker);
        float curTime = Time.realtimeSinceStartup;
        print("normal for loop took " + (curTime - prevTime) + "s");
//         prevTime = Time.realtimeSinceStartup;
//         Parallel.For(0, iniScanVertices.Length, (i) =>
//         {
//             iniScanVertices[i] *= 0.001f;
//             iniScanVertices[i].z *= -1f;
//             iniScanVertices[i] = (curScanTracker * matrixST2David).MultiplyPoint(iniScanVertices[i]);
//         });
//         curTime = Time.realtimeSinceStartup;
//         print("parallel for loop took " + (curTime - prevTime) + "s");
        
        // create mesh based on the ini data
        int meshCount = curInstance.curData[(int)curInstance.curState].vertices.Length / Utility.limitCount + 1;
        for (int i = 0; i < meshCount; i++)
        {
            Utility.createMesh(i, Math.Min(Utility.limitCount, iniScanVertices.Length - i * Utility.limitCount), ref curInstance, ref updateGOs, parentTracker, shader);
            
        }
        print("the amount of mesh is " + updateGOs.Count);
    }

    
	// Update is called once per frame
	void Update () {
        curSecondTracker = Matrix4x4.TRS(secondTracker.gameObject.transform.position, secondTracker.gameObject.transform.rotation, Vector3.one);
        Matrix4x4 diff = curSecondTracker * matrixSecT.inverse;
        //print("diff:" + diff.GetPosition().ToString("F3") + "\t" + diff.GetRotation().ToString("F3"));
        // update meshes here based on curSecTracker and iniSecTracker
        // loop the gos, to get the original obj.mesh.verts
        
        if (curInstance == null)
            return;
        if(updateGOs.Count != curInstance.curData[(int)curInstance.curState].verticesPieces.Count)
        {
//             print("amount of game objects:" + updateGOs.Count);
//             print("amount of vertex pieces:" + curInstance.curData[(int)curInstance.curState].verticesPieces.Count);
            return;
        }
        //Matrix4x4 diffreg = diff * curInstance.registerMtx;
//         for (int i = 0; i < updateGOs.Count; i++)
//         {
//             Mesh mesh = updateGOs[i].GetComponent<MeshFilter>().mesh;
//             Vector3[] verts = mesh.vertices;
//             Vector3[] curRawVerts = curInstance.curData[(int)curInstance.curState].verticesPieces[i];
//             for (int j = 0; j < curRawVerts.Length; j++)
//             {
//                 verts[j] = diff.MultiplyPoint(curRawVerts[j]);
//                 //verts[i] = Vector3.zero;
//             }
//             mesh.vertices = verts;
//         }
    }

    public void RecordSTMatrix()
    {
        if (!File.Exists(pathMatrixST))
        {
            File.CreateText(pathMatrixST).Dispose();
        }

        using (TextWriter writer = new StreamWriter(pathMatrixST, false))
        {
            matrixST = Matrix4x4.TRS(scanTracker.gameObject.transform.position, scanTracker.gameObject.transform.rotation, Vector3.one);
            //StreamWriter writer = new StreamWriter(pathMatrixST, true);
            writer.WriteLine(matrixST.ToString("F4"));

            Matrix4x4 iniSecTracker = Matrix4x4.TRS(secondTracker.gameObject.transform.position, secondTracker.gameObject.transform.rotation, Vector3.one);
            //StreamWriter writer = new StreamWriter(pathMatrixST, true);
            writer.WriteLine(iniSecTracker.ToString("F4"));
            writer.Close();
        }
    }

    void loadSTnSecT()
    {
        if (pathMatrixST != null)
        {
            if (File.Exists(pathMatrixST))
            {
                // check after I wrote the file down
                StreamReader reader = new StreamReader(pathMatrixST);
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    string line = reader.ReadLine();
                    string[] row = line.Split(new string[] { " ", "\t" }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < row.Length; colIdx++)
                        matrixST[rowIdx, colIdx] = float.Parse(row[colIdx]);
                }
                reader.ReadLine();
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    string line = reader.ReadLine();
                    string[] row = line.Split(new string[] { " ", "\t" }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < row.Length; colIdx++)
                        matrixSecT[rowIdx, colIdx] = float.Parse(row[colIdx]);
                }
                reader.Close();
                //matrixST = Matrix4x4.TRS(secondTracker.position, secondTracker.rotation, Vector3.one);
                print("matrixSecT:" + matrixSecT.ToString("F3"));
            }
        }      
    }

    void loadST2David()
    {
        StreamReader reader = new StreamReader(Utility.calibrationFilePath);
        string line;
        // Read and display lines from the file until the end of the file is reached.
        while ((line = reader.ReadLine()) != null)
        {
             if (line.Contains(Utility.calibScanTrackerIndicator))
            {
                line = reader.ReadLine();//#
                for (int rowIdx = 0; rowIdx < 4; rowIdx++)
                {
                    line = reader.ReadLine(); // first line
                    string[] firstRow = line.Split(new string[] { " " }, StringSplitOptions.None);
                    for (int colIdx = 0; colIdx < firstRow.Length; colIdx++)
                        matrixST2David[rowIdx, colIdx] = float.Parse(firstRow[colIdx]);
                }
            }
        }
        reader.Close();

        // from david scale to unity scale
        for (int i = 0; i < 3; i++)
        {
            matrixST2David[i, 3] /= 1000f;
        }
        // right hand to left hand
        matrixST2David[0, 2] *= -1f;
        matrixST2David[1, 2] *= -1f;
        matrixST2David[2, 0] *= -1f;
        matrixST2David[2, 1] *= -1f;
        matrixST2David[2, 3] *= -1f;   
    }
}
