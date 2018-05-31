using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class PosTracking : MonoBehaviour {

    public Transform vivecontroller1, vivetracker1, vivetracker2;

    [DllImport("svd", EntryPoint ="Calib")]
    public static extern void Calib(float[] a, float[] b, int len, float[] transform);

    Vector3 vcPos, vtPos, vtPos2;
    Vector3 vcNewPos, vtNewPos, vtNewPos2;

    Quaternion vcRot, vtRot1, vtRot2;

    List<Vector3> pointSetA, pointSetB;

    public Matrix4x4 rigidTransform;
    public Vector3 rigidTranslation;
    public Quaternion rigidRotation;
    public float rigidAngle, vcAngle, vtAngle1, vtAngle2;

    bool isBegin;
    // Use this for initialization
    void Start () {
        isBegin = false;
        pointSetA = new List<Vector3>();
        pointSetB = new List<Vector3>();
        rigidTransform = Matrix4x4.identity;
    }

    float[] getFloatArray(List<Vector3> vs)
    {
        float[] f = new float[3*vs.Count];
        for(int i = 0; i < f.Length; i++)
        {
            f[i] = vs[i/3][i%3];
        }
        return f;
    }

    float[] getFloatArray(Matrix4x4 m)
    {
        float[] f = new float[16];
        for (int i = 0; i < f.Length; i++)
        {
            f[i] = m[i % 4, i / 4];
        }
        return f;
    }

    void toMatrix(float[] f, ref Matrix4x4 m)
    {
        for (int i = 0; i < 16; i++)
        {
            m[i % 4, i / 4] = f[i];
        }
    }

   float CalculateAngle(Quaternion q1, Quaternion q2)
    {
        return Quaternion.Angle(q1, q2);
    }

    // Update is called once per frame
    void Update () {

        if (!isBegin)
        {
            // record the beginning place as the first point sets
            if (vivetracker1.GetComponent<SteamVR_TrackedObject>().isValid)
            {
                vcPos = vivecontroller1.position;
                vtPos = vivetracker1.position;
                vtPos2 = vivetracker2.position;

                pointSetA.Add(vcPos);
                pointSetA.Add(vtPos);
                pointSetA.Add(vtPos2);

                vcRot = vivecontroller1.rotation;
                vtRot1 = vivetracker1.rotation;
                vtRot2 = vivetracker2.rotation;

                isBegin = true;
            }
        }

        pointSetB.Clear();
        // get current position pair
        vcNewPos = vivecontroller1.position;
        vtNewPos = vivetracker1.position;
        vtNewPos2 = vivetracker2.position;
        pointSetB.Add(vcNewPos);
        pointSetB.Add(vtNewPos);
        pointSetB.Add(vtNewPos2);
        // apply rigid transformation
        float[] fTransformation = new float[16];
        Calib(getFloatArray(pointSetA), getFloatArray(pointSetB), pointSetB.Count*3, fTransformation);
        toMatrix(fTransformation, ref rigidTransform);
        rigidTranslation = rigidTransform.GetPosition();
        rigidRotation = rigidTransform.GetRotation();
        rigidAngle = Quaternion.Angle(rigidRotation, Quaternion.identity);

        vcAngle = CalculateAngle(vcRot, vivecontroller1.rotation);
        vtAngle1 = CalculateAngle(vtRot1, vivetracker1.rotation);
        vtAngle2 = CalculateAngle(vtRot2, vivetracker2.rotation);
    }
}
