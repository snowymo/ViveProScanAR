using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

    public static int[] indices;

    public static int limitCount = 65000;

    public static float downsample;

    public static void InitialIndices()
    {
        indices = new int[65000];
        for (int i = 0; i < 65000; i++)
        {
            indices[i] = i;
        }
    }
}
