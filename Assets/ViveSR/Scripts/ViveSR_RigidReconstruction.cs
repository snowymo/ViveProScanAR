using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Vive.Plugin.SR
{
    //public static class ViveSR_RigidReconstructionConfig
    //{
    //    public static ReconstructionDataSource ReconstructionDataSource = ReconstructionDataSource.DATASET;
    //    public static uint NumDatasetFrame = 0;
    //    public static string DatasetPath = "";
    //    public static ReconstructionQuality Quality = ReconstructionQuality.MID;
    //    public static bool ExportCollider = false;
    //}
    public class ViveSR_RigidReconstruction
    {
        public delegate void Callback(int numFrame, IntPtr poseMtx, IntPtr vertData, int numVert, int vertStide, IntPtr idxData, int numIdx);
        public delegate void ExportProgressCallback(int stage, int percentage);

        private static IntPtr PtrPointCloudAllData;
        private static byte[] RawPointCloudAllData;
        private static byte[] RawPointCloudFrameIndex = new byte[sizeof(int)];
        private static byte[] RawPointCloudVerticeNum = new byte[sizeof(int)];
        private static byte[] RawPointCloudIndicesNum = new byte[sizeof(int)];
        private static byte[] RawPointCloudBytePerVetex = new byte[sizeof(int)];

        //private static IntPtr PtrPoseMtx;
        private static float[] OutVertex;
        private static int[] OutIndex;
        private static float[] TrackedPose;
        //private static float[] ExternalPose;
        private static int ExportStage;
        private static int ExportPercentage;
        private static int FrameSeq;
        private static int VertNum;
        private static int IdxNum;
        private static int VertStrideInByte;
        private static bool UsingCallback = false;

        public static bool ExportAdaptiveMesh { get; set; }
        public static float ExportAdaptiveMaxGridSize { get; set; }
        public static float ExportAdaptiveMinGridSize { get; set; }
        public static float ExportAdaptiveErrorThres { get; set; }
        public static float LiveAdaptiveMaxGridSize { get; set; }
        public static float LiveAdaptiveMinGridSize { get; set; }
        public static float LiveAdaptiveErrorThres { get; set; }
        public static bool IsScanning { get; private set; }
        public static bool IsExporting { get; private set; }

        public static bool InitRigidReconstructionParamFromFile(string configFile)
        {
            return ViveSR_Framework.SetParameterString(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_FILEPATH, configFile) == (int)Error.WORK;
        }

        //public static void InitRigidReconstructionParam()
        //{
        //    this function is not called in current version, keep this API on, we can allow user to adjust some default setting
        //    ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_DATA_SOURCE, (int)ViveSR_RigidReconstructionConfig.ReconstructionDataSource);
        //    ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_DATASET_FRAME_NUM, (int)ViveSR_RigidReconstructionConfig.NumDatasetFrame);
        //    ViveSR_Framework.SetParameterString(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_DATASET_PATH, ViveSR_RigidReconstructionConfig.DatasetPath);
        //    ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_EXPORT_COLLIDER, ViveSR_RigidReconstructionConfig.ExportCollider);
        //    ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_EXPORT_TEXTURE, true);
        //    ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.CONFIG_QUALITY, (int)ViveSR_RigidReconstructionConfig.Quality);
        //}

        public static int GetRigidReconstructionIntParameter(int type)
        {
            int ret = -1;

            if (ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, type, ref ret) != (int)Error.WORK)
                Debug.Log("[ViveSR] [RigidReconstruction] GetRigidReconstructionIntParameter Failed");

            return ret;
        }

        public static void AllocOutputDataMemory()
        {
            OutVertex = new float[8 * 2500000];
            OutIndex = new int[2500000];
            TrackedPose = new float[16];
            //ExternalPose = new float[16];

            //PtrPoseMtx = Marshal.AllocCoTaskMem(sizeof(float) * 16);       // matrix 44
            Debug.Log("[ViveSR] [RigidReconstruction] AllocOutputMemory Done");

            ExportAdaptiveMesh = false;
            LiveAdaptiveMaxGridSize = ExportAdaptiveMaxGridSize = 64;
            LiveAdaptiveMinGridSize = ExportAdaptiveMinGridSize = 4;
            LiveAdaptiveErrorThres  = ExportAdaptiveErrorThres = 0.4f;
        }

        private static void AllocFullMarshalMemPtr()
        {
            int result = (int)Error.FAILED;
            int mask = (int)ReconstructionDataMask.FRAME_SEQ | (int)ReconstructionDataMask.POSEMTX44 |
                       (int)ReconstructionDataMask.NUM_VERTICES | (int)ReconstructionDataMask.BYTEPERVERT | (int)ReconstructionDataMask.VERTICES |
                       (int)ReconstructionDataMask.NUM_INDICES | (int)ReconstructionDataMask.INDICES;
            uint size = 0;
            if (PtrPointCloudAllData == IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, ref size, mask);
                if (result == (int)Error.WORK)
                {
                    PtrPointCloudAllData = Marshal.AllocCoTaskMem((int)size);
                    RawPointCloudAllData = new byte[(int)size];
                }
            }
        }

        public static bool GetRigidReconstructionFrame(ref int frame)
        {
            int result = (int)Error.FAILED;
            if (!UsingCallback)
            {
                if (PtrPointCloudAllData == IntPtr.Zero)
                {
                    AllocFullMarshalMemPtr();
                }
                if (PtrPointCloudAllData != IntPtr.Zero)
                {
                    result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, PtrPointCloudAllData, (int)ReconstructionDataMask.FRAME_SEQ, (uint)RawPointCloudAllData.Length);
                    if (result == (int)Error.WORK)
                    {
                        Marshal.Copy(PtrPointCloudAllData, RawPointCloudFrameIndex, 0, sizeof(int));
                        FrameSeq = BitConverter.ToInt32(RawPointCloudFrameIndex, 0);
                    }
                }
            }

            frame = FrameSeq;
            return (result == (int)Error.WORK);
        }

        public static bool GetRigidReconstructionData(ref int frame, out float[] pose, ref int verticesNum, out float[] verticesBuff, ref int vertStrideInFloat, ref int indicesNum, out int[] indicesBuff)
        {
            if (!UsingCallback)
            {
                int result = (int)Error.FAILED;
                int mask = (int)ReconstructionDataMask.FRAME_SEQ | (int)ReconstructionDataMask.POSEMTX44 | 
                           (int)ReconstructionDataMask.NUM_VERTICES | (int)ReconstructionDataMask.BYTEPERVERT | (int)ReconstructionDataMask.VERTICES |
                           (int)ReconstructionDataMask.NUM_INDICES | (int)ReconstructionDataMask.INDICES;
                if (PtrPointCloudAllData == IntPtr.Zero)
                {
                    AllocFullMarshalMemPtr();
                }
                if (PtrPointCloudAllData != IntPtr.Zero)
                {
                    result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, PtrPointCloudAllData, mask, (uint)RawPointCloudAllData.Length);
                    if (result == (int)Error.WORK)
                    {
                        int startIndex = 0, length = 0; 
                        length = sizeof(int);
                        Marshal.Copy(PtrPointCloudAllData, RawPointCloudFrameIndex, 0, length);
                        FrameSeq = BitConverter.ToInt32(RawPointCloudFrameIndex, 0);    // frame seq

                        startIndex += length;
                        length = sizeof(float) * TrackedPose.Length;
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), TrackedPose, 0, TrackedPose.Length);  // tracked pose // copy how many float

                        startIndex += length;
                        length = sizeof(int);
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), RawPointCloudVerticeNum, 0, length);  // num vert // copy how many byte
                        VertNum = BitConverter.ToInt32(RawPointCloudVerticeNum, 0);

                        startIndex += length;
                        length = sizeof(int);
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), RawPointCloudBytePerVetex, 0, length);  // vertex size    // copy how many byte
                        VertStrideInByte = BitConverter.ToInt32(RawPointCloudBytePerVetex, 0);

                        startIndex += length;
                        length = VertNum * VertStrideInByte;
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), OutVertex, 0, (length / 4));  // vertex data    // copy how many float

                        startIndex += length;
                        length = sizeof(int);
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), RawPointCloudIndicesNum, 0, length);  // num index    // copy how many byte
                        IdxNum = BitConverter.ToInt32(RawPointCloudIndicesNum, 0);

                        startIndex += length;
                        length = sizeof(int) * IdxNum;
                        Marshal.Copy(new IntPtr(PtrPointCloudAllData.ToInt64() + startIndex), OutIndex, 0, length); // index data
                    }
                }
            }

            verticesNum = VertNum;
            indicesNum = IdxNum;
            frame = FrameSeq;
            vertStrideInFloat = VertStrideInByte / 4;
            verticesBuff = OutVertex;
            indicesBuff = OutIndex;
            pose = TrackedPose;

            //if (verticesNum == 0) Debug.Log("Not Update");
            return (verticesNum > 0);   // verticeNum == 0 --> Mesh is not updated
        }

        public static int RegisterReconstructionCallback()
        {
            UsingCallback = true;
            return ViveSR_Framework.RegisterCallback(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionCallback.DATA, Marshal.GetFunctionPointerForDelegate((Callback)ReconstructionDataCallback));
        }

        public static int UnregisterReconstructionCallback()
        {
            UsingCallback = false;
            return ViveSR_Framework.UnregisterCallback(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionCallback.DATA, Marshal.GetFunctionPointerForDelegate((Callback)ReconstructionDataCallback));
        }

       private static void ReconstructionDataCallback(int numFrame, IntPtr poseMtx, IntPtr vertData, int numVert, int vertStride, IntPtr idxData, int numIdx)
        {
            VertNum = numVert;
            FrameSeq = numFrame;
            VertStrideInByte = vertStride;
            if (numVert > 0)
            {
                Marshal.Copy(vertData, OutVertex, 0, numVert * (VertStrideInByte / 4));
                Marshal.Copy(poseMtx, TrackedPose, 0, 16);
            }
        }

        public static void ExportModel(string filename)
        {
            ExportStage = 0;
            ExportPercentage = 0;
            IsExporting = true;

            ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.EXPORT_ADAPTIVE_MODEL, ExportAdaptiveMesh);
            if (ExportAdaptiveMesh)
            {
                ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_MAX_GRID, ExportAdaptiveMaxGridSize * 0.01f);   // cm to m
                ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_MIN_GRID, ExportAdaptiveMinGridSize * 0.01f);
                ViveSR_Framework.SetParameterFloat(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionParam.ADAPTIVE_ERROR_THRES, ExportAdaptiveErrorThres);
            }           
            ViveSR_Framework.RegisterCallback(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)ReconstructionCallback.EXPORT_PROGRESS, Marshal.GetFunctionPointerForDelegate((ExportProgressCallback)UpdateExportProgress));
            ViveSR_Framework.SetCommandString(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionCmd.EXPORT_MODEL_FOR_UNITY), filename);
        }

        private static void UpdateExportProgress(int stage, int percentage)
        {
            if (stage == (int)ReconstructionExportStage.STAGE_EXTRACTING_MODEL) ExportStage = 0;
            else if (stage == (int)ReconstructionExportStage.STAGE_COMPACTING_TEXTURE) ExportStage = 1;
            else if (stage == (int)ReconstructionExportStage.STAGE_EXTRACTING_COLLIDER) ExportStage = 2;
            else if (stage == (int)ReconstructionExportStage.STAGE_SAVING_MODEL_FILE) ExportStage = 3;
            ExportPercentage = percentage;

            //if (stage == (int)ReconstructionExportStage.STAGE_EXTRACTING_MODEL)
            //    Debug.Log("Extracting Model: " + percentage + "%");
            //else if (stage == (int)ReconstructionExportStage.STAGE_COMPACTING_TEXTURE)
            //    Debug.Log("Compacting Textures: " + percentage + "%");
            //else if (stage == (int)ReconstructionExportStage.STAGE_EXTRACTING_COLLIDER)
            //    Debug.Log("Extracting Collider: " + percentage + "%");
            //else if (stage == (int)ReconstructionExportStage.STAGE_SAVING_MODEL_FILE)
            //    Debug.Log("Saving Model: " + percentage + "%");

            if (ExportStage == 3 && ExportPercentage == 100)
            {
                StopScanning();
                Debug.Log("[ViveSR] [RigidReconstruction] Finish Exporting");
            }
        }

        public static void GetExportProgress(ref int stage, ref int percentage)
        {
            stage = ExportStage;
            percentage = ExportPercentage;
        }

        public static void GetExportProgress(ref int percentage)
        {
            percentage = ExportStage * 25 + (int)(ExportPercentage * 0.25f);
        }

        public static void EnableLiveMeshExtraction(bool enable)
        {
            ViveSR_Framework.SetCommandBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionCmd.EXTRACT_POINT_CLOUD), enable);
        }

        public static void SetLiveMeshExtractionMode(ReconstructionLiveMeshExtractMode mode)
        {
            ViveSR_Framework.SetCommandInt(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionCmd.EXTRACT_VERTEX_NORMAL), (int)mode);
        }

        public static void StartScanning()
        {            
            ViveSR_Framework.SetCommandBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionCmd.START), true);
            IsScanning = true;         
        }

        public static void StopScanning()
        {
            IsScanning = false;
            IsExporting = false;
            ViveSR_Framework.SetCommandBool(ViveSR_Framework.MODULE_ID_RIGID_RECONSTRUCTION, (int)(ReconstructionCmd.STOP), true);
        }
    }

}