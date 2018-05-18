using System.Collections;
using UnityEngine;
using System.IO;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_3DPreview_Save : ViveSR_Experience_ISubBtn
    {
        private static ViveSR_Experience_SubBtn_3DPreview_Save _instance;
        public static ViveSR_Experience_SubBtn_3DPreview_Save instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_SubBtn_3DPreview_Save>();
                }
                return _instance;
            }
        }

        int percentage = 0;
        int lastPercentage = 0;
        [SerializeField] ViveSR_Experience_Tutorial_InputHandler_3DPreview TutorialInputHandler_3DPreview;

        [SerializeField] _3DPreview_SubBtn SubBtnType;

        bool isSaving;

        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
        }

        public override void ExecuteToDo()
        {
            ViveSR_Experience.RenderButtons(false);
            SubMenu.RenderSubBtns(false);

            ViveSR_RigidReconstruction.ExportAdaptiveMesh = true;
            ViveSR_RigidReconstruction.ExportModel("Model");
                                                                                     
            StartCoroutine(SetPercentage());
        }

        public bool IsSaving()
        {
            return isSaving;
        }

        IEnumerator SetPercentage()
        {
            if(ViveSR_Experience_StaticMesh.instance.texturedMesh != null)
            {
                Destroy(ViveSR_Experience_StaticMesh.instance.collisionMesh);
                Destroy(ViveSR_Experience_StaticMesh.instance.texturedMesh);
            }
          //  string cld_path = ViveSR_Experience_SubBtn_EnableMesh_Static.instance.cld_path;

            isSaving = true;

            while (percentage < 100)
            {
                ViveSR_RigidReconstruction.GetExportProgress(ref percentage);
                ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "[Save Mesh]\n" + percentage + "%", false);
                // wait until saving is really processing then we disable depth
                if (lastPercentage == 0 && percentage > 0)
                    ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(false);
                lastPercentage = percentage;
                yield return new WaitForEndOfFrame();
            }

            ViveSR_Experience_Button_EnableDepth.instance.ForceExcuteButton(true);

            if (ViveSR_Experience_Button_Tutorial.instance.isOn) TutorialInputHandler_3DPreview.MeshSaved();

            ViveSR_Experience.RenderButtons(true);
            SubMenu.RenderSubBtns(true);

            isOn = false;
            percentage = 0;
            lastPercentage = 0;

            //Disable the [Save] button.
            SubMenu.subBtnScripts[ThisButtonTypeNum].isOn = false;
            SubMenu.subBtnScripts[ThisButtonTypeNum].EnableButton(false);

            //Enable the [Scan] button.
            ViveSR_Experience_SubBtn_3DPreview_Scan.instance.ForceExcute(false);
            ViveSR_Experience_SubBtn_3DPreview_Scan.instance.EnableButton(true);

            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "[Save Mesh]\nMesh Saved!", true);

            //[Enable Mesh] is available.
            if (File.Exists("Recons3DAsset/Model.obj"))
                ViveSR_Experience_Button_EnableMesh.instance.EnableButton(true);

            //NewMesh should be loaded after saving
            ViveSR_Experience_StaticMesh.instance.NewMeshSaved();

            isSaving = false;
        }
    }
}