using System;
using System.Runtime.InteropServices;
using Vive.Plugin.SR;

public class ViveSR_DualCameraDepthExtra {

    private static IntPtr PtrDepthColliderAllData = IntPtr.Zero;
    private static uint SizeDepthColliderAllData;
    private static byte[] RawDepthColliderFrameIndex = new byte[sizeof(int)];
    private static byte[] RawDepthColliderTimeIndex = new byte[sizeof(int)];
    private static float[] PtrDepthColliderVertices;
    private static int[] PtrDepthColliderIndices;
    public static byte[] DepthColliderVerticesNum = new byte[sizeof(int)];
    public static byte[] DepthColliderIndicesNum = new byte[sizeof(int)];
    public static byte[] DepthColliderBytePervert = new byte[sizeof(int)];
    public static int ColliderVerticeNum = 0;
    public static int ColliderIndicesNum = 0;
    public static int ColliderBytePervert = 0;

    private static IntPtr PtrDepthColliderFrameInfo = IntPtr.Zero;
    private static uint SizeDepthColliderFrameInfo;

    public static int DepthColliderFrameIndex;
    public static int DepthColliderTimeIndex;
    public static bool ExportCollider = true;

    public static int InitialDepthCollider(int depthImageWidth, int depthImageHeight)
    {
        PtrDepthColliderVertices = new float[depthImageWidth * depthImageHeight * 3];
        PtrDepthColliderIndices = new int[depthImageWidth * depthImageHeight * 6];
        return (int)Error.WORK;
    }
    public static bool GetDepthColliderFrameInfo()
    {
        int result = (int)Error.FAILED;
        int mask = (int)DepthDataMask.FRAME_SEQ | (int)DepthDataMask.TIME_STP;
        if (PtrDepthColliderFrameInfo == IntPtr.Zero)
        {
            result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_DEPTH, ref SizeDepthColliderFrameInfo, mask);
            if (result == (int)Error.WORK)
            {
                PtrDepthColliderFrameInfo = Marshal.AllocCoTaskMem((int)SizeDepthColliderFrameInfo);
            }
        }
        if (PtrDepthColliderFrameInfo != IntPtr.Zero)
        {
            result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DEPTH, PtrDepthColliderFrameInfo, mask, SizeDepthColliderFrameInfo);
            if (result == (int)Error.WORK)
            {
                int startIndex = 0, length = 0;

                length = sizeof(int);
                Marshal.Copy(new IntPtr(PtrDepthColliderFrameInfo.ToInt64() + startIndex), RawDepthColliderFrameIndex, 0, length);

                startIndex += length;
                length = sizeof(int);
                Marshal.Copy(new IntPtr(PtrDepthColliderFrameInfo.ToInt64() + startIndex), RawDepthColliderTimeIndex, 0, length);

                DepthColliderFrameIndex = BitConverter.ToInt32(RawDepthColliderFrameIndex, 0);
                DepthColliderTimeIndex = BitConverter.ToInt32(RawDepthColliderTimeIndex, 0);
            }
        }
        else
        {
            return false;
        }
        return true;
    }
    public static bool GetDepthColliderData(ref int verticesNum, out float[] verticesBuff, ref int indicesNum, out int[] indicesBuff)
    {
        int result = (int)Error.FAILED;
        int mask = (int)DepthDataMask.NUM_VERTICES | (int)DepthDataMask.BYTEPERVERT | (int)DepthDataMask.VERTICES | (int)DepthDataMask.NUM_INDICES | (int)DepthDataMask.INDICES;
        if (PtrDepthColliderAllData == IntPtr.Zero)
        {
            result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_DEPTH, ref SizeDepthColliderAllData, mask);
            if (result == (int)Error.WORK)
            {
                PtrDepthColliderAllData = Marshal.AllocCoTaskMem((int)SizeDepthColliderAllData);

            }
        }
        if (PtrDepthColliderAllData != IntPtr.Zero)
        {
            result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DEPTH, PtrDepthColliderAllData, mask, SizeDepthColliderAllData);
            if (result == (int)Error.WORK)
            {
                int startIndex = 0, length = 0, part_length = 0;

                length = sizeof(int);
                Marshal.Copy(new IntPtr(PtrDepthColliderAllData.ToInt64() + startIndex), DepthColliderVerticesNum, 0, length);
                ColliderVerticeNum = BitConverter.ToInt32(DepthColliderVerticesNum, 0);

                startIndex += length;
                length = sizeof(int);
                Marshal.Copy(new IntPtr(PtrDepthColliderAllData.ToInt64() + startIndex), DepthColliderBytePervert, 0, length);
                ColliderBytePervert = BitConverter.ToInt32(DepthColliderBytePervert, 0);

                startIndex += length;
                length = ColliderBytePervert * 640 * 480;
                part_length = ColliderVerticeNum * ColliderBytePervert / 3;
                Marshal.Copy(new IntPtr(PtrDepthColliderAllData.ToInt64() + startIndex), PtrDepthColliderVertices, 0, part_length);

                startIndex += length;
                length = sizeof(int);
                Marshal.Copy(new IntPtr(PtrDepthColliderAllData.ToInt64() + startIndex), DepthColliderIndicesNum, 0, length);
                ColliderIndicesNum = BitConverter.ToInt32(DepthColliderIndicesNum, 0);

                startIndex += length;
                length = ColliderIndicesNum;
                Marshal.Copy(new IntPtr(PtrDepthColliderAllData.ToInt64() + startIndex), PtrDepthColliderIndices, 0, length);

                DepthColliderFrameIndex = BitConverter.ToInt32(RawDepthColliderFrameIndex, 0);
                DepthColliderTimeIndex = BitConverter.ToInt32(RawDepthColliderTimeIndex, 0);
            }
            else
            {
                ColliderVerticeNum = 0;
                ColliderIndicesNum = 0;
                ColliderBytePervert = 0;
            }
        }
        verticesNum = ColliderVerticeNum;
        indicesNum = ColliderIndicesNum;
        verticesBuff = PtrDepthColliderVertices;
        indicesBuff = PtrDepthColliderIndices;

        return true;
    }
}
