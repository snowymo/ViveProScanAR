using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Effects : MonoBehaviour
    {
        [Header("EffectBalls")]
        public int CurrentEffectNumber = 0;

        [SerializeField] GameObject EffectBall;
        [SerializeField] Renderer EffectballRenderer;
        [SerializeField] List<Texture> EffectImages;

        [Header("Shader")]
        public List<Material> TargetMaterials = new List<Material>();
        [SerializeField] List<Shader> Shaders;
        int ShaderIndex = 0;

        public bool isMaterialSet = false;

        private void Update()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING)
            {
                if (!isMaterialSet)
                {
                    TargetMaterials.Add(ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.UndistortedLeft[0]);
                    TargetMaterials.Add(ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.UndistortedRight[0]);

                    isMaterialSet = true;
                }
            }
        }
  
        public void GenerateDart()
        {
            //Switch effectballs.
            CurrentEffectNumber++;
            if (CurrentEffectNumber > Shaders.Count - 1) CurrentEffectNumber = 1; //avoids 0, the original shader.
            EffectBall.SetActive(true);
            EffectballRenderer.material.mainTexture = EffectImages[CurrentEffectNumber];
        }
        public void ReleaseDart()
        {
            EffectBall.SetActive(false);
        }

        public void ChangeShader(int index)
        {
            try
            {
                ShaderIndex = index % Shaders.Count;
                for (int i = 0; i < TargetMaterials.Count; i++)
                {
                    TargetMaterials[i].shader = Shaders[ShaderIndex];
                }
            }
            catch (System.DivideByZeroException)
            {
                Debug.LogWarning("No shader assigned.");
            }
        }

        private void OnDestroy()
        {
            ChangeShader(0);
        }

        public void ToggleEffects(bool isOn)
        {
            if (!isOn)
            {
                CurrentEffectNumber = 0;
                ChangeShader(0);
            }
        }

    }
}