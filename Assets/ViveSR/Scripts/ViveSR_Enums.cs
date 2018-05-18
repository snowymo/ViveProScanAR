using System.Runtime.InteropServices;

namespace Vive.Plugin.SR
{
    [StructLayout(LayoutKind.Sequential)]
    struct CameraParams
    {
        public double Cx_L;
        public double Cx_R;
        public double Cy_L;
        public double Cy_R;
        public double FocalLength_L;
        public double FocalLength_R;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public double[] Rotation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] Translation;
    };
}


namespace Vive.Plugin.SR
{
    enum ModuleStatus
    {
        SR_DISABLE = 0,
        SR_ENABLE = 1,
    };
    enum ModuleDictionary
    {
        DEVICE_VIVE2_MODE1 = 1,
        DEVICE_VIVE2_MODE2 = 2,
        ENGINE_UNDISTORTED = 3,
        ENGINE_DEPTH = 4,
        ENGINE_RIGID_RECONSTRUCTION = 5,
        ENGINE_CHAPERONE = 6,
    };

    enum WorkLinkMethod
    {
        NONE = -1,
        PASSIVE = 0,
        ACTIVE = 1,
    };

    public enum FrameworkStatus
    {
        WORKING,
        STOP,
        ERROR
    }

    public enum DualCameraMode
    {
        REAL,
        MIX
    }

    public enum DualCameraIndex
    {
        LEFT,
        RIGHT
    }

    public enum CalibrationType
    {
        RELATIVE,
        ABSOLUTE
    }

    public enum CalibrationAxis
    {
        X, Y, Z
    }

    public enum DualCameraDisplayMode
    {
        VIRTUAL,
        REAL,
        MIX
    }

    public enum DualCameraStatus
    {
        NOT_FOUND,
        IDLE,
        WORKING,
        ERROR
    }

    public enum UndistortionMethod
    {
        DEFISH_BY_MESH,
        DEFISH_BY_SHADER,
        DEFISH_BY_SRMODULE,
    }
}

// Native
namespace Vive.Plugin.SR
{
    enum Error
    {
        FAILED = -1,
        WORK = 0,
        INVAILD_INPUT = 1,
        FILE_NOT_FOUND = 2,
        DATA_NOT_FOUND = 13,
        INITIAL_FAILED = 1001,
        NOT_IMPLEMENTED = 1003,
        NULL_POINTER = 1004,
        OVER_MAX_LENGTH = 1005,
        FILE_INVALID = 1006,
        UNINSTALL_STEAM = 1007,
        MEMCPY_FAIL = 1008,
        NOT_MATCH = 1009,
        NODE_NOT_EXIST = 1010,
        UNKONW_MODULE = 1011,
        MODULE_FULL = 1012,
        UNKNOW_TYPE = 1013,
        INVALID_MODULE = 1014,
        INVALID_TYPE = 1015,
        MEMORY_NOT_ENOUGH = 1016,
    };

    enum Module_Param
    {
        ENABLE_FPSCONTROL = 1001,
        SET_FPS = 1002,
    };

    enum CameraParam
    {
        Cx_L,
        Cx_R,
        Cy_L,
        Cy_R,
        FocalLength_L,
        FocalLength_R,
        R_m0,
        R_m1,
        R_m2,
        R_m3,
        R_m4,
        R_m5,
        R_m6,
        R_m7,
        R_m8,
        T_m0,
        T_m1,
        T_m2
    };

    #region Distorted
    enum DistortedParam
    {
        VR_INIT,
        VR_INIT_TYPE,
        OUTPUT_WIDTH,
        OUTPUT_HEIGHT,
        OUTPUT_CHAANEL,
        OPTPUT_IMAGE_ORIGIN,    // 0 top-left, 1 top-right, 2 bottom-left, 3 bottom-right
        OUTPUT_FPS,
        OFFSET_HEAD_TO_CAMERA,  // float[6]  (x0,y0,z0,x1,y1,z1)
        PLAY_AREA_RECT,		    // float[12] (x0,y0,z0,x1,y1,z1,...)
    };

    enum DistortedDataMask
    {
        LEFT_FRAME = 0X01,
        RIGHT_FRAME = 0X01 << 1,
        FRAME_SEQ = 0X01 << 2,
        TIME_STP = 0X01 << 3,
        LEFT_POSE = 0X01 << 4,
        RIGHT_POSE = 0X01 << 5,
    };
    #endregion

    #region Undistorted
    enum UndistortedParam
    {
        OUTPUT_WIDTH,
        OUTPUT_HEIGHT,
        OUTPUT_CHAANEL,
        CX = 3,
		CY = 4,
		MODE = 5,
		FOCULENS = 6,
		FMAT_RM_L = 7,
		FMAT_RM_R = 8,
		INTRINSIC_L = 9,
		INTRINSIC_R = 10,
		R_RECTIFY_L = 13,
		R_RECTIFY_R = 14,
		COEFFS_L = 15,
		COEFFS_R = 16,
		P_NEWPROJ_L = 17,
		P_NEWPROJ_R = 18,
		MAP_UndistortionSize,
		MAP_Undistortion_L,
		MAP_Undistortion_R,
        UndistortionCenter,
    };

    enum UndistortedDataMask
    {
        LEFT_FRAME = 0X01,
        RIGHT_FRAME = 0X01 << 1,
        FRAME_SEQ = 0X01 << 2,
        TIME_STP = 0X01 << 3,
        LEFT_POSE = 0X01 << 4,
        RIGHT_POSE = 0X01 << 5,
    };
    #endregion

