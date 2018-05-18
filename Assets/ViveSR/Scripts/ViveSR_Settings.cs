#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Vive.Plugin.SR
{
    public class ViveSR_Settings : EditorWindow
    {
        public const string LefteyeLayerName = "DualCamera (Left)";
        public const string RighteyeLayerName = "DualCamera (Right)";
        public const string VirtualWorldLayerName = "VirtualWorldLayer";
        private const int VRWorldLayer = 2;
        private static bool IsNeedStereoSetting = false;
        private static bool IsNeedPortalSetting = false;
        private int[] candidateLayerIndex = new int[3];
        private bool IsStereoInputValid = false;
        private bool IsPortalInputValid = false;

        const string Label_LayerSetting = "ViveSR needs 2 layers for rendering Lefteye and Righteye images.";
        const string HelpboxText_LayerNotValid = "Please insert valid values";
        const string BtnName_UpdateLayers = "Update layers";
        const string DialogTitle_Error = "Error";
        const string DialogTitle_ScriptNotFound = "ViveSR_DualCameraRig.Instance does not exist in current scene";
        const string Label_LayerChangeName = "If you want to change layer name, please modify the variables of LefteyeLayerName and RighteyeLayerName.";

        const string Label_PortalLayerSetting = "ViveSR needs 1 layer for rendering portal effect.";

        static ViveSR_Settings window;        
        private static string[] CandidateLayername = new string[3] { "", "", "" };
        private Rect LastRect;

        public static void Update()
        {
            IsNeedPortalSetting = FindObjectOfType<ViveSR_PortalMgr>() != null && !CheckPortalLayer();
            IsNeedStereoSetting = !CheckLayers(DualCameraIndex.LEFT) || !CheckLayers(DualCameraIndex.RIGHT);

            bool show = IsNeedStereoSetting || IsNeedPortalSetting;
            if (show)
            {
                window = GetWindow<ViveSR_Settings>(true);
                window.minSize = new Vector2(300, 400);
            }
            FindCandidate();
            EditorApplication.update -= Update;
        }

        void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets\\ViveSR\\Textures\\SRWorks_logo.png");
            var rect = GUILayoutUtility.GetRect(position.width, 150, GUI.skin.box);
            if (logo)
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            if (IsNeedStereoSetting) OnGUIStereo();

            // portal layer part
            if (IsNeedPortalSetting) OnGUIPortal();

            if (IsStereoInputValid || IsPortalInputValid)
                if (GUILayout.Button(BtnName_UpdateLayers))
                {
                    if (ViveSR_DualCameraRig.Instance != null)
                    {
                        if (IsStereoInputValid) SetStereoLayer();
                        if (IsPortalInputValid) SetPortalLayer();
                        EditorUtility.DisplayDialog(BtnName_UpdateLayers, "Done!", "Ok");
                        Close();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(DialogTitle_Error, DialogTitle_ScriptNotFound, "Cancel");
                        Close();
                    }
                }

            GUILayout.Label(Label_LayerChangeName);
        }

        private void OnGUIStereo()
        {
            GUILayout.Label(Label_LayerSetting);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LefteyeLayerName + ":  ");
            LastRect = GUILayoutUtility.GetLastRect();
            CandidateLayername[(int)DualCameraIndex.LEFT] = GUI.TextField(new Rect(LastRect.x + 130, LastRect.y, 25, 20), CandidateLayername[(int)DualCameraIndex.LEFT], 2);
            CandidateLayername[(int)DualCameraIndex.LEFT] = Regex.Replace(CandidateLayername[(int)DualCameraIndex.LEFT], @"[^0-9.]", "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(RighteyeLayerName + ":");
            LastRect = GUILayoutUtility.GetLastRect();
            CandidateLayername[(int)DualCameraIndex.RIGHT] = GUI.TextField(new Rect(LastRect.x + 130, LastRect.y, 25, 20), CandidateLayername[(int)DualCameraIndex.RIGHT], 2);
            CandidateLayername[(int)DualCameraIndex.RIGHT] = Regex.Replace(CandidateLayername[(int)DualCameraIndex.RIGHT], @"[^0-9.]", "");
            GUILayout.EndHorizontal();

            Int32.TryParse(CandidateLayername[(int)DualCameraIndex.LEFT], out candidateLayerIndex[(int)DualCameraIndex.LEFT]);
            Int32.TryParse(CandidateLayername[(int)DualCameraIndex.RIGHT], out candidateLayerIndex[(int)DualCameraIndex.RIGHT]);

            GUILayout.FlexibleSpace();
            if (CandidateLayername[(int)DualCameraIndex.LEFT] != CandidateLayername[(int)DualCameraIndex.RIGHT] &&
                candidateLayerIndex[(int)DualCameraIndex.LEFT] >= 8 &&
                candidateLayerIndex[(int)DualCameraIndex.LEFT] < 32 &&
                candidateLayerIndex[(int)DualCameraIndex.RIGHT] >= 8 &&
                candidateLayerIndex[(int)DualCameraIndex.RIGHT] < 32)
            {
                IsStereoInputValid = true;
            }
            else
                EditorGUILayout.HelpBox(HelpboxText_LayerNotValid, MessageType.Error);
        }

        private void OnGUIPortal()
        {
            //GUILayout.FlexibleSpace();
            GUILayout.Label(Label_PortalLayerSetting);
            GUILayout.BeginHorizontal();
            GUILayout.Label(VirtualWorldLayerName + ":");
            LastRect = GUILayoutUtility.GetLastRect();
            CandidateLayername[VRWorldLayer] = GUI.TextField(new Rect(LastRect.x + 130, LastRect.y, 25, 20), CandidateLayername[VRWorldLayer], 2);
            CandidateLayername[VRWorldLayer] = Regex.Replace(CandidateLayername[VRWorldLayer], @"[^0-9.]", "");
            GUILayout.EndHorizontal();

            Int32.TryParse(CandidateLayername[VRWorldLayer], out candidateLayerIndex[VRWorldLayer]);

            GUILayout.FlexibleSpace();
            if (CandidateLayername[VRWorldLayer] != "" && candidateLayerIndex[VRWorldLayer] >= 8 && candidateLayerIndex[VRWorldLayer] < 32)
            {
                IsPortalInputValid = true;
            }
        }

        private void SetStereoLayer()
        {
            ModifyLayer(Int32.Parse(CandidateLayername[(int)DualCameraIndex.LEFT]), LefteyeLayerName);
            ModifyLayer(Int32.Parse(CandidateLayername[(int)DualCameraIndex.RIGHT]), RighteyeLayerName);
            UpdateLayers(DualCameraIndex.LEFT);
            UpdateLayers(DualCameraIndex.RIGHT);
        }

        private void SetPortalLayer()
        {
            ModifyLayer(Int32.Parse(CandidateLayername[VRWorldLayer]), VirtualWorldLayerName);
            UpdateCollisionMatrix();
        }

        private static bool FindCandidate()
        {
            int[] candidateLayer = new int[3] { LayerMask.NameToLayer(LefteyeLayerName), LayerMask.NameToLayer(RighteyeLayerName), LayerMask.NameToLayer(VirtualWorldLayerName) };
            int searchLayer = 31;
            for (int i = 0; i < candidateLayer.Length; i++)
            {                
                if (candidateLayer[i] == -1)
                {
                    for (; searchLayer > 8; searchLayer--)
                    {
                        if (LayerMask.LayerToName(searchLayer) == "")
                        {
                            bool occupied = false;
                            for (int j = i; j >= 0; j--) if (candidateLayer[j] == searchLayer) occupied = true;
                            if (occupied) continue;

                            candidateLayer[i] = searchLayer;
                            break;
                        }
                    }
                }
            }

            CandidateLayername[(int)DualCameraIndex.LEFT] = candidateLayer[(int)DualCameraIndex.LEFT].ToString();
            CandidateLayername[(int)DualCameraIndex.RIGHT] = candidateLayer[(int)DualCameraIndex.RIGHT].ToString();
            CandidateLayername[VRWorldLayer] = candidateLayer[VRWorldLayer].ToString();

            return true;
        }

        private static bool CheckPortalLayer()
        {
            int layer = LayerMask.NameToLayer(VirtualWorldLayerName);
            return (layer != -1);           
        }

        private static bool CheckLayers(DualCameraIndex cameraIndex)
        {
            if (ViveSR_DualCameraRig.Instance == null || ViveSR_DualCameraRig.Instance == null) return false;
            string layername = cameraIndex == DualCameraIndex.LEFT ? LefteyeLayerName : RighteyeLayerName;
            int mask = LayerMask.GetMask(layername);
            int layer = LayerMask.NameToLayer(layername);
            Camera cam = cameraIndex == DualCameraIndex.LEFT ?
                ViveSR_DualCameraRig.Instance.DualCameraLeft :
                ViveSR_DualCameraRig.Instance.DualCameraRight;
            ViveSR_TrackedCamera trackedCam = cameraIndex == DualCameraIndex.LEFT ?
                ViveSR_DualCameraRig.Instance.TrackedCameraLeft :
                ViveSR_DualCameraRig.Instance.TrackedCameraRight;
            ViveSR_TrackedCamera trackedCamAnother = cameraIndex == DualCameraIndex.LEFT ?
                ViveSR_DualCameraRig.Instance.TrackedCameraRight :
                ViveSR_DualCameraRig.Instance.TrackedCameraLeft;

            if (cam.cullingMask != mask) return false;
            if (trackedCam.gameObject.layer != layer) return false;
            if (trackedCam.Anchor.gameObject.layer != layer) return false;
            if (trackedCam.ImagePlane.gameObject.layer != layer) return false;
            if (trackedCamAnother.ImagePlaneCalibration.gameObject.layer != layer) return false;
            return true;
        }
        
        private void UpdateCollisionMatrix()
        {
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer(VirtualWorldLayerName));
        }

        private void UpdateLayers(DualCameraIndex cameraIndex)
        {
            string layername = cameraIndex == DualCameraIndex.LEFT ? LefteyeLayerName : RighteyeLayerName;
            int mask = LayerMask.GetMask(layername);
            int layer = LayerMask.NameToLayer(layername);
            ViveSR_DualCameraRig prefabRig = PrefabUtility.GetPrefabParent(ViveSR_DualCameraRig.Instance) as ViveSR_DualCameraRig;
            prefabRig.VirtualCamera.cullingMask = -1;
            prefabRig.VirtualCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(LefteyeLayerName));
            prefabRig.VirtualCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(RighteyeLayerName));

            Camera cam = cameraIndex == DualCameraIndex.LEFT ?
                prefabRig.DualCameraLeft:
                prefabRig.DualCameraRight;
            ViveSR_TrackedCamera trackedCam = cameraIndex == DualCameraIndex.LEFT ?
                prefabRig.TrackedCameraLeft :
                prefabRig.TrackedCameraRight;
            ViveSR_TrackedCamera trackedCamAnother = cameraIndex == DualCameraIndex.LEFT ?
                prefabRig.TrackedCameraRight :
                prefabRig.TrackedCameraLeft;

            cam.cullingMask = mask;
            trackedCam.gameObject.layer = layer;
            trackedCam.Anchor.gameObject.layer = layer;
            trackedCam.ImagePlane.gameObject.layer = layer;
            trackedCamAnother.ImagePlaneCalibration.gameObject.layer = layer;
        }

        private static bool ModifyLayer(int index, string name)
        {
            if (index < 0 || index > 31) return false;
            if (LayerMask.NameToLayer(name) == index) return true;
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null) return false;
            if (!layers.isArray) return false;
            layers.GetArrayElementAtIndex(index).stringValue = name;
            tagManager.ApplyModifiedProperties();
            return true;
        }
    }
}
#endif