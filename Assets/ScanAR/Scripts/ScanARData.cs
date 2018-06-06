using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanARData {

    // to control ALLLL information here. SAD
    public class MESHDATA
    {
        public Vector3[] vertices;
        public List<Vector3[]> verticesPieces;
        public Color32[] colors;
        public List<Color32[]> colorsPieces;    // for integrated
        public PlyLoaderDll.LABCOLOR[] labColors;// for david and vive
        public uint[] faces;
        public List<uint[]> facesPieces;// for integrated

        public MESHDATA()
        {
            vertices = new Vector3[0];
            verticesPieces = new List<Vector3[]>();
            colors = new Color32[0];
            colorsPieces = new List<Color32[]>();
            labColors = new PlyLoaderDll.LABCOLOR[0];
            facesPieces = new List<uint[]>();
            faces = new uint[0];
        }
    }

    public enum ScanState { DAVIDSYSTEM, VIVESYSTEM, INTEG};

    public ScanState curState;

    public MESHDATA[] curData;

    public Matrix4x4 registerMtx;

    public void AfterRegister()
    {
        // right hand to left hand
        registerMtx[0, 2] *= -1f;
        registerMtx[1, 2] *= -1f;
        registerMtx[2, 0] *= -1f;
        registerMtx[2, 1] *= -1f;
        registerMtx[2, 3] *= -1f;

        // apply registerMtx to all vertices;
        for (int i = 0; i < curData[(int)curState].verticesPieces.Count; i++)
        {
            for(int j = 0; j < curData[(int)curState].verticesPieces[i].Length; j++)
            {
                curData[(int)curState].verticesPieces[i][j] = registerMtx.MultiplyPoint(curData[(int)curState].verticesPieces[i][j]);
            }
        }

        registerMtx = Matrix4x4.identity;
    }

    public ScanARData()
    {
        curState = ScanState.DAVIDSYSTEM;
        curData = new MESHDATA[3];
        for(int i = 0; i < curData.Length; i++)
        {
            curData[i] = new MESHDATA();
        }
        registerMtx = Matrix4x4.identity;
    }

    public void SetData(Vector3[] rawScanVertices, Color32[] rawScanColors, PlyLoaderDll.LABCOLOR[] rawScanLabColors, uint[] rawScanFaces)
    {
        curData[(int)curState].vertices = rawScanVertices;
        curData[(int)curState].colors = rawScanColors;
        curData[(int)curState].labColors = rawScanLabColors;
        curData[(int)curState].faces = rawScanFaces;
    }

    public void SetDavidToViveTransform(Matrix4x4 matrixST2David, Matrix4x4 curScanTracker)
    {
        curData[(int)curState].vertices = curData[(int)ScanState.DAVIDSYSTEM].vertices;
        curData[(int)curState].colors = curData[(int)ScanState.DAVIDSYSTEM].colors;
        curData[(int)curState].labColors = curData[(int)ScanState.DAVIDSYSTEM].labColors;
        curData[(int)curState].faces = curData[(int)ScanState.DAVIDSYSTEM].faces;

        for (int i = 0; i < curData[(int)curState].vertices.Length; i++)
        {
            curData[(int)curState].vertices[i] *= 0.001f;
            curData[(int)curState].vertices[i].z *= -1f;
            curData[(int)curState].vertices[i] = (curScanTracker * matrixST2David).MultiplyPoint(curData[(int)curState].vertices[i]);
        }
    }

    public MESHDATA getCurrentMeshData()
    {
        return curData[(int)curState];
    }
}