    #region Depth
    enum DepthParam
    {
        OUTPUT_WIDTH,
        OUTPUT_HEIGHT,
        OUTPUT_CHAANEL_0,
        OUTPUT_CHAANEL_1,
        TYPE,
        FOCULENS,
        BASELINE,
        COLLIDER_QUALITY,
        MESH_NEAR_DISTANCE,
        MESH_FAR_DISTANCE,
    };
    enum DepthDataMask
    {
        LEFT_FRAME = 0X01,
        DEPTH_MAP = 0X01 << 1,
        FRAME_SEQ = 0X01 << 2,
        TIME_STP = 0X01 << 3,
        POSE = 0X01 << 4,
        NUM_VERTICES = 0X01 << 5,
        BYTEPERVERT = 0X01 << 6,
        VERTICES = 0X01 << 7,
        NUM_INDICES = 0X01 << 8,
        INDICES = 0X01 << 9,
    };
    enum DepthCmd
    {
        EXTRACT_DEPTH_MESH = 0,
        ENABLE_SELECT_MESH_DISTANCE_RANGE,
		ENABLE_REFINE,
    }
    #endregion

    #region Reconstruction
    enum ReconstructionParam
    {
        VOXEL_SIZE = 0,
        COLOR_SIZE = 1,
        DATA_SOURCE = 2,
        DATASET_PATH = 3,
        RGB_IMAGE_EXT = 4,
        DATASET_FRAME = 5,
        MAX_DEPTH = 6,
        MIN_DEPTH = 7,
        POINTCLOUD_POINTSIZE = 9,
        EXPORT_ADAPTIVE_MODEL = 10,
        ADAPTIVE_MAX_GRID = 11,
        ADAPTIVE_MIN_GRID = 12,
        ADAPTIVE_ERROR_THRES = 13,

        CONFIG_FILEPATH = 21,
        //CONFIG_DATA_SOURCE,
        //CONFIG_DATASET_FRAME_NUM,
        //CONFIG_DATASET_PATH,
        CONFIG_QUALITY,
        CONFIG_EXPORT_COLLIDER,
        CONFIG_EXPORT_TEXTURE,

        DATA_CURRENT_POS = 31,
        LITE_POINT_CLOUD_MODE,
        FULL_POINT_CLOUD_MODE,
        LIVE_ADAPTIVE_MODE,

        MESH_REFRESH_INTERVAL = 37,
    };

    enum ReconstructionDataMask
    {
        FRAME_SEQ = 0X01,
        POSEMTX44 = 0X01 << 1,
        NUM_VERTICES = 0X01 << 2,
        BYTEPERVERT = 0X01 << 3,
        VERTICES = 0X01 << 4,
        NUM_INDICES = 0X01 << 5,
        INDICES = 0X01 << 6,
        CLDTYPE = 0X01 << 7,
        CLD_VERTICES = 0X01 << 8,
        CLD_INDICES = 0X01 << 9,
        COLLIDERNUM = 0X01 << 10,
        CLD_NUM_VERTS = 0X01 << 11,
        CLD_NUMIDX = 0X01 << 12,
    };

    enum ReconstructionCmd
    {
        START = 0,
        STOP = 1,
        SHOW_INFO = 2,
        EXTRACT_POINT_CLOUD = 3,
        EXTRACT_VERTEX_NORMAL = 4,
        EXPORT_MODEL_RIGHT_HAND = 5,
        EXPORT_MODEL_FOR_UNITY = 6,
        EXTRACT_COLLIDER_ONE_TIME = 7,
    };

    enum ReconstructionCallback
    {
        DATA,
        EXPORT_PROGRESS
    };

    public enum ReconstructionDataSource
    {
        HMD = 0,
        DATASET = 1
    }
    public enum ReconstructionQuality
    {
        LOW = 2,
        MID = 3,
        HIGH = 4,
    }

    public enum ReconstructionLiveMeshExtractMode
    {
        VERTEX_WITHOUT_NORMAL = 0,
        VERTEX_WITH_NORMAL = 1,
        FACE_NORMAL = 2,
    };

    public enum ReconstructionLiveColliderType
    {
        CONVEX_SHAPE = 0,
        BOUNDING_BOX_SHAPE = 1,
    };

    public enum ReconstructionExportStage
    {
        STAGE_EXTRACTING_MODEL = 0x0017,
        STAGE_COMPACTING_TEXTURE = 0x0018,
        STAGE_EXTRACTING_COLLIDER = 0x0019,
        STAGE_SAVING_MODEL_FILE = 0x0020,
    }

    public enum ReconstructionDisplayMode
    {
        FULL_SCENE = 0,
        FIELD_OF_VIEW = 1,
        ADAPTIVE_MESH = 2,
    }
    #endregion

    #region Chaperone
    enum ChaperoneParam
    {
        OUTPUT_WIDTH,
        OUTPUT_HEIGHT,
        OUTPUT_CHAANEL_0,
        OUTPUT_CHAANEL_1,
        TYPE,
    };
    enum ChaperoneDataMask
    {
        LEFT_FRAME = 0X01,
        MASK_MAP = 0X01 << 1,
        DEPTH_MAP = 0X01 << 2,
        FRAME_SEQ = 0X01 << 3,
        TIME_STP = 0X01 << 4,
        POSE = 0X01 << 5,
    };
    #endregion
}
