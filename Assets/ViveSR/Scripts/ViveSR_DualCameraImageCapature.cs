//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace Vive.Plugin.SR
{
    /// <summary>
    /// This is the wrapper for converting datas to fit unity format.
    /// </summary>
    public class ViveSR_DualCameraImageCapature
    {
        [DllImport("ViveSR_API", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ViveSR_GetCameraParams(ref CameraParams parameter);
        public delegate void CallbackWithPose(IntPtr ptr1, IntPtr ptr2, int int1, int int2, IntPtr pose1);
        public delegate void CallbackWith2Pose(IntPtr ptr1, IntPtr ptr2, int int1, int int2, IntPtr pose1, IntPtr pose2);

        private static IntPtr PtrDistortedAllData = IntPtr.Zero;
        private static IntPtr PtrUndistortedAllData = IntPtr.Zero;
        private static IntPtr PtrDepthAllData = IntPtr.Zero;

        private static uint SizeRawDistortedAllData, SizeRawDistortedWithoutFrameData;
        private static uint SizeRawUndistortedAllData, SizeRawUndistortedWithoutFrameData;
        private static uint SizeRawDepthAllData, SizeRawDepthWithoutFrameData;

        private static int[] RawDistortedFrameIndex = new int[1];
        private static int[] RawUndistortedFrameIndex = new int[1];
        private static int[] RawDepthFrameIndex = new int[1];

        private static int[] RawDistortedTimeIndex = new int[1];
        private static int[] RawUndistortedTimeIndex = new int[1];
        private static int[] RawDepthTimeIndex = new int[1];

        private static float[] RawDistortedPoseLeft = new float[16];
        private static float[] RawDistortedPoseRight = new float[16];
        private static float[] RawUndistortedPoseLeft = new float[16];
        private static float[] RawUndistortedPoseRight = new float[16];
        private static float[] RawDepthPose = new float[16];
        public static Matrix4x4 DistortedPoseLeft, DistortedPoseRight;
        public static Matrix4x4 UndistortedPoseLeft, UndistortedPoseRight;
        public static Matrix4x4 DepthPose;

        private static IntPtr[] PtrDistorted;
        private static IntPtr[] PtrUndistorted;
        private static IntPtr PtrDepth;
        private static Texture2D TextureDistortedLeft;
        private static Texture2D TextureDistortedRight;
        private static Texture2D TextureUndistortedLeft;
        private static Texture2D TextureUndistortedRight;
        private static Texture2D TextureDepth;

        private static CameraParams CameraParameters = new CameraParams();
        public static double DistortedCx_L;
        public static double DistortedCy_L;
        public static double DistortedCx_R;
        public static double DistortedCy_R;
        public static double UndistortedCx_L;
        public static double UndistortedCy_L;
        public static double UndistortedCx_R;
        public static double UndistortedCy_R;
        public static double FocalLength_L;
        public static double FocalLength_R;
        public static int DistortedImageWidth = 0, DistortedImageHeight = 0, DistortedImageChannel = 0;
        public static int UndistortedImageWidth = 0, UndistortedImageHeight = 0, UndistortedImageChannel = 0;
        public static int DepthImageWidth = 0, DepthImageHeight = 0, DepthImageChannel = 0, DepthDataSize = 4;
        public static float[] OffsetHeadToCamera = new float[6];

        private static int LastDistortedFrameIndex = -1;
        private static int LastUndistortedFrameIndex = -1;
        private static int LastDepthFrameIndex = -1;
        public static float[] UndistortionMap_L;
        public static float[] UndistortionMap_R;

        public static int DistortedFrameIndex { get { return RawDistortedFrameIndex[0]; } }
        public static int DistortedTimeIndex { get { return RawDistortedTimeIndex[0]; } }
        public static int UndistortedFrameIndex { get { return RawUndistortedFrameIndex[0]; } }
        public static int UndistortedTimeIndex { get { return RawUndistortedTimeIndex[0]; } }
        public static int DepthFrameIndex { get { return RawDepthFrameIndex[0]; } }
        public static int DepthTimeIndex { get { return RawDepthTimeIndex[0]; } }
        public static bool DepthProcessing { get; private set; }

        /// <summary>
        /// Initialize the image capturing tool.
        /// </summary>
        /// <returns></returns>
        public static int Initial()
        {
            GetParameters();

            TextureDistortedLeft = new Texture2D(DistortedImageWidth, DistortedImageHeight, TextureFormat.RGB24, false);
            TextureDistortedRight = new Texture2D(DistortedImageWidth, DistortedImageHeight, TextureFormat.RGB24, false);
            TextureUndistortedLeft = new Texture2D(UndistortedImageWidth, UndistortedImageHeight, TextureFormat.RGB24, false);
            TextureUndistortedRight = new Texture2D(UndistortedImageWidth, UndistortedImageHeight, TextureFormat.RGB24, false);
            TextureDepth = new Texture2D(DepthImageWidth, DepthImageHeight, TextureFormat.RFloat, false);

            PtrDistorted = new IntPtr[] { Marshal.AllocCoTaskMem(DistortedImageWidth * DistortedImageHeight * DistortedImageChannel),
                                          Marshal.AllocCoTaskMem(DistortedImageWidth * DistortedImageHeight * DistortedImageChannel) };
            PtrUndistorted = new IntPtr[] { Marshal.AllocCoTaskMem(UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel),
                                          Marshal.AllocCoTaskMem(UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel) };
            PtrDepth = Marshal.AllocCoTaskMem(DepthImageWidth * DepthImageHeight * DepthImageChannel * DepthDataSize);
            return (int)Error.WORK;
        }

        /// <summary>
        /// Check the necessary file and copy it to correct path.
        /// </summary>
        /// <returns></returns>
        public static bool CheckNecessayFile()
        {
            bool file1 = ViveSR_FileTool.CopyFile(Application.streamingAssetsPath + "/", "EZDUMP", ".ax",
                Directory.GetParent(Application.streamingAssetsPath).Parent.FullName + "/");
            bool file2 = ViveSR_FileTool.CopyFile(Application.streamingAssetsPath + "/", "VideoSinkFilter", ".ax",
                Directory.GetParent(Application.streamingAssetsPath).Parent.FullName + "/");
            bool result = file1 && file2;
            Debug.Log("[ViveSR] check file: " + result);
            return result;
        }

        private static void GetParameters()
        {
            ViveSR_GetCameraParams(ref CameraParameters);
            DistortedCx_L = CameraParameters.Cx_L;
            DistortedCy_L = CameraParameters.Cy_L;
            DistortedCx_R = CameraParameters.Cx_R;
            DistortedCy_R = CameraParameters.Cy_R;
            FocalLength_L = CameraParameters.FocalLength_L;
            FocalLength_R = CameraParameters.FocalLength_R;

            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DISTORTED, (int)DistortedParam.OUTPUT_WIDTH, ref DistortedImageWidth);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DISTORTED, (int)DistortedParam.OUTPUT_HEIGHT, ref DistortedImageHeight);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DISTORTED, (int)DistortedParam.OUTPUT_CHAANEL, ref DistortedImageChannel);

            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.OUTPUT_WIDTH, ref UndistortedImageWidth);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.OUTPUT_HEIGHT, ref UndistortedImageHeight);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.OUTPUT_CHAANEL, ref UndistortedImageChannel);

            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.OUTPUT_WIDTH, ref DepthImageWidth);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.OUTPUT_HEIGHT, ref DepthImageHeight);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_DEPTH, (int)DepthParam.OUTPUT_CHAANEL_1, ref DepthImageChannel);

            int undistortionMapSize = 0;
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.MAP_UndistortionSize, ref undistortionMapSize);
            UndistortionMap_L = new float[undistortionMapSize / sizeof(float)];
            UndistortionMap_R = new float[undistortionMapSize / sizeof(float)];
            ViveSR_Framework.GetParameterFloatArray(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.MAP_Undistortion_L, ref UndistortionMap_L);
            ViveSR_Framework.GetParameterFloatArray(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.MAP_Undistortion_R, ref UndistortionMap_R);

            float[] rawUndistortedCxCyArray = new float[8];
            ViveSR_Framework.GetParameterFloatArray(ViveSR_Framework.MODULE_ID_UNDISTORTED, (int)UndistortedParam.UndistortionCenter, ref rawUndistortedCxCyArray);
            double[] undistortedCxCyArray = new double[4];
            Buffer.BlockCopy(rawUndistortedCxCyArray, 0, undistortedCxCyArray, 0, rawUndistortedCxCyArray.Length * sizeof(float));
            UndistortedCx_L = undistortedCxCyArray[0];
            UndistortedCy_L = undistortedCxCyArray[1];
            UndistortedCx_R = undistortedCxCyArray[2];
            UndistortedCy_R = undistortedCxCyArray[3];

            ViveSR_Framework.GetParameterFloatArray(ViveSR_Framework.MODULE_ID_DISTORTED, (int)DistortedParam.OFFSET_HEAD_TO_CAMERA, ref OffsetHeadToCamera);
        }

        # region Register/Unregister
        public static int RegisterDistortedCallback()
        {
            int result = ViveSR_Framework.RegisterCallback(ViveSR_Framework.MODULE_ID_DISTORTED, 0, Marshal.GetFunctionPointerForDelegate((CallbackWith2Pose)UpdateDistortedCallback));
            return result;
        }
        public static int RegisterUndistortedCallback()
        {
            int result = ViveSR_Framework.RegisterCallback(ViveSR_Framework.MODULE_ID_UNDISTORTED, 0, Marshal.GetFunctionPointerForDelegate((CallbackWith2Pose)UpdateUndistortedCallback));
            return result;
        }
        public static int RegisterDepthCallback()
        {
            int result = ViveSR_Framework.RegisterCallback(ViveSR_Framework.MODULE_ID_DEPTH, 0, Marshal.GetFunctionPointerForDelegate((CallbackWithPose)UpdateDepthCallback));
            return result;
        }

        public static int UnregisterDistortedCallback()
        {
            int result = ViveSR_Framework.UnregisterCallback(ViveSR_Framework.MODULE_ID_DISTORTED, 0, Marshal.GetFunctionPointerForDelegate((CallbackWith2Pose)UpdateDistortedCallback));
            return result;
        }
        public static int UnregisterUndistortedCallback()
        {
            int result = ViveSR_Framework.UnregisterCallback(ViveSR_Framework.MODULE_ID_UNDISTORTED, 0, Marshal.GetFunctionPointerForDelegate((CallbackWith2Pose)UpdateUndistortedCallback));
            return result;
        }
        public static int UnregisterDepthCallback()
        {
            int result = ViveSR_Framework.UnregisterCallback(ViveSR_Framework.MODULE_ID_DEPTH, 0, Marshal.GetFunctionPointerForDelegate((CallbackWithPose)UpdateDepthCallback));
            return result;
        }
        #endregion

        #region GetTexture2D
        /// <summary>
        /// Get the distorted texture, frame index, time index from current buffer.
        /// </summary>
        /// <param name="imageLeft"></param>
        /// <param name="imageRight"></param>
        /// <param name="frameIndex"></param>
        /// <param name="timeIndex"></param>
        public static void GetDistortedTexture(out Texture2D imageLeft, out Texture2D imageRight, out int frameIndex, out int timeIndex)
        {
            if (PtrDistorted[0] != IntPtr.Zero && PtrDistorted[1] != IntPtr.Zero)
            {
                TextureDistortedLeft.LoadRawTextureData(PtrDistorted[0], DistortedImageWidth * DistortedImageHeight * DistortedImageChannel);
                TextureDistortedRight.LoadRawTextureData(PtrDistorted[1], DistortedImageWidth * DistortedImageHeight * DistortedImageChannel);
                TextureDistortedLeft.Apply();
                TextureDistortedRight.Apply();
            }
            imageLeft = TextureDistortedLeft;
            imageRight = TextureDistortedRight;
            frameIndex = DistortedFrameIndex;
            timeIndex = DistortedTimeIndex;
        }

        /// <summary>
        /// Get the undistorted texture, frame index, time index from current buffer.
        /// </summary>
        /// <param name="imageLeft"></param>
        /// <param name="imageRight"></param>
        /// <param name="frameIndex"></param>
        /// <param name="timeIndex"></param>
        public static void GetUndistortedTexture(out Texture2D imageLeft, out Texture2D imageRight, out int frameIndex, out int timeIndex)
        {
            if (PtrUndistorted[0] != IntPtr.Zero && PtrUndistorted[1] != IntPtr.Zero)
            {
                TextureUndistortedLeft.LoadRawTextureData(PtrUndistorted[0], UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel);
                TextureUndistortedRight.LoadRawTextureData(PtrUndistorted[1], UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel);
                TextureUndistortedLeft.Apply();
                TextureUndistortedRight.Apply();
            }
            imageLeft = TextureUndistortedLeft;
            imageRight = TextureUndistortedRight;
            frameIndex = UndistortedFrameIndex;
            timeIndex = UndistortedTimeIndex;
        }

        /// <summary>
        /// Get the depth texture, frame index, time index from current buffer.
        /// </summary>
        /// <param name="imageDepth"></param>
        /// <param name="frameIndex"></param>
        /// <param name="timeIndex"></param>
        public static void GetDepthTexture(out Texture2D imageDepth, out int frameIndex, out int timeIndex)
        {
            if (PtrDepth != IntPtr.Zero)
            {
                TextureDepth.LoadRawTextureData(PtrDepth, DepthImageWidth * DepthImageHeight * DepthImageChannel * DepthDataSize);
                TextureDepth.Apply();
            }
            imageDepth = TextureDepth;
            frameIndex = DepthFrameIndex;
            timeIndex = DepthTimeIndex;
        }
        #endregion

        #region GetPosture
        public static Vector3 GetDistortedLocalPosition(DualCameraIndex eye)
        {
            return eye == DualCameraIndex.LEFT ? Position(DistortedPoseLeft) : Position(DistortedPoseRight);
        }
        public static Quaternion GetDistortedLocalRotation(DualCameraIndex eye)
        {
            return eye == DualCameraIndex.LEFT ? Rotation(DistortedPoseLeft) : Rotation(DistortedPoseRight);
        }
        public static Vector3 GetUndistortedLocalPosition(DualCameraIndex eye)
        {
            return eye == DualCameraIndex.LEFT ? Position(UndistortedPoseLeft) : Position(UndistortedPoseRight);
        }
        public static Quaternion GetUndistortedLocalRotation(DualCameraIndex eye)
        {
            return eye == DualCameraIndex.LEFT ? Rotation(UndistortedPoseLeft) : Rotation(UndistortedPoseRight);
        }
        public static Vector3 GetDepthLocalPosition()
        {
            return Position(DepthPose);
        }
        public static Quaternion GetDepthLocalRotation()
        {
            return Rotation(DepthPose);
        }
        #endregion

        #region Active
        /// <summary>
        /// Update the buffer of distorted texture, frame index and time index.
        /// </summary>
        public static void UpdateDistortedImage()
        {
            int result = (int)Error.FAILED;
            int mask = (int)DistortedDataMask.LEFT_FRAME | (int)DistortedDataMask.RIGHT_FRAME
                | (int)DistortedDataMask.FRAME_SEQ | (int)DistortedDataMask.TIME_STP | (int)DistortedDataMask.LEFT_POSE | (int)DistortedDataMask.RIGHT_POSE;
            if (PtrDistortedAllData == IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_DISTORTED, ref SizeRawDistortedAllData, mask);
                if (result == (int)Error.WORK)
                {
                    PtrDistortedAllData = Marshal.AllocCoTaskMem((int)SizeRawDistortedAllData);
                }
            }
            if (PtrDistortedAllData != IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DISTORTED, PtrDistortedAllData, (int)DistortedDataMask.FRAME_SEQ, sizeof(int));
                Marshal.Copy(PtrDistortedAllData, RawDistortedFrameIndex, 0, RawDistortedFrameIndex.Length);
                if (LastDistortedFrameIndex == DistortedFrameIndex) return;
                else LastDistortedFrameIndex = DistortedFrameIndex;

                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DISTORTED, PtrDistortedAllData, mask, SizeRawDistortedAllData);
                if (result == (int)Error.WORK)
                {
                    int startIndex = 0, length = 0;
                    length = DistortedImageWidth * DistortedImageHeight * DistortedImageChannel;
                    PtrDistorted[0] = new IntPtr(PtrDistortedAllData.ToInt64() + startIndex);

                    startIndex += length;
                    length = DistortedImageWidth * DistortedImageHeight * DistortedImageChannel;
                    PtrDistorted[1] = new IntPtr(PtrDistortedAllData.ToInt64() + startIndex);

                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrDistortedAllData.ToInt64() + startIndex), RawDistortedFrameIndex, 0, RawDistortedFrameIndex.Length);

                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrDistortedAllData.ToInt64() + startIndex), RawDistortedTimeIndex, 0, RawDistortedTimeIndex.Length);

                    startIndex += length;
                    length = RawDistortedPoseLeft.Length * sizeof(float);
                    Marshal.Copy(new IntPtr(PtrDistortedAllData.ToInt64() + startIndex), RawDistortedPoseLeft, 0, RawDistortedPoseLeft.Length);

                    startIndex += length;
                    length = RawDistortedPoseRight.Length * sizeof(float);
                    Marshal.Copy(new IntPtr(PtrDistortedAllData.ToInt64() + startIndex), RawDistortedPoseRight, 0, RawDistortedPoseRight.Length);

                    for (int i = 0; i < 4; i++)
                    {
                        DistortedPoseLeft.SetColumn(i, new Vector4(RawDistortedPoseLeft[i * 4 + 0], RawDistortedPoseLeft[i * 4 + 1],
                                                                   RawDistortedPoseLeft[i * 4 + 2], RawDistortedPoseLeft[i * 4 + 3]));
                        DistortedPoseRight.SetColumn(i, new Vector4(RawDistortedPoseRight[i * 4 + 0], RawDistortedPoseRight[i * 4 + 1],
                                                                    RawDistortedPoseRight[i * 4 + 2], RawDistortedPoseRight[i * 4 + 3]));

                    }
                }
            }
        }

        /// <summary>
        /// Update the buffer of undistorted texture, frame index and time index.
        /// </summary>
        public static void UpdateUndistortedImage()
        {
            int result = (int)Error.FAILED;
            int mask = (int)UndistortedDataMask.LEFT_FRAME | (int)UndistortedDataMask.RIGHT_FRAME
                | (int)UndistortedDataMask.FRAME_SEQ | (int)UndistortedDataMask.TIME_STP | (int)UndistortedDataMask.LEFT_POSE | (int)UndistortedDataMask.RIGHT_POSE;
            if (PtrUndistortedAllData == IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_UNDISTORTED, ref SizeRawUndistortedAllData, mask);
                if (result == (int)Error.WORK)
                {
                    PtrUndistortedAllData = Marshal.AllocCoTaskMem((int)SizeRawUndistortedAllData);
                }
            }
            if (PtrUndistortedAllData != IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_UNDISTORTED, PtrUndistortedAllData, (int)UndistortedDataMask.FRAME_SEQ, sizeof(int));
                Marshal.Copy(PtrUndistortedAllData, RawUndistortedFrameIndex, 0, RawUndistortedFrameIndex.Length);
                if (LastUndistortedFrameIndex == UndistortedFrameIndex) return;
                else LastUndistortedFrameIndex = UndistortedFrameIndex;

                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_UNDISTORTED, PtrUndistortedAllData, mask, SizeRawUndistortedAllData);
                if (result == (int)Error.WORK)
                {
                    int startIndex = 0, length = 0;
                    length = UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel;
                    PtrUndistorted[0] = new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex);

                    startIndex += length;
                    length = UndistortedImageWidth * UndistortedImageHeight * UndistortedImageChannel;
                    PtrUndistorted[1] = new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex);

                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex), RawUndistortedFrameIndex, 0, RawUndistortedFrameIndex.Length);

                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex), RawUndistortedTimeIndex, 0, RawUndistortedTimeIndex.Length);

                    startIndex += length;
                    length = RawUndistortedPoseLeft.Length * sizeof(float);
                    Marshal.Copy(new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex), RawUndistortedPoseLeft, 0, RawUndistortedPoseLeft.Length);

                    startIndex += length;
                    length = RawUndistortedPoseRight.Length * sizeof(float);
                    Marshal.Copy(new IntPtr(PtrUndistortedAllData.ToInt64() + startIndex), RawUndistortedPoseRight, 0, RawUndistortedPoseRight.Length);

                    for (int i = 0; i < 4; i++)
                    {
                        UndistortedPoseLeft.SetColumn(i, new Vector4(RawUndistortedPoseLeft[i * 4 + 0], RawUndistortedPoseLeft[i * 4 + 1],
                                                                     RawUndistortedPoseLeft[i * 4 + 2], RawUndistortedPoseLeft[i * 4 + 3]));
                        UndistortedPoseRight.SetColumn(i, new Vector4(RawUndistortedPoseRight[i * 4 + 0], RawUndistortedPoseRight[i * 4 + 1],
                                                                      RawUndistortedPoseRight[i * 4 + 2], RawUndistortedPoseRight[i * 4 + 3]));
                    }
                }
            }
        }

        /// <summary>
        /// Update the buffer of depth texture, frame index and time index.
        /// </summary>
        public static void UpdateDepthImage()
        {
            int result = (int)Error.FAILED;
            int mask = (int)DepthDataMask.DEPTH_MAP | (int)DepthDataMask.FRAME_SEQ | (int)DepthDataMask.TIME_STP | (int)DepthDataMask.POSE;
            if (PtrDepthAllData == IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_DEPTH, ref SizeRawDepthAllData, mask);
                if (result == (int)Error.WORK)
                {
                    PtrDepthAllData = Marshal.AllocCoTaskMem((int)SizeRawDepthAllData);
                }
            }
            if (PtrDepthAllData != IntPtr.Zero)
            {
                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DEPTH, PtrDepthAllData, (int)DepthDataMask.FRAME_SEQ, sizeof(int));
                Marshal.Copy(PtrDepthAllData, RawDepthFrameIndex, 0, RawDepthFrameIndex.Length);
                if (LastDepthFrameIndex == DepthFrameIndex) return;
                else LastDepthFrameIndex = DepthFrameIndex;

                result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_DEPTH, PtrDepthAllData, mask, SizeRawDepthAllData);
                if (result == (int)Error.WORK)
                {
                    int startIndex = 0, length = 0;
                    length = DepthImageWidth * DepthImageHeight * DepthImageChannel * DepthDataSize;
                    PtrDepth = new IntPtr(PtrDepthAllData.ToInt64() + startIndex);
                    
                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrDepthAllData.ToInt64() + startIndex), RawDepthFrameIndex, 0, RawDepthFrameIndex.Length);

                    startIndex += length;
                    length = sizeof(int);
                    Marshal.Copy(new IntPtr(PtrDepthAllData.ToInt64() + startIndex), RawDepthTimeIndex, 0, RawDepthTimeIndex.Length);

                    startIndex += length;
                    length = RawDepthPose.Length * sizeof(float);
                    Marshal.Copy(new IntPtr(PtrDepthAllData.ToInt64() + startIndex), RawDepthPose, 0, RawDepthPose.Length);

                    for (int i = 0; i < 4; i++)
                    {
                        DepthPose.SetColumn(i, new Vector4(RawDepthPose[i * 4 + 0], RawDepthPose[i * 4 + 1],
                                                           RawDepthPose[i * 4 + 2], RawDepthPose[i * 4 + 3]));
                    }
                }
            }
        }
        #endregion

        #region Passive Callbacks
        private static void UpdateDistortedCallback(IntPtr left, IntPtr right, int frame, int time, IntPtr ptrPoseLeft, IntPtr ptrPoseRight)
        {
            PtrDistorted[0] = left;
            PtrDistorted[1] = right;
            Marshal.Copy(ptrPoseLeft, RawDistortedPoseLeft, 0, RawDistortedPoseLeft.Length);
            Marshal.Copy(ptrPoseRight, RawDistortedPoseRight, 0, RawDistortedPoseRight.Length);
            RawDistortedFrameIndex[0] = frame;
            RawDistortedTimeIndex[0] = time;

            for (int i = 0; i < 4; i++)
            {
                DistortedPoseLeft.SetColumn(i, new Vector4(RawDistortedPoseLeft[i * 4 + 0], RawDistortedPoseLeft[i * 4 + 1],
                                                           RawDistortedPoseLeft[i * 4 + 2], RawDistortedPoseLeft[i * 4 + 3]));
                DistortedPoseRight.SetColumn(i, new Vector4(RawDistortedPoseRight[i * 4 + 0], RawDistortedPoseRight[i * 4 + 1],
                                                            RawDistortedPoseRight[i * 4 + 2], RawDistortedPoseRight[i * 4 + 3]));

            }
        }

        private static void UpdateUndistortedCallback(IntPtr left, IntPtr right, int frame, int time, IntPtr ptrPoseLeft, IntPtr ptrPoseRight)
        {
            PtrUndistorted[0] = left;
            PtrUndistorted[1] = right;
            Marshal.Copy(ptrPoseLeft, RawUndistortedPoseLeft, 0, RawUndistortedPoseLeft.Length);
            Marshal.Copy(ptrPoseRight, RawUndistortedPoseRight, 0, RawUndistortedPoseRight.Length);
            RawUndistortedFrameIndex[0] = frame;
            RawUndistortedTimeIndex[0] = time;

            for (int i = 0; i < 4; i++)
            {
                UndistortedPoseLeft.SetColumn(i, new Vector4(RawUndistortedPoseLeft[i * 4 + 0], RawUndistortedPoseLeft[i * 4 + 1],
                                                             RawUndistortedPoseLeft[i * 4 + 2], RawUndistortedPoseLeft[i * 4 + 3]));
                UndistortedPoseRight.SetColumn(i, new Vector4(RawUndistortedPoseRight[i * 4 + 0], RawUndistortedPoseRight[i * 4 + 1],
                                                              RawUndistortedPoseRight[i * 4 + 2], RawUndistortedPoseRight[i * 4 + 3]));
            }
        }

        private static void UpdateDepthCallback(IntPtr left, IntPtr depth, int frame, int time, IntPtr ptrPose)
        {
            PtrDepth = depth;
            RawDepthFrameIndex[0] = frame;
            RawDepthTimeIndex[0] = time;
            Marshal.Copy(ptrPose, RawDepthPose, 0, RawDepthPose.Length);
            for (int i = 0; i < 4; i++)
            {
                DepthPose.SetColumn(i, new Vector4(RawDepthPose[i * 4 + 0], RawDepthPose[i * 4 + 1],
                                                   RawDepthPose[i * 4 + 2], RawDepthPose[i * 4 + 3]));
            }
        }
        #endregion

        #region Utility
        private static Quaternion Rotation(Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }
        private static Vector3 Position(Matrix4x4 m)
        {
            return new Vector3(m.m03, m.m13, m.m23);
        }
        #endregion

        #region User
        public static int EnableDepthProcess(bool active)
        {
            int result = ViveSR_Framework.ChangeModuleLinkStatus(ViveSR_Framework.MODULE_ID_UNDISTORTED, ViveSR_Framework.MODULE_ID_DEPTH, DepthProcessing ? (int)WorkLinkMethod.NONE : (int)WorkLinkMethod.ACTIVE);
            if (result == (int)Error.WORK) DepthProcessing = active;
            return result;
        }
        #endregion
    }
}