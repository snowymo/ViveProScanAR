using System.Runtime.InteropServices;

namespace Vive.Plugin.SR
{
    public class ViveSR_Framework
    {
        [DllImport("ViveSR_API")] private static extern int ViveSR_Initial();
        [DllImport("ViveSR_API")] private static extern int ViveSR_Stop();

        [DllImport("ViveSR_API")] private static extern int ViveSR_CreateModule(int ModuleType, ref int ModuleID);
        [DllImport("ViveSR_API")] private static extern int ViveSR_StartModule(int ModuleID);
        [DllImport("ViveSR_API")] private static extern int ViveSR_ModuleLink(int from, int to, int mode);

        [DllImport("ViveSR_API")] private static extern int ViveSR_GetMultiDataSize(int moduleID, ref uint size, int DataMask);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetMultiData(int moduleID, System.IntPtr data, int DataMask, uint size);

        [DllImport("ViveSR_API")] private static extern int ViveSR_RegisterCallback(int module, int type, System.IntPtr callback);
        [DllImport("ViveSR_API")] private static extern int ViveSR_UnregisterCallback(int module, int type, System.IntPtr callback);

        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterBool(int module, int type, ref bool parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterBool(int module, int type, bool parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterInt(int module, int type, ref int parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterInt(int module, int type, int parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterFloat(int module, int type, ref float parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterFloat(int module, int type, float parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterDouble(int module, int type, ref double parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterDouble(int module, int type, double parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterString(int module, int type, ref string parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterString(int module, int type, string parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_GetParameterFloatArray(int module, int type, ref float[] parameter);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetParameterFloatArray(int module, int type, float[] parameter);

        [DllImport("ViveSR_API")] private static extern int ViveSR_SetCommandBool(int module, int type, bool content);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetCommandInt(int module, int type, int content);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetCommandFloat(int module, int type, float content);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetCommandString(int module, int type, string content);
        [DllImport("ViveSR_API")] private static extern int ViveSR_SetCommandFloat3(int module, int type, float content0, float content1, float content2);
        [DllImport("ViveSR_API")] private static extern int ViveSR_ChangeModuleLinkStatus(int from, int to, int mode);

        [DllImport("ViveSR_API")] private static extern int ViveSR_SetLogLevel(int level);

        public static int MODULE_ID_DISTORTED;
        public static int MODULE_ID_UNDISTORTED;
        public static int MODULE_ID_DEPTH;
        public static int MODULE_ID_RIGID_RECONSTRUCTION;
        public static int MODULE_ID_CHAPERONE;

        public static int Initial()
        {
            return ViveSR_Initial();
        }

        public static int Stop()
        {
            return ViveSR_Stop();
        }

        public static int CreateModule(int ModuleType, ref int ModuleID)
        {
            return ViveSR_CreateModule(ModuleType, ref ModuleID);
        }

        public static int StartModule(int ModuleID)
        {
            return ViveSR_StartModule(ModuleID);
        }

        public static int ModuleLink(int from, int to, int mode)
        {
            return ViveSR_ModuleLink(from, to, mode);
        }

        public static int GetMultiDataSize(int moduleID, ref uint size, int DataMask)
        {
            return ViveSR_GetMultiDataSize(moduleID, ref size, DataMask);
        }

        public static int GetMultiData(int moduleID, System.IntPtr data, int DataMask, uint size)
        {
            return ViveSR_GetMultiData(moduleID, data, DataMask, size);
        }

        public static int RegisterCallback(int module, int type, System.IntPtr callback)
        {
            return ViveSR_RegisterCallback(module, type, callback);
        }

        public static int UnregisterCallback(int module, int type, System.IntPtr callback)
        {
            return ViveSR_UnregisterCallback(module, type, callback);
        }

        public static int GetParameterBool(int module, int type, ref bool parameter)
        {
            return ViveSR_GetParameterBool(module, type, ref parameter);
        }
        public static int SetParameterBool(int module, int type, bool parameter)
        {
            return ViveSR_SetParameterBool(module, type, parameter);
        }

        public static int GetParameterInt(int module, int type, ref int parameter)
        {
            return ViveSR_GetParameterInt(module, type, ref parameter);
        }
        public static int SetParameterInt(int module, int type, int parameter)
        {
            return ViveSR_SetParameterInt(module, type, parameter);
        }

        public static int GetParameterFloat(int module, int type, ref float parameter)
        {
            return ViveSR_GetParameterFloat(module, type, ref parameter);
        }
        public static int SetParameterFloat(int module, int type, float parameter)
        {
            return ViveSR_SetParameterFloat(module, type, parameter);
        }

        public static int GetParameterDouble(int module, int type, ref double parameter)
        {
            return ViveSR_GetParameterDouble(module, type, ref parameter);
        }
        public static int SetParameterDouble(int module, int type, double parameter)
        {
            return ViveSR_SetParameterDouble(module, type, parameter);
        }

        public static int SetParameterString(int module, int type, string parameter)
        {
            return ViveSR_SetParameterString(module, type, parameter);
        }

        public static int GetParameterFloatArray(int module, int type, ref float[] parameter)
        {
            return ViveSR_GetParameterFloatArray(module, type, ref parameter);
        }

        public static int SetParameterFloatArray(int module, int type, float[] parameter)
        {
            return ViveSR_SetParameterFloatArray(module, type, parameter);
        }

        public static int SetCommandBool(int module, int type, bool content)
        {
            return ViveSR_SetCommandBool(module, type, content);
        }
        public static int SetCommandInt(int module, int type, int content)
        {
            return ViveSR_SetCommandInt(module, type, content);
        }
        public static int SetCommandFloat(int module, int type, float content)
        {
            return ViveSR_SetCommandFloat(module, type, content);
        }
        public static int SetCommandString(int module, int type, string content)
        {
            return ViveSR_SetCommandString(module, type, content);
        }
        public static int SetCommandFloat3(int module, int type, float content0, float content1, float content2)
        {
            return ViveSR_SetCommandFloat3(module, type, content0, content1, content2);
        }
        public static int ChangeModuleLinkStatus(int from, int to, int mode)
        {
            return ViveSR_ChangeModuleLinkStatus(from, to, mode);
        }

        public static int SetLogLevel(int level)
        {
            return ViveSR_SetLogLevel(level);
        }
    }
}